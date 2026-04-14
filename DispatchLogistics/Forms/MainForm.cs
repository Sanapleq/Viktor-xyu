using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Главная форма приложения с боковым меню и дашбордом
    /// </summary>
    public partial class MainForm : Form
    {
        private Panel panelSidebar;
        private Panel panelTopBar;
        private Panel panelWorkspace;
        private Label lblUserName;
        private Label lblUserRole;
        private Button btnDashboard;
        private Button btnClients;
        private Button btnTransport;
        private Button btnTariffs;
        private Button btnGeoPoints;
        private Button btnOrders;
        private Button btnServices;
        private Button btnReports;
        private Button btnUsers;
        private Button btnLogout;
        private Button btnCurrent;  // текущая активная кнопка

        private ReportRepository _reportRepo = new ReportRepository();

        public MainForm()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(900, 600);
            this.Text = "Диспетчерская логистика: Заказы и тарифы";
            this.StartPosition = FormStartPosition.CenterScreen;

            // ===== Боковое меню =====
            panelSidebar = new Panel();
            panelSidebar.Width = 220;
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.BackColor = Color.FromArgb(44, 62, 80);
            panelSidebar.Padding = new Padding(0);

            // Логотип в сайдбаре
            Label lblLogo = new Label();
            lblLogo.Text = "🚚 Логистика";
            lblLogo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.AutoSize = false;
            lblLogo.Size = new Size(220, 60);
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            panelSidebar.Controls.Add(lblLogo);

            // Разделитель
            Panel sep1 = new Panel();
            sep1.Height = 1;
            sep1.BackColor = Color.FromArgb(60, 80, 100);
            sep1.Dock = DockStyle.Top;
            panelSidebar.Controls.Add(sep1);

            // Кнопки меню
            int y = 61;
            int btnWidth = 220;
            int btnHeight = 40;
            int spacing = 2;

            btnDashboard = CreateMenuButton("  📊  Дашборд", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnClients = CreateMenuButton("  👥  Клиенты", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnTransport = CreateMenuButton("  🚛  Транспорт", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnTariffs = CreateMenuButton("  💰  Тарифы", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnGeoPoints = CreateMenuButton("  📍  Геоточки", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnOrders = CreateMenuButton("  📋  Заказы", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnServices = CreateMenuButton("  ⚙️  Доп. услуги", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnReports = CreateMenuButton("  📈  Отчёты", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            btnUsers = CreateMenuButton("  🔐  Пользователи", y, btnWidth, btnHeight);
            y += btnHeight + spacing;

            // Разделитель перед выходом
            Panel sep2 = new Panel();
            sep2.Height = 1;
            sep2.BackColor = Color.FromArgb(60, 80, 100);
            sep2.Dock = DockStyle.Top;
            sep2.Margin = new Padding(0, 10, 0, 10);
            panelSidebar.Controls.Add(sep2);

            btnLogout = CreateMenuButton("  🚪  Выход", y + 12, btnWidth, btnHeight);
            btnLogout.BackColor = Color.FromArgb(192, 57, 43);

            // Активная кнопка по умолчанию
            btnCurrent = btnDashboard;
            btnCurrent.BackColor = UIStyleHelper.PrimaryColor;

            // Привязка событий
            btnDashboard.Click += (s, e) => SwitchSection("Дашборд", btnDashboard);
            btnClients.Click += (s, e) => SwitchSection("Клиенты", btnClients);
            btnTransport.Click += (s, e) => SwitchSection("Транспорт", btnTransport);
            btnTariffs.Click += (s, e) => SwitchSection("Тарифы", btnTariffs);
            btnGeoPoints.Click += (s, e) => SwitchSection("Геоточки", btnGeoPoints);
            btnOrders.Click += (s, e) => SwitchSection("Заказы", btnOrders);
            btnServices.Click += (s, e) => SwitchSection("Доп. услуги", btnServices);
            btnReports.Click += (s, e) => SwitchSection("Отчёты", btnReports);
            btnUsers.Click += (s, e) => SwitchSection("Пользователи", btnUsers);
            btnLogout.Click += (s, e) => Logout();

            // Скрываем кнопку пользователей для диспетчера
            if (Helpers.SessionHelper.CurrentUser.Role == "Диспетчер")
                btnUsers.Visible = false;

            // ===== КОНТЕЙНЕР ПРАВОЙ ЧАСТИ (справа от sidebar) =====
            Panel panelRightArea = new Panel();
            panelRightArea.Dock = DockStyle.Fill;

            // Верхняя панель (header)
            panelTopBar = new Panel();
            panelTopBar.Dock = DockStyle.Top;
            panelTopBar.Height = 55;
            panelTopBar.BackColor = Color.White;
            panelTopBar.Padding = new Padding(20, 0, 20, 0);

            Label lblAppTitle = new Label();
            lblAppTitle.Text = "Диспетчерская логистика: Заказы и тарифы";
            lblAppTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblAppTitle.ForeColor = UIStyleHelper.HeaderColor;
            lblAppTitle.AutoSize = true;
            lblAppTitle.Location = new Point(0, 14);

            lblUserName = new Label();
            lblUserName.Font = new Font("Segoe UI", 10F);
            lblUserName.ForeColor = Color.FromArgb(120, 120, 120);
            lblUserName.AutoSize = true;
            lblUserName.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            lblUserRole = new Label();
            lblUserRole.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblUserRole.ForeColor = Color.FromArgb(160, 160, 160);
            lblUserRole.AutoSize = true;
            lblUserRole.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            panelTopBar.Resize += (s, e) =>
            {
                lblUserName.Left = panelTopBar.Width - lblUserName.Width - 20;
                lblUserRole.Left = panelTopBar.Width - lblUserRole.Width - 20;
                lblUserName.Top = 10;
                lblUserRole.Top = 30;
            };

            lblUserName.Text = SessionHelper.CurrentUser.FullName;
            lblUserRole.Text = SessionHelper.CurrentUser.Role;

            panelTopBar.Controls.Add(lblAppTitle);
            panelTopBar.Controls.Add(lblUserName);
            panelTopBar.Controls.Add(lblUserRole);

            // Разделитель под header
            Panel sepTop = new Panel();
            sepTop.Dock = DockStyle.Top;
            sepTop.Height = 1;
            sepTop.BackColor = Color.FromArgb(220, 220, 220);
            panelTopBar.Controls.Add(sepTop);

            // Рабочая область
            panelWorkspace = new Panel();
            panelWorkspace.Dock = DockStyle.Fill;
            panelWorkspace.BackColor = UIStyleHelper.BackgroundColor;
            panelWorkspace.AutoScroll = true;

            // Собираем правую область: header сверху, workspace заполняет остальное
            // ВАЖНО: при Dock-раскладке WinForms обрабатывает Controls в ОБРАТНОМ порядке.
            // Сначала добавляем workspace (Fill), потом header (Top) — тогда header
            // получит верхнюю полоску, а workspace заполнит оставшееся.
            panelRightArea.Controls.Add(panelWorkspace);
            panelRightArea.Controls.Add(panelTopBar);

            // Собираем форму: sidebar слева, правая область занимает всё остальное
            // ВАЖНО: порядок добавления определяет z-order (кто поверх кого).
            // Сначала правая область (Fill), потом sidebar (Left) — sidebar будет слева
            // и не будет перекрывать контент.
            this.Controls.Add(panelRightArea);
            this.Controls.Add(panelSidebar);
        }

        /// <summary>
        /// Создаёт кнопку бокового меню
        /// </summary>
        private Button CreateMenuButton(string text, int y, int width, int height)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(width, height);
            btn.Location = new Point(0, y);
            btn.Font = new Font("Segoe UI", 10F);
            btn.ForeColor = Color.FromArgb(200, 210, 220);
            btn.BackColor = Color.FromArgb(44, 62, 80);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Padding(10, 0, 0, 0);
            panelSidebar.Controls.Add(btn);
            return btn;
        }

        /// <summary>
        /// Переключает раздел приложения
        /// </summary>
        private void SwitchSection(string sectionName, Button clickedBtn)
        {
            // Сбрасываем предыдущую кнопку
            if (btnCurrent != null)
                btnCurrent.BackColor = Color.FromArgb(44, 62, 80);

            // Подсвечиваем текущую
            clickedBtn.BackColor = UIStyleHelper.PrimaryColor;
            btnCurrent = clickedBtn;

            // Очищаем рабочую область
            panelWorkspace.Controls.Clear();

            // Открываем нужный раздел
            switch (sectionName)
            {
                case "Дашборд":
                    LoadDashboard();
                    break;
                case "Клиенты":
                    OpenFormInWorkspace(new ClientsForm());
                    break;
                case "Транспорт":
                    OpenFormInWorkspace(new TransportForm());
                    break;
                case "Тарифы":
                    OpenFormInWorkspace(new TariffsForm());
                    break;
                case "Геоточки":
                    OpenFormInWorkspace(new GeoPointsForm());
                    break;
                case "Заказы":
                    OpenFormInWorkspace(new OrdersForm());
                    break;
                case "Доп. услуги":
                    OpenFormInWorkspace(new AdditionalServicesForm());
                    break;
                case "Отчёты":
                    OpenFormInWorkspace(new ReportsForm());
                    break;
                case "Пользователи":
                    OpenFormInWorkspace(new UsersForm());
                    break;
            }
        }

        /// <summary>
        /// Открывает дочернюю форму в рабочей области
        /// </summary>
        private void OpenFormInWorkspace(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.AutoScroll = true;
            panelWorkspace.Controls.Add(form);
            form.Show();
            form.BringToFront();
        }

        /// <summary>
        /// Загружает дашборд со статистикой
        /// </summary>
        private void LoadDashboard()
        {
            panelWorkspace.Controls.Clear();

            Panel content = new Panel();
            content.Dock = DockStyle.Fill;
            content.BackColor = UIStyleHelper.BackgroundColor;
            content.Padding = new Padding(25);

            // Заголовок
            Label lblWelcome = new Label();
            lblWelcome.Text = string.Format("Добро пожаловать, {0}!", SessionHelper.CurrentUser.FullName);
            lblWelcome.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblWelcome.ForeColor = UIStyleHelper.HeaderColor;
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(25, 20);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Обзор системы диспетчерской логистики";
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(130, 130, 130);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(25, 52);

            content.Controls.Add(lblWelcome);
            content.Controls.Add(lblSubtitle);

            // Статистические карточки
            try
            {
                DataTable counters = _reportRepo.GetDashboardCounters();
                if (counters.Rows.Count > 0)
                {
                    DataRow row = counters.Rows[0];

                    int clientCount = (int)row["ClientCount"];
                    int transportCount = (int)row["TransportCount"];
                    int activeOrders = (int)row["ActiveOrders"];
                    decimal totalRevenue = row["TotalRevenue"] != DBNull.Value ? (decimal)row["TotalRevenue"] : 0;
                    int newOrders = (int)row["NewOrders"];
                    int inTransit = (int)row["InTransitOrders"];

                    int cardWidth = 200;
                    int cardHeight = 100;
                    int cardY = 90;
                    int cardSpacing = 15;
                    int startX = 25;

                    // Карточка 1: Клиенты
                    Panel card1 = UIStyleHelper.CreateStatCard("Клиенты", clientCount.ToString(),
                        UIStyleHelper.PrimaryColor);
                    card1.Location = new Point(startX, cardY);
                    card1.Size = new Size(cardWidth, cardHeight);
                    content.Controls.Add(card1);

                    // Карточка 2: Транспорт
                    Panel card2 = UIStyleHelper.CreateStatCard("Транспорт", transportCount.ToString(),
                        UIStyleHelper.SuccessColor);
                    card2.Location = new Point(startX + cardWidth + cardSpacing, cardY);
                    card2.Size = new Size(cardWidth, cardHeight);
                    content.Controls.Add(card2);

                    // Карточка 3: Активные заказы
                    Panel card3 = UIStyleHelper.CreateStatCard("Активные заказы", activeOrders.ToString(),
                        UIStyleHelper.WarningColor);
                    card3.Location = new Point(startX + (cardWidth + cardSpacing) * 2, cardY);
                    card3.Size = new Size(cardWidth, cardHeight);
                    content.Controls.Add(card3);

                    // Карточка 4: Выручка
                    Panel card4 = UIStyleHelper.CreateStatCard("Выручка (заверш.)",
                        string.Format("{0:N0} ₽", totalRevenue),
                        UIStyleHelper.PrimaryDark);
                    card4.Location = new Point(startX + (cardWidth + cardSpacing) * 3, cardY);
                    card4.Size = new Size(cardWidth, cardHeight);
                    content.Controls.Add(card4);

                    // Карточка 5: Новые заказы
                    Panel card5 = UIStyleHelper.CreateStatCard("Новые заказы", newOrders.ToString(),
                        Color.FromArgb(142, 68, 173));
                    card5.Location = new Point(startX, cardY + cardHeight + cardSpacing);
                    card5.Size = new Size(cardWidth, cardHeight);
                    content.Controls.Add(card5);

                    // Карточка 6: В пути
                    Panel card6 = UIStyleHelper.CreateStatCard("В пути", inTransit.ToString(),
                        Color.FromArgb(211, 84, 0));
                    card6.Location = new Point(startX + cardWidth + cardSpacing, cardY + cardHeight + cardSpacing);
                    card6.Size = new Size(cardWidth, cardHeight);
                    content.Controls.Add(card6);
                }

                // Таблица статусов заказов
                Label lblStatusTitle = UIStyleHelper.CreateSubHeaderLabel("Статусы заказов");
                lblStatusTitle.Location = new Point(25, 310);
                content.Controls.Add(lblStatusTitle);

                DataTable statusData = _reportRepo.GetOrdersStatusCount();
                if (statusData.Rows.Count > 0)
                {
                    DataGridView dgvStatus = new DataGridView();
                    dgvStatus.DataSource = statusData;
                    dgvStatus.Location = new Point(25, 340);
                    dgvStatus.Size = new Size(350, 180);
                    UIStyleHelper.StyleDataGridView(dgvStatus);
                    content.Controls.Add(dgvStatus);
                }
            }
            catch (Exception ex)
            {
                Label lblErr = new Label();
                lblErr.Text = string.Format("Ошибка загрузки данных: {0}", ex.Message);
                lblErr.ForeColor = Color.Red;
                lblErr.Location = new Point(25, 100);
                lblErr.AutoSize = true;
                content.Controls.Add(lblErr);
            }

            panelWorkspace.Controls.Add(content);
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        private void Logout()
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SessionHelper.CurrentUser = null;
                this.Close();
            }
        }
    }
}
