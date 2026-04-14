using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Журнал заказов — просмотр, фильтры, управление статусами
    /// </summary>
    public partial class OrdersForm : Form
    {
        private OrderRepository _orderRepo = new OrderRepository();
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cbStatus;
        private DateTimePicker dtpFrom, dtpTo;
        private Label lblCount;

        public OrdersForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Журнал заказов";

            // Заголовок
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top; headerPanel.Height = 60;
            headerPanel.BackColor = Color.White; headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Журнал заказов");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            // Панель фильтров
            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 85;
            filterPanel.BackColor = Color.White;
            filterPanel.Padding = new Padding(10, 5, 10, 5);

            // Поиск по номеру
            Label lblSearch = UIStyleHelper.CreateLabel("Поиск по номеру:");
            lblSearch.Location = new Point(10, 8);
            filterPanel.Controls.Add(lblSearch);

            txtSearch = UIStyleHelper.CreateTextBox();
            txtSearch.Location = new Point(10, 28);
            txtSearch.Size = new Size(140, 26);
            txtSearch.TextChanged += (s, e) => LoadData();
            filterPanel.Controls.Add(txtSearch);

            // Фильтр по статусу
            Label lblStatus = UIStyleHelper.CreateLabel("Статус:");
            lblStatus.Location = new Point(160, 8);
            filterPanel.Controls.Add(lblStatus);

            cbStatus = UIStyleHelper.CreateComboBox();
            cbStatus.Location = new Point(160, 28);
            cbStatus.Size = new Size(140, 26);
            cbStatus.Items.Add("Все");
            cbStatus.Items.Add("Новый");
            cbStatus.Items.Add("Подтвержден");
            cbStatus.Items.Add("В пути");
            cbStatus.Items.Add("Завершен");
            cbStatus.Items.Add("Отменен");
            cbStatus.SelectedIndex = 0;
            cbStatus.SelectedIndexChanged += (s, e) => LoadData();
            filterPanel.Controls.Add(cbStatus);

            // Дата от
            Label lblFrom = UIStyleHelper.CreateLabel("Дата от:");
            lblFrom.Location = new Point(310, 8);
            filterPanel.Controls.Add(lblFrom);

            dtpFrom = UIStyleHelper.CreateDateTimePicker();
            dtpFrom.Location = new Point(310, 28);
            dtpFrom.Size = new Size(120, 26);
            dtpFrom.Value = DateTime.Now.AddMonths(-1);
            dtpFrom.ValueChanged += (s, e) => LoadData();
            filterPanel.Controls.Add(dtpFrom);

            // Дата до
            Label lblTo = UIStyleHelper.CreateLabel("Дата до:");
            lblTo.Location = new Point(440, 8);
            filterPanel.Controls.Add(lblTo);

            dtpTo = UIStyleHelper.CreateDateTimePicker();
            dtpTo.Location = new Point(440, 28);
            dtpTo.Size = new Size(120, 26);
            dtpTo.Value = DateTime.Now.AddDays(1);
            dtpTo.ValueChanged += (s, e) => LoadData();
            filterPanel.Controls.Add(dtpTo);

            // Кнопки действий
            AddBtn(filterPanel, "➕ Новый заказ", 580, 8, UIStyleHelper.SuccessColor, BtnAdd_Click);
            AddBtn(filterPanel, "✏️ Редактировать", 710, 8, UIStyleHelper.PrimaryColor, BtnEdit_Click);
            AddBtn(filterPanel, "🗑️ Удалить", 850, 8, UIStyleHelper.DangerColor, BtnDelete_Click);

            // Кнопка смены статуса
            Button btnChangeStatus = new Button();
            btnChangeStatus.Text = "🔄 Сменить статус";
            btnChangeStatus.Location = new Point(580, 46);
            btnChangeStatus.Size = new Size(140, 30);
            UIStyleHelper.StyleButton(btnChangeStatus, UIStyleHelper.WarningColor);
            btnChangeStatus.Click += BtnChangeStatus_Click;
            filterPanel.Controls.Add(btnChangeStatus);

            AddBtn(filterPanel, "📄 Экспорт", 730, 46, Color.FromArgb(142, 68, 173), BtnExport_Click);

            lblCount = UIStyleHelper.CreateLabel("");
            lblCount.Location = new Point(870, 50);
            filterPanel.Controls.Add(lblCount);

            // Таблица заказов
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            UIStyleHelper.StyleDataGridView(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(null, null); };

            this.Controls.Add(dgv);
            this.Controls.Add(filterPanel);
            this.Controls.Add(headerPanel);
            filterPanel.BringToFront();
        }

        private void AddBtn(Panel p, string text, int x, int y, Color c, EventHandler click)
        {
            Button btn = new Button(); btn.Text = text; btn.Location = new Point(x, y); btn.Size = new Size(130, 30);
            UIStyleHelper.StyleButton(btn, c); btn.Click += click; p.Controls.Add(btn);
        }

        private void LoadData()
        {
            try
            {
                DataTable dt;
                string search = txtSearch.Text.Trim();
                string status = cbStatus.SelectedItem != null ? cbStatus.SelectedItem.ToString() : "Все";

                if (!string.IsNullOrEmpty(search))
                {
                    dt = _orderRepo.SearchOrders(search);
                }
                else if (status != "Все")
                {
                    dt = _orderRepo.GetOrdersByStatus(status);
                }
                else
                {
                    dt = _orderRepo.GetOrdersByDateRange(dtpFrom.Value.Date, dtpTo.Value.Date);
                }

                dgv.DataSource = dt;
                lblCount.Text = string.Format("Заказов: {0}", dt.Rows.Count);
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка загрузки:\n" + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var f = new OrderEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите заказ."); return; }
            int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            var f = new OrderEditForm(id);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите заказ."); return; }
            string num = dgv.SelectedRows[0].Cells["Номер"].Value.ToString();
            if (!ValidationHelper.ConfirmDelete(num)) return;
            try
            {
                int id = (int)dgv.SelectedRows[0].Cells["ID"].Value;
                _orderRepo.DeleteOrder(id);
                ValidationHelper.ShowSuccess(string.Format("Заказ \"{0}\" удалён.", num));
                LoadData();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
        }

        private void BtnChangeStatus_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите заказ."); return; }

            int orderId = (int)dgv.SelectedRows[0].Cells["ID"].Value;
            string currentStatus = dgv.SelectedRows[0].Cells["Статус"].Value.ToString();
            string orderNum = dgv.SelectedRows[0].Cells["Номер"].Value.ToString();

            // Форма смены статуса
            Form statusForm = new Form();
            UIStyleHelper.StyleForm(statusForm);
            statusForm.Size = new Size(350, 200);
            statusForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            statusForm.MaximizeBox = false;
            statusForm.Text = "Смена статуса заказа " + orderNum;
            statusForm.StartPosition = FormStartPosition.CenterParent;

            Label lblInfo = UIStyleHelper.CreateLabel(string.Format("Текущий статус: {0}", currentStatus));
            lblInfo.Location = new Point(20, 15);
            lblInfo.AutoSize = true;

            Label lblNew = UIStyleHelper.CreateLabel("Новый статус:");
            lblNew.Location = new Point(20, 50);

            ComboBox cbNewStatus = UIStyleHelper.CreateComboBox();
            cbNewStatus.Location = new Point(130, 47);
            cbNewStatus.Size = new Size(170, 26);
            cbNewStatus.Items.Add("Новый");
            cbNewStatus.Items.Add("Подтвержден");
            cbNewStatus.Items.Add("В пути");
            cbNewStatus.Items.Add("Завершен");
            cbNewStatus.Items.Add("Отменен");
            cbNewStatus.SelectedItem = currentStatus;

            Button btnOK = new Button();
            btnOK.Text = "Изменить";
            btnOK.Location = new Point(50, 100);
            btnOK.Size = new Size(110, 34);
            UIStyleHelper.StyleButton(btnOK, UIStyleHelper.PrimaryColor);

            Button btnCancel2 = new Button();
            btnCancel2.Text = "Отмена";
            btnCancel2.Location = new Point(180, 100);
            btnCancel2.Size = new Size(110, 34);
            UIStyleHelper.StyleButton(btnCancel2, Color.FromArgb(150, 150, 150));

            btnOK.Click += (s, ev) =>
            {
                string newSt = cbNewStatus.SelectedItem != null ? cbNewStatus.SelectedItem.ToString() : currentStatus;
                if (newSt != currentStatus)
                {
                    try
                    {
                        _orderRepo.UpdateOrderStatus(orderId, newSt);
                        ValidationHelper.ShowSuccess(string.Format("Статус заказа {0} изменён на \"{1}\".", orderNum, newSt));
                        statusForm.DialogResult = DialogResult.OK;
                        statusForm.Close();
                        LoadData();
                    }
                    catch (Exception ex) { ValidationHelper.ShowError("Ошибка: " + ex.Message); }
                }
                else
                {
                    statusForm.Close();
                }
            };

            btnCancel2.Click += (s, ev) => statusForm.Close();

            statusForm.Controls.Add(lblInfo);
            statusForm.Controls.Add(lblNew);
            statusForm.Controls.Add(cbNewStatus);
            statusForm.Controls.Add(btnOK);
            statusForm.Controls.Add(btnCancel2);

            statusForm.ShowDialog();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            ExportHelper.ExportToCSV(dt, "Журнал_заказов");
        }
    }
}
