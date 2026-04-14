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
    /// Форма геоточек и расстояний (с вкладками)
    /// </summary>
    public partial class GeoPointsForm : Form
    {
        private GeoPointRepository _geoRepo = new GeoPointRepository();
        private DistanceRepository _distRepo = new DistanceRepository();
        private TabControl tabControl;

        public GeoPointsForm()
        {
            InitializeComponent();
            LoadGeoPoints();
            LoadDistances();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Геоточки и расстояния";

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top; headerPanel.Height = 60;
            headerPanel.BackColor = Color.White; headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Геоточки и расстояния");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F);

            // Вкладка геоточек
            TabPage tabGeo = new TabPage("Геоточки");
            tabGeo.BackColor = UIStyleHelper.BackgroundColor;

            Panel toolbarGeo = new Panel();
            toolbarGeo.Dock = DockStyle.Top; toolbarGeo.Height = 45;
            toolbarGeo.BackColor = Color.White; toolbarGeo.Padding = new Padding(10, 5, 10, 5);

            AddButton(toolbarGeo, "➕ Добавить точку", 10, 8, UIStyleHelper.SuccessColor, BtnAddGeo_Click);
            AddButton(toolbarGeo, "✏️ Изменить", 170, 8, UIStyleHelper.PrimaryColor, BtnEditGeo_Click);
            AddButton(toolbarGeo, "🗑️ Удалить", 300, 8, UIStyleHelper.DangerColor, BtnDeleteGeo_Click);
            AddButton(toolbarGeo, "🔄 Обновить", 430, 8, Color.FromArgb(130, 130, 130), (s, e) => { LoadGeoPoints(); LoadDistances(); });

            DataGridView dgvGeo = new DataGridView();
            dgvGeo.Name = "dgvGeo";
            dgvGeo.Dock = DockStyle.Fill;
            UIStyleHelper.StyleDataGridView(dgvGeo);

            tabGeo.Controls.Add(dgvGeo);
            tabGeo.Controls.Add(toolbarGeo);

            // Вкладка расстояний
            TabPage tabDist = new TabPage("Расстояния");
            tabDist.BackColor = UIStyleHelper.BackgroundColor;

            Panel toolbarDist = new Panel();
            toolbarDist.Dock = DockStyle.Top; toolbarDist.Height = 45;
            toolbarDist.BackColor = Color.White; toolbarDist.Padding = new Padding(10, 5, 10, 5);

            AddButton(toolbarDist, "➕ Добавить расстояние", 10, 8, UIStyleHelper.SuccessColor, BtnAddDist_Click);
            AddButton(toolbarDist, "✏️ Изменить", 190, 8, UIStyleHelper.PrimaryColor, BtnEditDist_Click);
            AddButton(toolbarDist, "🗑️ Удалить", 320, 8, UIStyleHelper.DangerColor, BtnDeleteDist_Click);
            AddButton(toolbarDist, "🔄 Обновить", 450, 8, Color.FromArgb(130, 130, 130), (s, e) => { LoadDistances(); LoadGeoPoints(); });

            DataGridView dgvDist = new DataGridView();
            dgvDist.Name = "dgvDist";
            dgvDist.Dock = DockStyle.Fill;
            UIStyleHelper.StyleDataGridView(dgvDist);

            tabDist.Controls.Add(dgvDist);
            tabDist.Controls.Add(toolbarDist);

            tabControl.TabPages.Add(tabGeo);
            tabControl.TabPages.Add(tabDist);

            this.Controls.Add(tabControl);
            this.Controls.Add(headerPanel);
        }

        private void AddButton(Panel p, string text, int x, int y, Color c, EventHandler click)
        {
            Button btn = new Button();
            btn.Text = text; btn.Location = new Point(x, y); btn.Size = new Size(150, 30);
            UIStyleHelper.StyleButton(btn, c); btn.Click += click;
            p.Controls.Add(btn);
        }

        private DataGridView DgvGeo { get { return tabControl.TabPages[0].Controls["dgvGeo"] as DataGridView; } }
        private DataGridView DgvDist { get { return tabControl.TabPages[1].Controls["dgvDist"] as DataGridView; } }

        private void LoadGeoPoints()
        {
            try { DgvGeo.DataSource = _geoRepo.GetAllGeoPoints(); }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void LoadDistances()
        {
            try { DgvDist.DataSource = _distRepo.GetAllDistances(); }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnAddGeo_Click(object sender, EventArgs e)
        {
            var f = new GeoPointEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadGeoPoints();
        }

        private void BtnEditGeo_Click(object sender, EventArgs e)
        {
            if (DgvGeo.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите точку."); return; }
            int id = (int)DgvGeo.SelectedRows[0].Cells["ID"].Value;
            var f = new GeoPointEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadGeoPoints();
        }

        private void BtnDeleteGeo_Click(object sender, EventArgs e)
        {
            if (DgvGeo.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите точку."); return; }
            string name = DgvGeo.SelectedRows[0].Cells["Название"].Value.ToString();
            if (!ValidationHelper.ConfirmDelete(name)) return;
            try { int id = (int)DgvGeo.SelectedRows[0].Cells["ID"].Value; _geoRepo.DeleteGeoPoint(id); ValidationHelper.ShowSuccess("Точка удалена."); LoadGeoPoints(); }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnAddDist_Click(object sender, EventArgs e)
        {
            var f = new DistanceEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadDistances();
        }

        private void BtnEditDist_Click(object sender, EventArgs e)
        {
            if (DgvDist.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите расстояние."); return; }
            // Для упрощения — удаляем и создаём заново
            ValidationHelper.ShowWarning("Для изменения удалите и создайте новую запись.");
        }

        private void BtnDeleteDist_Click(object sender, EventArgs e)
        {
            if (DgvDist.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите расстояние."); return; }
            if (!ValidationHelper.ConfirmDelete("расстояние")) return;
            try { int id = (int)DgvDist.SelectedRows[0].Cells["ID"].Value; _distRepo.DeleteDistance(id); ValidationHelper.ShowSuccess("Расстояние удалено."); LoadDistances(); }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }

    /// <summary>
    /// Форма редактирования геоточки
    /// </summary>
    public partial class GeoPointEditForm : Form
    {
        private GeoPointRepository _repo = new GeoPointRepository();
        private int _id;
        private bool _isNew;
        private TextBox txtName, txtRegion, txtLat, txtLng, txtNotes;
        private Button btnSave, btnCancel;

        public GeoPointEditForm(int id = 0) { _id = id; _isNew = (id == 0); InitializeComponent(); LoadData(); }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(450, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = _isNew ? "Новая геоточка" : "Редактирование геоточки";
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblT = UIStyleHelper.CreateHeaderLabel(_isNew ? "Новая геоточка" : "Редактирование");
            lblT.Location = new Point(20, 15);

            int x = 20, y = 55, lw = 110, fw = 260, fh = 26, rs = 38;

            AddF("Название: *", ref txtName, x, ref y, lw, fw, fh, rs);
            AddF("Регион:", ref txtRegion, x, ref y, lw, fw, fh, rs);
            AddF("Широта:", ref txtLat, x, ref y, lw, fw, fh, rs);
            AddF("Долгота:", ref txtLng, x, ref y, lw, fw, fh, rs);
            AddF("Примечание:", ref txtNotes, x, ref y, lw, fw, 50, 70);
            txtNotes.Multiline = true;

            btnSave = new Button(); btnSave.Text = _isNew ? "➕ Добавить" : "💾 Сохранить";
            btnSave.Location = new Point(x + lw + 10, y + 15); btnSave.Size = new Size(130, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor); btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button(); btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(x + lw + 150, y + 15); btnCancel.Size = new Size(110, 36);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150));
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddF(string label, ref TextBox txt, int x, ref int y, int lw, int fw, int fh, int rs)
        {
            Label lbl = UIStyleHelper.CreateLabel(label); lbl.Location = new Point(x, y); lbl.Width = lw; this.Controls.Add(lbl);
            txt = UIStyleHelper.CreateTextBox(); txt.Location = new Point(x + lw + 10, y - 3); txt.Size = new Size(fw, fh); this.Controls.Add(txt);
            y += rs;
        }

        private void LoadData()
        {
            if (_isNew) return;
            var gp = _repo.GetById(_id);
            if (gp == null) { ValidationHelper.ShowError("Не найдено."); this.Close(); return; }
            txtName.Text = gp.PointName; txtRegion.Text = gp.Region ?? "";
            txtLat.Text = gp.Latitude.HasValue ? gp.Latitude.Value.ToString() : "";
            txtLng.Text = gp.Longitude.HasValue ? gp.Longitude.Value.ToString() : "";
            txtNotes.Text = gp.Notes ?? "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsNotEmpty(txtName, "Название")) return;
            try
            {
                var gp = new GeoPointModel();
                gp.GeoPointId = _id; gp.PointName = txtName.Text.Trim();
                gp.Region = string.IsNullOrWhiteSpace(txtRegion.Text) ? null : txtRegion.Text.Trim();
                gp.Latitude = ParseDec(txtLat.Text); gp.Longitude = ParseDec(txtLng.Text);
                gp.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                if (_isNew) _repo.InsertGeoPoint(gp); else _repo.UpdateGeoPoint(gp);
                ValidationHelper.ShowSuccess(_isNew ? "Точка добавлена." : "Точка обновлена.");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private decimal? ParseDec(string s) { if (string.IsNullOrWhiteSpace(s)) return null; decimal v; return decimal.TryParse(s, out v) ? v : (decimal?)null; }
    }

    /// <summary>
    /// Форма добавления расстояния
    /// </summary>
    public partial class DistanceEditForm : Form
    {
        private GeoPointRepository _geoRepo = new GeoPointRepository();
        private DistanceRepository _distRepo = new DistanceRepository();
        private ComboBox cbFrom, cbTo;
        private TextBox txtDist;
        private Button btnSave, btnCancel;

        public DistanceEditForm() { InitializeComponent(); LoadPoints(); }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(420, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Новое расстояние";
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblT = UIStyleHelper.CreateHeaderLabel("Новое расстояние");
            lblT.Location = new Point(20, 15);

            int x = 20, y = 55, lw = 100, fw = 250, fh = 26, rs = 40;

            Label lbl1 = UIStyleHelper.CreateLabel("Откуда:"); lbl1.Location = new Point(x, y); lbl1.Width = lw; this.Controls.Add(lbl1);
            cbFrom = UIStyleHelper.CreateComboBox(); cbFrom.Location = new Point(x + lw + 10, y - 3); cbFrom.Size = new Size(fw, fh); this.Controls.Add(cbFrom);
            y += rs;

            Label lbl2 = UIStyleHelper.CreateLabel("Куда:"); lbl2.Location = new Point(x, y); lbl2.Width = lw; this.Controls.Add(lbl2);
            cbTo = UIStyleHelper.CreateComboBox(); cbTo.Location = new Point(x + lw + 10, y - 3); cbTo.Size = new Size(fw, fh); this.Controls.Add(cbTo);
            y += rs;

            Label lbl3 = UIStyleHelper.CreateLabel("Расстояние (км):"); lbl3.Location = new Point(x, y); lbl3.Width = lw; this.Controls.Add(lbl3);
            txtDist = UIStyleHelper.CreateTextBox(); txtDist.Location = new Point(x + lw + 10, y - 3); txtDist.Size = new Size(fw, fh); this.Controls.Add(txtDist);
            y += rs + 10;

            btnSave = new Button(); btnSave.Text = "➕ Добавить"; btnSave.Location = new Point(x + lw + 10, y); btnSave.Size = new Size(130, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor); btnSave.Click += BtnSave_Click; this.Controls.Add(btnSave);

            btnCancel = new Button(); btnCancel.Text = "Отмена"; btnCancel.Location = new Point(x + lw + 150, y); btnCancel.Size = new Size(110, 36);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150)); btnCancel.Click += (s, e) => this.Close(); this.Controls.Add(btnCancel);
        }

        private void LoadPoints()
        {
            DataTable dt = _geoRepo.GetAllForCombo();
            cbFrom.DataSource = dt.Copy(); cbFrom.DisplayMember = "PointName"; cbFrom.ValueMember = "GeoPointId";
            cbTo.DataSource = dt.Copy(); cbTo.DisplayMember = "PointName"; cbTo.ValueMember = "GeoPointId";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbFrom.SelectedItem == null || cbTo.SelectedItem == null) { ValidationHelper.ShowWarning("Выберите точки."); return; }
            if ((int)cbFrom.SelectedValue == (int)cbTo.SelectedValue) { ValidationHelper.ShowWarning("Точки должны быть разными."); return; }

            decimal dist;
            if (!decimal.TryParse(txtDist.Text, out dist) || dist <= 0) { ValidationHelper.ShowWarning("Введите корректное расстояние."); return; }

            try
            {
                // Проверяем, нет ли уже такого расстояния
                var existing = _distRepo.GetDistanceBetween((int)cbFrom.SelectedValue, (int)cbTo.SelectedValue);
                if (existing.HasValue) { ValidationHelper.ShowWarning("Расстояние между этими точками уже существует."); return; }

                _distRepo.InsertDistance((int)cbFrom.SelectedValue, (int)cbTo.SelectedValue, dist);
                ValidationHelper.ShowSuccess("Расстояние добавлено.");
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }
}
