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
    /// Форма справочника тарифов
    /// </summary>
    public partial class TariffsForm : Form
    {
        private TariffRepository _repo = new TariffRepository();
        private DataGridView dgv;
        private Label lblCount;

        public TariffsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Тарифы";

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top; headerPanel.Height = 60;
            headerPanel.BackColor = Color.White; headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Справочник тарифов");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top; toolbar.Height = 45;
            toolbar.BackColor = Color.White; toolbar.Padding = new Padding(10, 5, 10, 5);

            AddButton(toolbar, "➕ Добавить", 10, 8, UIStyleHelper.SuccessColor, BtnAdd_Click);
            AddButton(toolbar, "✏️ Изменить", 140, 8, UIStyleHelper.PrimaryColor, BtnEdit_Click);
            AddButton(toolbar, "🗑️ Удалить", 270, 8, UIStyleHelper.DangerColor, BtnDelete_Click);
            AddButton(toolbar, "🔄 Обновить", 400, 8, Color.FromArgb(130, 130, 130), (s, e) => LoadData());

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
        }

        private void AddButton(Panel p, string text, int x, int y, Color c, EventHandler click)
        {
            Button btn = new Button();
            btn.Text = text; btn.Location = new Point(x, y); btn.Size = new Size(115, 30);
            UIStyleHelper.StyleButton(btn, c); btn.Click += click;
            p.Controls.Add(btn);
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = _repo.GetAllTariffs();
                dgv.DataSource = dt;
                lblCount.Text = string.Format("Записей: {0}", dt.Rows.Count);
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new TariffEditForm(); f.ShowDialog(); LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите тариф."); return; }
            int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            var f = new TariffEditForm(id); f.ShowDialog(); LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите тариф."); return; }
            string name = dgv.SelectedRows[0].Cells["Название"].Value.ToString();
            if (!ValidationHelper.ConfirmDelete(name)) return;
            try
            {
                int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
                _repo.DeleteTariff(id);
                ValidationHelper.ShowSuccess(string.Format("Тариф \"{0}\" удалён.", name));
                LoadData();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }

    /// <summary>
    /// Форма добавления/редактирования тарифа
    /// </summary>
    public partial class TariffEditForm : Form
    {
        private TariffRepository _repo = new TariffRepository();
        private int _id;
        private bool _isNew;

        private TextBox txtName, txtCostKm, txtCostHour, txtCostTon, txtFuel, txtSeasonal, txtNotes;
        private ComboBox cbCalcType;
        private CheckBox chkActive;
        private Button btnSave, btnCancel;

        public TariffEditForm(int id = 0)
        {
            _id = id; _isNew = (id == 0);
            InitializeComponent(); LoadData();
        }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(520, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = _isNew ? "Новый тариф" : "Редактирование тарифа";
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblTitle = UIStyleHelper.CreateHeaderLabel(_isNew ? "Новый тариф" : "Редактирование тарифа");
            lblTitle.Location = new Point(20, 15);

            int x = 20, y = 55, lw = 140, fw = 280, fh = 26, rs = 38;

            AddField("Название тарифа: *", ref txtName, x, ref y, lw, fw, fh, rs);

            // Тип расчёта
            Label lbl = UIStyleHelper.CreateLabel("Тип расчёта:");
            lbl.Location = new Point(x, y); lbl.Width = lw; this.Controls.Add(lbl);
            cbCalcType = UIStyleHelper.CreateComboBox();
            cbCalcType.Location = new Point(x + lw + 10, y - 3);
            cbCalcType.Size = new Size(fw, fh);
            cbCalcType.Items.Add("За км"); cbCalcType.Items.Add("За час");
            cbCalcType.Items.Add("За тонну"); cbCalcType.Items.Add("Смешанный");
            this.Controls.Add(cbCalcType);
            y += rs;

            AddField("Стоимость за км:", ref txtCostKm, x, ref y, lw, fw, fh, rs);
            AddField("Стоимость за час:", ref txtCostHour, x, ref y, lw, fw, fh, rs);
            AddField("Стоимость за тонну:", ref txtCostTon, x, ref y, lw, fw, fh, rs);
            AddField("Топливный сбор:", ref txtFuel, x, ref y, lw, fw, fh, rs);
            AddField("Сезонный коэффициент:", ref txtSeasonal, x, ref y, lw, fw, fh, rs);

            // Активен
            chkActive = new CheckBox();
            chkActive.Text = "Активный тариф";
            chkActive.Font = new Font("Segoe UI", 9F);
            chkActive.Location = new Point(x + lw + 10, y - 3);
            chkActive.AutoCheck = true;
            chkActive.Checked = true;
            this.Controls.Add(chkActive);
            y += rs;

            AddField("Примечание:", ref txtNotes, x, ref y, lw, fw, 50, 70);
            txtNotes.Multiline = true; txtNotes.ScrollBars = ScrollBars.Vertical;

            btnSave = new Button();
            btnSave.Text = _isNew ? "➕ Добавить" : "💾 Сохранить";
            btnSave.Location = new Point(x + lw + 10, y + 15);
            btnSave.Size = new Size(140, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(x + lw + 160, y + 15);
            btnCancel.Size = new Size(120, 36);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150));
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddField(string label, ref TextBox txt, int x, ref int y, int lw, int fw, int fh, int rs)
        {
            Label lbl = UIStyleHelper.CreateLabel(label);
            lbl.Location = new Point(x, y); lbl.Width = lw; this.Controls.Add(lbl);
            txt = UIStyleHelper.CreateTextBox();
            txt.Location = new Point(x + lw + 10, y - 3);
            txt.Size = new Size(fw, fh);
            this.Controls.Add(txt);
            y += rs;
        }

        private void LoadData()
        {
            if (_isNew) { cbCalcType.SelectedIndex = 3; txtSeasonal.Text = "1.00"; return; }
            try
            {
                var t = _repo.GetById(_id);
                if (t == null) { ValidationHelper.ShowError("Тариф не найден."); this.Close(); return; }
                txtName.Text = t.TariffName;
                cbCalcType.SelectedItem = t.CalculationType;
                txtCostKm.Text = t.CostPerKm.HasValue ? t.CostPerKm.Value.ToString() : "";
                txtCostHour.Text = t.CostPerHour.HasValue ? t.CostPerHour.Value.ToString() : "";
                txtCostTon.Text = t.CostPerTon.HasValue ? t.CostPerTon.Value.ToString() : "";
                txtFuel.Text = t.FuelSurcharge.HasValue ? t.FuelSurcharge.Value.ToString() : "";
                txtSeasonal.Text = t.SeasonalCoefficient.ToString();
                chkActive.Checked = t.IsActive;
                txtNotes.Text = t.Notes ?? "";
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsNotEmpty(txtName, "Название тарифа")) return;

            try
            {
                var t = new TariffModel();
                t.TariffId = _id;
                t.TariffName = txtName.Text.Trim();
                t.CalculationType = cbCalcType.SelectedItem != null ? cbCalcType.SelectedItem.ToString() : "Смешанный";
                t.CostPerKm = ParseDecimal(txtCostKm.Text);
                t.CostPerHour = ParseDecimal(txtCostHour.Text);
                t.CostPerTon = ParseDecimal(txtCostTon.Text);
                t.FuelSurcharge = ParseDecimal(txtFuel.Text);
                t.SeasonalCoefficient = string.IsNullOrWhiteSpace(txtSeasonal.Text) ? 1.00m : decimal.Parse(txtSeasonal.Text);
                t.IsActive = chkActive.Checked;
                t.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                if (_isNew) _repo.InsertTariff(t);
                else _repo.UpdateTariff(t);

                ValidationHelper.ShowSuccess(_isNew ? "Тариф добавлен." : "Тариф обновлён.");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private decimal? ParseDecimal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            decimal val;
            return decimal.TryParse(s, out val) ? val : (decimal?)null;
        }
    }
}
