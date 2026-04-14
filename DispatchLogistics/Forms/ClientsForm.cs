using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма справочника клиентов — просмотр, поиск, CRUD
    /// </summary>
    public partial class ClientsForm : Form
    {
        private ClientRepository _clientRepo = new ClientRepository();
        private DataGridView dgvClients;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnExport;
        private Label lblCount;

        public ClientsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Клиенты";

            // Панель заголовка
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = Color.White;
            headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Справочник клиентов");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            // Панель инструментов
            Panel toolbarPanel = new Panel();
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Height = 45;
            toolbarPanel.BackColor = Color.White;
            toolbarPanel.Padding = new Padding(10, 5, 10, 5);

            // Поиск
            Label lblSearch = UIStyleHelper.CreateLabel("Поиск:");
            lblSearch.Location = new Point(10, 12);
            toolbarPanel.Controls.Add(lblSearch);

            txtSearch = UIStyleHelper.CreateTextBox();
            txtSearch.Location = new Point(60, 10);
            txtSearch.Size = new Size(200, 26);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            toolbarPanel.Controls.Add(txtSearch);

            // Кнопка "Добавить"
            btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Location = new Point(280, 8);
            btnAdd.Size = new Size(120, 30);
            UIStyleHelper.StyleButton(btnAdd, UIStyleHelper.SuccessColor);
            btnAdd.Click += BtnAdd_Click;
            toolbarPanel.Controls.Add(btnAdd);

            // Кнопка "Редактировать"
            btnEdit = new Button();
            btnEdit.Text = "✏️ Изменить";
            btnEdit.Location = new Point(410, 8);
            btnEdit.Size = new Size(120, 30);
            UIStyleHelper.StyleButton(btnEdit, UIStyleHelper.PrimaryColor);
            btnEdit.Click += BtnEdit_Click;
            toolbarPanel.Controls.Add(btnEdit);

            // Кнопка "Удалить"
            btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить";
            btnDelete.Location = new Point(540, 8);
            btnDelete.Size = new Size(110, 30);
            UIStyleHelper.StyleButton(btnDelete, UIStyleHelper.DangerColor);
            btnDelete.Click += BtnDelete_Click;
            toolbarPanel.Controls.Add(btnDelete);

            // Кнопка "Обновить"
            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Location = new Point(660, 8);
            btnRefresh.Size = new Size(110, 30);
            UIStyleHelper.StyleButton(btnRefresh, Color.FromArgb(130, 130, 130));
            btnRefresh.Click += BtnRefresh_Click;
            toolbarPanel.Controls.Add(btnRefresh);

            // Кнопка "Экспорт"
            btnExport = new Button();
            btnExport.Text = "📄 Экспорт";
            btnExport.Location = new Point(780, 8);
            btnExport.Size = new Size(100, 30);
            UIStyleHelper.StyleButton(btnExport, Color.FromArgb(142, 68, 173));
            btnExport.Click += BtnExport_Click;
            toolbarPanel.Controls.Add(btnExport);

            // Счётчик записей
            lblCount = UIStyleHelper.CreateLabel("");
            lblCount.Location = new Point(900, 12);
            toolbarPanel.Controls.Add(lblCount);

            // DataGridView
            dgvClients = new DataGridView();
            dgvClients.Dock = DockStyle.Fill;
            UIStyleHelper.StyleDataGridView(dgvClients);
            dgvClients.CellDoubleClick += DgvClients_CellDoubleClick;

            // Сборка
            this.Controls.Add(dgvClients);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(headerPanel);

            toolbarPanel.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = _clientRepo.GetAllClients();
                dgvClients.DataSource = dt;
                lblCount.Text = string.Format("Записей: {0}", dt.Rows.Count);
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка загрузки данных:\n{0}", ex.Message));
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(search))
            {
                LoadData();
                return;
            }

            try
            {
                DataTable dt = _clientRepo.SearchClients(search);
                dgvClients.DataSource = dt;
                lblCount.Text = string.Format("Найдено: {0}", dt.Rows.Count);
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка поиска:\n{0}", ex.Message));
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ClientEditForm editForm = new ClientEditForm();
            editForm.ShowDialog();
            LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                ValidationHelper.ShowWarning("Выберите клиента для редактирования.");
                return;
            }

            int clientId = (int)dgvClients.SelectedRows[0].Cells["ID"].Value;
            ClientEditForm editForm = new ClientEditForm(clientId);
            editForm.ShowDialog();
            LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                ValidationHelper.ShowWarning("Выберите клиента для удаления.");
                return;
            }

            int clientId = (int)dgvClients.SelectedRows[0].Cells["ID"].Value;
            string clientName = dgvClients.SelectedRows[0].Cells["Название"].Value.ToString();

            if (!ValidationHelper.ConfirmDelete(clientName))
                return;

            // Проверяем, есть ли заказы у клиента
            if (!_clientRepo.CanDelete(clientId))
            {
                ValidationHelper.ShowWarning(
                    string.Format("Невозможно удалить клиента \"{0}\",\nтак как у него есть заказы.", clientName));
                return;
            }

            try
            {
                _clientRepo.DeleteClient(clientId);
                ValidationHelper.ShowSuccess(string.Format("Клиент \"{0}\" удалён.", clientName));
                LoadData();
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка удаления:\n{0}", ex.Message));
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            LoadData();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvClients.DataSource;
            ExportHelper.ExportToCSV(dt, "Клиенты");
        }

        private void DgvClients_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int clientId = (int)dgvClients.Rows[e.RowIndex].Cells["ID"].Value;
                ClientEditForm editForm = new ClientEditForm(clientId);
                editForm.ShowDialog();
                LoadData();
            }
        }
    }
}
