using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;
using DispatchLogistics.Models;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма справочника транспорта
    /// </summary>
    public partial class TransportForm : Form
    {
        private TransportRepository _repo = new TransportRepository();
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cbFilterStatus;
        private Label lblCount;

        public TransportForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Транспорт";

            // Заголовок
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = Color.White;
            headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Справочник транспорта");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            // Панель инструментов
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 45;
            toolbar.BackColor = Color.White;
            toolbar.Padding = new Padding(10, 5, 10, 5);

            Label lblSearch = UIStyleHelper.CreateLabel("Поиск:");
            lblSearch.Location = new Point(10, 12);
            toolbar.Controls.Add(lblSearch);

            txtSearch = UIStyleHelper.CreateTextBox();
            txtSearch.Location = new Point(60, 10);
            txtSearch.Size = new Size(160, 26);
            txtSearch.TextChanged += (s, e) => LoadData();
            toolbar.Controls.Add(txtSearch);

            Label lblStatus = UIStyleHelper.CreateLabel("Статус:");
            lblStatus.Location = new Point(235, 12);
            toolbar.Controls.Add(lblStatus);

            cbFilterStatus = UIStyleHelper.CreateComboBox();
            cbFilterStatus.Location = new Point(285, 10);
            cbFilterStatus.Size = new Size(130, 26);
            cbFilterStatus.Items.Add("Все");
            cbFilterStatus.Items.Add("Свободен");
            cbFilterStatus.Items.Add("В рейсе");
            cbFilterStatus.Items.Add("На ремонте");
            cbFilterStatus.SelectedIndex = 0;
            cbFilterStatus.SelectedIndexChanged += (s, e) => LoadData();
            toolbar.Controls.Add(cbFilterStatus);

            // Кнопки
            AddButton(toolbar, "➕ Добавить", 430, 8, UIStyleHelper.SuccessColor, BtnAdd_Click);
            AddButton(toolbar, "✏️ Изменить", 560, 8, UIStyleHelper.PrimaryColor, BtnEdit_Click);
            AddButton(toolbar, "🗑️ Удалить", 690, 8, UIStyleHelper.DangerColor, BtnDelete_Click);
            AddButton(toolbar, "🔄 Обновить", 810, 8, Color.FromArgb(130, 130, 130), (s, e) => { txtSearch.Text = ""; cbFilterStatus.SelectedIndex = 0; LoadData(); });

            lblCount = UIStyleHelper.CreateLabel("");
            lblCount.Location = new Point(940, 12);
            toolbar.Controls.Add(lblCount);

            // Таблица
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            UIStyleHelper.StyleDataGridView(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(null, null); };

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
            this.Controls.Add(headerPanel);
            toolbar.BringToFront();
        }

        private void AddButton(Panel parent, string text, int x, int y, Color color, EventHandler click)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(110, 30);
            UIStyleHelper.StyleButton(btn, color);
            btn.Click += click;
            parent.Controls.Add(btn);
        }

        private void LoadData()
        {
            try
            {
                DataTable dt;
                string search = txtSearch.Text.Trim();
                string status = cbFilterStatus.SelectedItem != null ? cbFilterStatus.SelectedItem.ToString() : "Все";

                if (!string.IsNullOrEmpty(search))
                {
                    dt = _repo.SearchTransport(search);
                }
                else if (status != "Все")
                {
                    dt = _repo.GetByStatus(status);
                }
                else
                {
                    dt = _repo.GetAllTransport();
                }

                dgv.DataSource = dt;
                lblCount.Text = string.Format("Записей: {0}", dt.Rows.Count);
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка загрузки:\n{0}", ex.Message));
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            TransportEditForm form = new TransportEditForm();
            form.ShowDialog();
            LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                ValidationHelper.ShowWarning("Выберите транспорт для редактирования.");
                return;
            }
            int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            TransportEditForm form = new TransportEditForm(id);
            form.ShowDialog();
            LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                ValidationHelper.ShowWarning("Выберите транспорт для удаления.");
                return;
            }

            string info = dgv.SelectedRows[0].Cells["Гос. номер"].Value.ToString();
            if (!ValidationHelper.ConfirmDelete(info))
                return;

            try
            {
                int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
                _repo.DeleteTransport(id);
                ValidationHelper.ShowSuccess(string.Format("Транспорт \"{0}\" удалён.", info));
                LoadData();
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка удаления:\n{0}", ex.Message));
            }
        }
    }

    /// <summary>
    /// Форма добавления/редактирования транспорта
    /// </summary>
    public partial class TransportEditForm : Form
    {
        private TransportRepository _repo = new TransportRepository();
        private int _id;
        private bool _isNew;

        private TextBox txtVehicleNum, txtModel, txtBodyType, txtCapacity, txtFuel, txtCostKm, txtIdleCost, txtNotes;
        private ComboBox cbStatus;
        private Button btnSave, btnCancel;

        public TransportEditForm(int id = 0)
        {
            _id = id;
            _isNew = (id == 0);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(520, 530);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = _isNew ? "Новый транспорт" : "Редактирование транспорта";
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblTitle = UIStyleHelper.CreateHeaderLabel(_isNew ? "Новый транспорт" : "Редактирование транспорта");
            lblTitle.Location = new Point(20, 15);

            int x = 20, y = 55, lw = 140, fw = 280, fh = 26, rs = 38;

            AddField(this, "Гос. номер: *", ref txtVehicleNum, x, ref y, lw, fw, fh, rs);
            AddField(this, "Марка/Модель: *", ref txtModel, x, ref y, lw, fw, fh, rs);
            AddField(this, "Тип кузова:", ref txtBodyType, x, ref y, lw, fw, fh, rs);

            // Грузоподъёмность
            Label lblCap = UIStyleHelper.CreateLabel("Грузоподъёмность (т):");
            lblCap.Location = new Point(x, y); lblCap.Width = lw;
            this.Controls.Add(lblCap);
            txtCapacity = UIStyleHelper.CreateTextBox();
            txtCapacity.Location = new Point(x + lw + 10, y - 3);
            txtCapacity.Size = new Size(fw, fh);
            this.Controls.Add(txtCapacity);
            y += rs;

            AddField(this, "Расход л/100км:", ref txtFuel, x, ref y, lw, fw, fh, rs);
            AddField(this, "Стоимость за км:", ref txtCostKm, x, ref y, lw, fw, fh, rs);
            AddField(this, "Стоимость часа простоя:", ref txtIdleCost, x, ref y, lw, fw, fh, rs);

            // Статус
            Label lblSt = UIStyleHelper.CreateLabel("Статус:");
            lblSt.Location = new Point(x, y); lblSt.Width = lw;
            this.Controls.Add(lblSt);
            cbStatus = UIStyleHelper.CreateComboBox();
            cbStatus.Location = new Point(x + lw + 10, y - 3);
            cbStatus.Size = new Size(fw, fh);
            cbStatus.Items.Add("Свободен"); cbStatus.Items.Add("В рейсе"); cbStatus.Items.Add("На ремонте");
            this.Controls.Add(cbStatus);
            y += rs;

            // Примечание
            Label lblN = UIStyleHelper.CreateLabel("Примечание:");
            lblN.Location = new Point(x, y); lblN.Width = lw;
            this.Controls.Add(lblN);
            txtNotes = new TextBox();
            txtNotes.Font = new Font("Segoe UI", 9F);
            txtNotes.BorderStyle = BorderStyle.FixedSingle;
            txtNotes.Multiline = true; txtNotes.Height = 50;
            txtNotes.Location = new Point(x + lw + 10, y - 3);
            txtNotes.Size = new Size(fw, 50);
            this.Controls.Add(txtNotes);
            y += 70;

            btnSave = new Button();
            btnSave.Text = _isNew ? "➕ Добавить" : "💾 Сохранить";
            btnSave.Location = new Point(x + lw + 10, y + 10);
            btnSave.Size = new Size(140, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(x + lw + 160, y + 10);
            btnCancel.Size = new Size(120, 36);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150));
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddField(Form f, string label, ref TextBox txt, int x, ref int y, int lw, int fw, int fh, int rs)
        {
            Label lbl = UIStyleHelper.CreateLabel(label);
            lbl.Location = new Point(x, y); lbl.Width = lw;
            f.Controls.Add(lbl);
            txt = UIStyleHelper.CreateTextBox();
            txt.Location = new Point(x + lw + 10, y - 3);
            txt.Size = new Size(fw, fh);
            f.Controls.Add(txt);
            y += rs;
        }

        private void LoadData()
        {
            if (_isNew) { cbStatus.SelectedIndex = 0; return; }
            try
            {
                var t = _repo.GetById(_id);
                if (t == null) { ValidationHelper.ShowError("Транспорт не найден."); this.Close(); return; }
                txtVehicleNum.Text = t.VehicleNumber;
                txtModel.Text = t.Model;
                txtBodyType.Text = t.BodyType;
                txtCapacity.Text = t.CapacityTons.ToString();
                txtFuel.Text = t.FuelConsumption.ToString();
                txtCostKm.Text = t.CostPerKm.ToString();
                txtIdleCost.Text = t.IdleHourCost.ToString();
                cbStatus.SelectedItem = t.Status;
                txtNotes.Text = t.Notes ?? "";
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsNotEmpty(txtVehicleNum, "Гос. номер")) return;
            if (!ValidationHelper.IsNotEmpty(txtModel, "Марка/Модель")) return;

            try
            {
                var t = new TransportModel();
                t.TransportId = _id;
                t.VehicleNumber = txtVehicleNum.Text.Trim();
                t.Model = txtModel.Text.Trim();
                t.BodyType = string.IsNullOrWhiteSpace(txtBodyType.Text) ? "Тент" : txtBodyType.Text.Trim();
                t.CapacityTons = string.IsNullOrWhiteSpace(txtCapacity.Text) ? 0 : decimal.Parse(txtCapacity.Text);
                t.FuelConsumption = string.IsNullOrWhiteSpace(txtFuel.Text) ? 0 : decimal.Parse(txtFuel.Text);
                t.CostPerKm = string.IsNullOrWhiteSpace(txtCostKm.Text) ? 0 : decimal.Parse(txtCostKm.Text);
                t.IdleHourCost = string.IsNullOrWhiteSpace(txtIdleCost.Text) ? 0 : decimal.Parse(txtIdleCost.Text);
                t.Status = cbStatus.SelectedItem != null ? cbStatus.SelectedItem.ToString() : "Свободен";
                t.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                if (_isNew) _repo.InsertTransport(t);
                else _repo.UpdateTransport(t);

                ValidationHelper.ShowSuccess(_isNew ? "Транспорт добавлен." : "Транспорт обновлён.");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка сохранения:\n" + ex.Message); }
        }
    }
}
