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
    /// Форма справочника дополнительных услуг
    /// </summary>
    public partial class AdditionalServicesForm : Form
    {
        private AdditionalServiceRepository _repo = new AdditionalServiceRepository();
        private DataGridView dgv;
        private Label lblCount;

        public AdditionalServicesForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Дополнительные услуги";

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top; headerPanel.Height = 60;
            headerPanel.BackColor = Color.White; headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Дополнительные услуги");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top; toolbar.Height = 45;
            toolbar.BackColor = Color.White; toolbar.Padding = new Padding(10, 5, 10, 5);

            AddBtn(toolbar, "➕ Добавить", 10, 8, UIStyleHelper.SuccessColor, BtnAdd_Click);
            AddBtn(toolbar, "✏️ Изменить", 140, 8, UIStyleHelper.PrimaryColor, BtnEdit_Click);
            AddBtn(toolbar, "🗑️ Удалить", 270, 8, UIStyleHelper.DangerColor, BtnDelete_Click);
            AddBtn(toolbar, "🔄 Обновить", 400, 8, Color.FromArgb(130, 130, 130), (s, e) => LoadData());

            lblCount = UIStyleHelper.CreateLabel("");
            lblCount.Location = new Point(530, 12);
            toolbar.Controls.Add(lblCount);

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            UIStyleHelper.StyleDataGridView(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(null, null); };

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
            this.Controls.Add(headerPanel);
            toolbar.BringToFront();
        }

        private void AddBtn(Panel p, string t, int x, int y, Color c, EventHandler click)
        {
            Button btn = new Button(); btn.Text = t; btn.Location = new Point(x, y); btn.Size = new Size(115, 30);
            UIStyleHelper.StyleButton(btn, c); btn.Click += click; p.Controls.Add(btn);
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = _repo.GetAllServices();
                dgv.DataSource = dt;
                lblCount.Text = string.Format("Записей: {0}", dt.Rows.Count);
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new AdditionalServiceEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите услугу."); return; }
            int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            var f = new AdditionalServiceEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите услугу."); return; }
            string name = dgv.SelectedRows[0].Cells["Название"].Value.ToString();
            if (!ValidationHelper.ConfirmDelete(name)) return;
            try { int id = (int)dgv.SelectedRows[0].Cells["ID"].Value; _repo.DeleteService(id); ValidationHelper.ShowSuccess("Услуга удалена."); LoadData(); }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }

    /// <summary>
    /// Форма добавления/редактирования доп. услуги
    /// </summary>
    public partial class AdditionalServiceEditForm : Form
    {
        private AdditionalServiceRepository _repo = new AdditionalServiceRepository();
        private int _id;
        private bool _isNew;
        private TextBox txtName, txtPrice, txtUnit;
        private ComboBox cbChargeType;
        private CheckBox chkActive;
        private Button btnSave, btnCancel;

        public AdditionalServiceEditForm(int id = 0) { _id = id; _isNew = (id == 0); InitializeComponent(); LoadData(); }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(480, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = _isNew ? "Новая услуга" : "Редактирование услуги";
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblT = UIStyleHelper.CreateHeaderLabel(_isNew ? "Новая услуга" : "Редактирование услуги");
            lblT.Location = new Point(20, 15);

            int x = 20, y = 55, lw = 120, fw = 270, fh = 26, rs = 38;

            AddF("Название: *", ref txtName, x, ref y, lw, fw, fh, rs);
            AddF("Цена (руб.): *", ref txtPrice, x, ref y, lw, fw, fh, rs);

            Label lbl = UIStyleHelper.CreateLabel("Тип начисления:"); lbl.Location = new Point(x, y); lbl.Width = lw; this.Controls.Add(lbl);
            cbChargeType = UIStyleHelper.CreateComboBox(); cbChargeType.Location = new Point(x + lw + 10, y - 3); cbChargeType.Size = new Size(fw, fh);
            cbChargeType.Items.Add("Фиксированная"); cbChargeType.Items.Add("За единицу"); this.Controls.Add(cbChargeType);
            y += rs;

            AddF("Ед. измерения:", ref txtUnit, x, ref y, lw, fw, fh, rs);

            chkActive = new CheckBox(); chkActive.Text = "Активна"; chkActive.AutoCheck = true; chkActive.Checked = true;
            chkActive.Location = new Point(x + lw + 10, y - 3); this.Controls.Add(chkActive);
            y += rs + 15;

            btnSave = new Button(); btnSave.Text = _isNew ? "➕ Добавить" : "💾 Сохранить";
            btnSave.Location = new Point(x + lw + 10, y); btnSave.Size = new Size(130, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor); btnSave.Click += BtnSave_Click; this.Controls.Add(btnSave);

            btnCancel = new Button(); btnCancel.Text = "Отмена"; btnCancel.Location = new Point(x + lw + 150, y); btnCancel.Size = new Size(120, 36);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150)); btnCancel.Click += (s, e) => this.Close(); this.Controls.Add(btnCancel);
        }

        private void AddF(string label, ref TextBox txt, int x, ref int y, int lw, int fw, int fh, int rs)
        {
            Label lbl = UIStyleHelper.CreateLabel(label); lbl.Location = new Point(x, y); lbl.Width = lw; this.Controls.Add(lbl);
            txt = UIStyleHelper.CreateTextBox(); txt.Location = new Point(x + lw + 10, y - 3); txt.Size = new Size(fw, fh); this.Controls.Add(txt);
            y += rs;
        }

        private void LoadData()
        {
            if (_isNew) { cbChargeType.SelectedIndex = 0; return; }
            try
            {
                var s = _repo.GetById(_id);
                if (s == null) { ValidationHelper.ShowError("Не найдено."); this.Close(); return; }
                txtName.Text = s.ServiceName; txtPrice.Text = s.Price.ToString();
                cbChargeType.SelectedItem = s.ChargeType; txtUnit.Text = s.UnitName ?? "";
                chkActive.Checked = s.IsActive;
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsNotEmpty(txtName, "Название")) return;
            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price) || price < 0) { ValidationHelper.ShowWarning("Введите корректную цену."); return; }

            try
            {
                var s = new AdditionalServiceModel();
                s.ServiceId = _id; s.ServiceName = txtName.Text.Trim(); s.Price = price;
                s.ChargeType = cbChargeType.SelectedItem != null ? cbChargeType.SelectedItem.ToString() : "Фиксированная";
                s.UnitName = string.IsNullOrWhiteSpace(txtUnit.Text) ? null : txtUnit.Text.Trim();
                s.IsActive = chkActive.Checked;

                if (_isNew) _repo.InsertService(s); else _repo.UpdateService(s);
                ValidationHelper.ShowSuccess(_isNew ? "Услуга добавлена." : "Услуга обновлена.");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }
}
