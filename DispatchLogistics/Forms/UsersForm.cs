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
    /// Форма управления пользователями (только для администратора)
    /// </summary>
    public partial class UsersForm : Form
    {
        private UserRepository _repo = new UserRepository();
        private DataGridView dgv;
        private Label lblCount;

        public UsersForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Пользователи";

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top; headerPanel.Height = 60;
            headerPanel.BackColor = Color.White; headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Управление пользователями");
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
        }

        private void AddBtn(Panel p, string text, int x, int y, Color c, EventHandler click)
        {
            Button btn = new Button(); btn.Text = text; btn.Location = new Point(x, y); btn.Size = new Size(115, 30);
            UIStyleHelper.StyleButton(btn, c); btn.Click += click; p.Controls.Add(btn);
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = _repo.GetAllUsers();
                dgv.DataSource = dt;
                lblCount.Text = string.Format("Пользователей: {0}", dt.Rows.Count);
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new UserEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите пользователя."); return; }
            int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            var f = new UserEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите пользователя."); return; }

            int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            string login = dgv.SelectedRows[0].Cells["Логин"].Value.ToString();

            // Нельзя удалить самого себя
            if (id == SessionHelper.CurrentUser.UserId)
            {
                ValidationHelper.ShowWarning("Вы не можете удалить свой аккаунт.");
                return;
            }

            if (!ValidationHelper.ConfirmDelete(login)) return;

            try
            {
                _repo.DeleteUser(id);
                ValidationHelper.ShowSuccess(string.Format("Пользователь \"{0}\" удалён.", login));
                LoadData();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }

    /// <summary>
    /// Форма добавления/редактирования пользователя
    /// </summary>
    public partial class UserEditForm : Form
    {
        private UserRepository _repo = new UserRepository();
        private int _id;
        private bool _isNew;
        private TextBox txtLogin, txtPassword, txtFullName;
        private ComboBox cbRole;
        private CheckBox chkActive;
        private Button btnSave, btnCancel;

        public UserEditForm(int id = 0) { _id = id; _isNew = (id == 0); InitializeComponent(); LoadData(); }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(420, 340);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = _isNew ? "Новый пользователь" : "Редактирование пользователя";
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblT = UIStyleHelper.CreateHeaderLabel(_isNew ? "Новый пользователь" : "Редактирование");
            lblT.Location = new Point(20, 15);

            int x = 20, y = 55, lw = 100, fw = 250, fh = 26, rs = 38;

            AddF("Логин: *", ref txtLogin, x, ref y, lw, fw, fh, rs);
            AddF("Пароль: *", ref txtPassword, x, ref y, lw, fw, fh, rs);
            txtPassword.PasswordChar = '●';
            AddF("ФИО: *", ref txtFullName, x, ref y, lw, fw, fh, rs);

            Label lbl = UIStyleHelper.CreateLabel("Роль:"); lbl.Location = new Point(x, y); lbl.Width = lw; this.Controls.Add(lbl);
            cbRole = UIStyleHelper.CreateComboBox(); cbRole.Location = new Point(x + lw + 10, y - 3); cbRole.Size = new Size(fw, fh);
            cbRole.Items.Add("Администратор"); cbRole.Items.Add("Диспетчер"); this.Controls.Add(cbRole);
            y += rs;

            chkActive = new CheckBox(); chkActive.Text = "Активен"; chkActive.AutoCheck = true; chkActive.Checked = true;
            chkActive.Font = new Font("Segoe UI", 9F); chkActive.Location = new Point(x + lw + 10, y - 3);
            this.Controls.Add(chkActive);
            y += rs + 15;

            btnSave = new Button(); btnSave.Text = _isNew ? "➕ Добавить" : "💾 Сохранить";
            btnSave.Location = new Point(x + lw + 10, y); btnSave.Size = new Size(130, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor); btnSave.Click += BtnSave_Click; this.Controls.Add(btnSave);

            btnCancel = new Button(); btnCancel.Text = "Отмена"; btnCancel.Location = new Point(x + lw + 150, y); btnCancel.Size = new Size(110, 36);
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
            if (_isNew) { cbRole.SelectedIndex = 1; return; }
            try
            {
                DataTable dt = _repo.GetAllUsersFull();
                foreach (DataRow row in dt.Rows)
                {
                    if ((int)row["UserId"] == _id)
                    {
                        txtLogin.Text = row["Login"].ToString();
                        txtPassword.Text = row["PasswordHash"].ToString();
                        txtFullName.Text = row["FullName"].ToString();
                        cbRole.SelectedItem = row["Role"].ToString();
                        chkActive.Checked = (bool)row["IsActive"];
                        break;
                    }
                }
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsNotEmpty(txtLogin, "Логин")) return;
            if (!ValidationHelper.IsNotEmpty(txtPassword, "Пароль")) return;
            if (!ValidationHelper.IsNotEmpty(txtFullName, "ФИО")) return;

            try
            {
                if (_isNew)
                {
                    _repo.InsertUser(txtLogin.Text.Trim(), txtPassword.Text, txtFullName.Text.Trim(),
                        cbRole.SelectedItem != null ? cbRole.SelectedItem.ToString() : "Диспетчер", chkActive.Checked);
                    ValidationHelper.ShowSuccess("Пользователь добавлен.");
                }
                else
                {
                    _repo.UpdateUser(_id, txtLogin.Text.Trim(), txtPassword.Text, txtFullName.Text.Trim(),
                        cbRole.SelectedItem != null ? cbRole.SelectedItem.ToString() : "Диспетчер", chkActive.Checked);
                    ValidationHelper.ShowSuccess("Пользователь обновлён.");
                }
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }
    }
}
