using System;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;
using DispatchLogistics.Models;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма авторизации пользователя
    /// </summary>
    public partial class LoginForm : Form
    {
        private UserRepository _userRepository = new UserRepository();

        // Элементы управления
        private Label lblTitle;
        private Label lblLogin;
        private Label lblPassword;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private Panel panelMain;
        private Panel panelHeader;
        private Label lblIcon;

        public LoginForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основная панель
            panelMain = new Panel();
            panelMain.BackColor = Color.White;
            panelMain.Dock = DockStyle.Fill;
            panelMain.Padding = new Padding(40);

            // Верхняя панель-заголовок
            panelHeader = new Panel();
            panelHeader.BackColor = UIStyleHelper.PrimaryColor;
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 80;

            // Иконка
            lblIcon = new Label();
            lblIcon.Text = "🚚";
            lblIcon.Font = new Font("Segoe UI", 28F);
            lblIcon.ForeColor = Color.White;
            lblIcon.AutoSize = true;
            lblIcon.Location = new Point(40, 18);

            // Заголовок
            lblTitle = new Label();
            lblTitle.Text = "Диспетчерская логистика";
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(90, 14);

            // Подзаголовок
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Заказы и тарифы — Вход в систему";
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(180, 210, 235);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(90, 44);

            panelHeader.Controls.Add(lblIcon);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Метка "Логин"
            lblLogin = UIStyleHelper.CreateLabel("Логин:");
            lblLogin.Location = new Point(40, 110);

            // Поле логина
            txtLogin = UIStyleHelper.CreateTextBox();
            txtLogin.Location = new Point(40, 135);
            txtLogin.Size = new Size(300, 30);
            txtLogin.Font = new Font("Segoe UI", 10F);

            // Метка "Пароль"
            lblPassword = UIStyleHelper.CreateLabel("Пароль:");
            lblPassword.Location = new Point(40, 180);

            // Поле пароля
            txtPassword = UIStyleHelper.CreateTextBox();
            txtPassword.Location = new Point(40, 205);
            txtPassword.Size = new Size(300, 30);
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.PasswordChar = '●';

            // Кнопка "Войти"
            btnLogin = new Button();
            btnLogin.Text = "Войти";
            btnLogin.Location = new Point(40, 260);
            btnLogin.Size = new Size(140, 38);
            UIStyleHelper.StyleButton(btnLogin, UIStyleHelper.PrimaryColor);

            // Кнопка "Выход"
            btnExit = new Button();
            btnExit.Text = "Выход";
            btnExit.Location = new Point(200, 260);
            btnExit.Size = new Size(140, 38);
            UIStyleHelper.StyleButton(btnExit, Color.FromArgb(150, 150, 150));

            // Подсказка
            Label lblHint = new Label();
            lblHint.Text = "Тестовые пользователи: admin / admin123  |  dispatcher / disp123";
            lblHint.Font = new Font("Segoe UI", 8F);
            lblHint.ForeColor = Color.FromArgb(150, 150, 150);
            lblHint.AutoSize = true;
            lblHint.Location = new Point(40, 320);

            panelMain.Controls.Add(lblLogin);
            panelMain.Controls.Add(txtLogin);
            panelMain.Controls.Add(lblPassword);
            panelMain.Controls.Add(txtPassword);
            panelMain.Controls.Add(btnLogin);
            panelMain.Controls.Add(btnExit);
            panelMain.Controls.Add(lblHint);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelHeader);

            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(420, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Вход в систему — Диспетчерская логистика";

            // Привязка событий
            btnLogin.Click += BtnLogin_Click;
            btnExit.Click += BtnExit_Click;
            txtLogin.KeyDown += TextField_KeyDown;
            txtPassword.KeyDown += TextField_KeyDown;

            // Фокус на поле логина
            txtLogin.Focus();
        }

        private void TextField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                BtnLogin_Click(sender, e);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                ValidationHelper.ShowWarning("Введите логин.");
                txtLogin.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ValidationHelper.ShowWarning("Введите пароль.");
                txtPassword.Focus();
                return;
            }

            try
            {
                // Попытка авторизации
                UserModel user = _userRepository.Authenticate(txtLogin.Text.Trim(), txtPassword.Text);

                if (user == null)
                {
                    ValidationHelper.ShowError("Неверный логин или пароль. Попробуйте снова.");
                    txtPassword.Text = "";
                    txtPassword.Focus();
                    return;
                }

                // Сохраняем текущего пользователя
                SessionHelper.CurrentUser = user;

                // Открываем главную форму
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка подключения к базе данных:\n{0}", ex.Message));
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
