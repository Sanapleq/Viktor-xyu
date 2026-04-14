using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;
using DispatchLogistics.Models;
using DispatchLogistics.Services;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма создания/редактирования заказа
    /// Включает: данные заказа, маршрут, тариф, расчёт стоимости, доп. услуги
    /// </summary>
    public partial class OrderEditForm : Form
    {
        private OrderRepository _orderRepo = new OrderRepository();
        private ClientRepository _clientRepo = new ClientRepository();
        private GeoPointRepository _geoRepo = new GeoPointRepository();
        private TransportRepository _transportRepo = new TransportRepository();
        private TariffRepository _tariffRepo = new TariffRepository();
        private AdditionalServiceRepository _serviceRepo = new AdditionalServiceRepository();
        private DistanceRepository _distRepo = new DistanceRepository();
        private CostCalculationService _calcService = new CostCalculationService();

        private int _orderId;
        private bool _isNew;

        // Основные поля
        private TextBox txtOrderNumber, txtDistance, txtWeight, txtIdleHours, txtNotes;
        private DateTimePicker dtpOrderDate;
        private ComboBox cbClient, cbPointFrom, cbPointTo, cbTransport, cbTariff, cbStatus;

        // Блок расчёта
        private Label lblCalcBreakdown;
        private TextBox txtCalcTotal, txtFinalAmount, txtAdjustReason;

        // Доп. услуги
        private DataGridView dgvServices;
        private ComboBox cbAddService;
        private NumericUpDown nudSvcQty;
        private Button btnAddService, btnRemoveService;

        // Кнопки
        private Button btnSave, btnCancel, btnRecalc;

        // Текущие доп. услуги
        private List<OrderServiceModel> _orderServices = new List<OrderServiceModel>();

        public OrderEditForm(int orderId = 0)
        {
            _orderId = orderId;
            _isNew = (orderId == 0);
            InitializeComponent();
            LoadComboData();
            LoadOrderData();
            Recalculate();
        }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(850, 780);
            this.Text = _isNew ? "Новый заказ" : "Редактирование заказа " + _orderId;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScroll = true;

            Panel content = new Panel();
            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(20);
            content.AutoScroll = true;

            int x = 20, y = 10, lw = 140, fw = 250, fh = 26, rs = 34;

            // ===== Заголовок =====
            Label lblTitle = UIStyleHelper.CreateHeaderLabel(_isNew ? "📋 Новый заказ" : "📋 Редактирование заказа");
            lblTitle.Location = new Point(x, y);
            content.Controls.Add(lblTitle);
            y += 40;

            // ===== Блок: Основная информация =====
            GroupBox gbMain = UIStyleHelper.CreateGroupBox("Основная информация");
            gbMain.Location = new Point(x, y);
            gbMain.Size = new Size(790, 110);

            int gx = 15, gy = 22, glw = 110, gfw = 200, gfh = 24, grs = 32;

            AddFieldToGroup(gbMain, "Номер заказа:", ref txtOrderNumber, gx, ref gy, glw, gfw, gfh, grs);
            txtOrderNumber.ReadOnly = _isNew; // номер авто-генерируется

            AddFieldToGroup(gbMain, "Дата заказа:", ref dtpOrderDate, gx, ref gy, glw, gfw, gfh, grs);

            // Статус
            Label lblSt = UIStyleHelper.CreateLabel("Статус:");
            lblSt.Location = new Point(gx, gy); this.SetCtrlWidth(lblSt, glw); gbMain.Controls.Add(lblSt);
            cbStatus = UIStyleHelper.CreateComboBox();
            cbStatus.Location = new Point(gx + glw + 10, gy - 3);
            cbStatus.Size = new Size(gfw, gfh);
            cbStatus.Items.Add("Новый"); cbStatus.Items.Add("Подтвержден");
            cbStatus.Items.Add("В пути"); cbStatus.Items.Add("Завершен"); cbStatus.Items.Add("Отменен");
            gbMain.Controls.Add(cbStatus);

            y += 120;

            // ===== Блок: Маршрут =====
            GroupBox gbRoute = UIStyleHelper.CreateGroupBox("Маршрут");
            gbRoute.Location = new Point(x, y);
            gbRoute.Size = new Size(790, 75);

            int rx = 15, ry = 22, rlw = 100, rfw = 220, rfh = 24, rrs = 32;

            Label lblCli = UIStyleHelper.CreateLabel("Клиент: *");
            lblCli.Location = new Point(rx, ry); this.SetCtrlWidth(lblCli, rlw); gbRoute.Controls.Add(lblCli);
            cbClient = UIStyleHelper.CreateComboBox();
            cbClient.Location = new Point(rx + rlw + 10, ry - 3);
            cbClient.Size = new Size(rfw, rfh);
            cbClient.SelectedIndexChanged += (s, e) => Recalculate();
            gbRoute.Controls.Add(cbClient);

            Label lblPF = UIStyleHelper.CreateLabel("Погрузка:");
            lblPF.Location = new Point(rx + rfw + rlw + 30, ry); this.SetCtrlWidth(lblPF, rlw); gbRoute.Controls.Add(lblPF);
            cbPointFrom = UIStyleHelper.CreateComboBox();
            cbPointFrom.Location = new Point(rx + rfw + rlw + 30 + rlw + 10, ry - 3);
            cbPointFrom.Size = new Size(rfw, rfh);
            cbPointFrom.SelectedIndexChanged += (s, e) => OnRouteChanged();
            gbRoute.Controls.Add(cbPointFrom);

            Label lblPT = UIStyleHelper.CreateLabel("Разгрузка:");
            lblPT.Location = new Point(rx + rfw + rlw + 30 + rfw + rlw + 50, ry); this.SetCtrlWidth(lblPT, rlw); gbRoute.Controls.Add(lblPT);
            cbPointTo = UIStyleHelper.CreateComboBox();
            cbPointTo.Location = new Point(rx + rfw + rlw + 30 + rfw + rlw + 30 + rlw + 10, ry - 3);
            cbPointTo.Size = new Size(rfw, rfh);
            cbPointTo.SelectedIndexChanged += (s, e) => OnRouteChanged();
            gbRoute.Controls.Add(cbPointTo);

            y += 85;

            // ===== Блок: Транспорт и тариф =====
            GroupBox gbTT = UIStyleHelper.CreateGroupBox("Транспорт и тариф");
            gbTT.Location = new Point(x, y);
            gbTT.Size = new Size(790, 75);

            int tx = 15, ty = 22, tlw = 110, tfw = 200, tfh = 24, trs = 32;

            Label lblTr = UIStyleHelper.CreateLabel("Транспорт:");
            lblTr.Location = new Point(tx, ty); this.SetCtrlWidth(lblTr, tlw); gbTT.Controls.Add(lblTr);
            cbTransport = UIStyleHelper.CreateComboBox();
            cbTransport.Location = new Point(tx + tlw + 10, ty - 3);
            cbTransport.Size = new Size(tfw, tfh);
            cbTransport.SelectedIndexChanged += (s, e) => Recalculate();
            gbTT.Controls.Add(cbTransport);

            Label lblTar = UIStyleHelper.CreateLabel("Тариф:");
            lblTar.Location = new Point(tx + tfw + tlw + 40, ty); this.SetCtrlWidth(lblTar, tlw); gbTT.Controls.Add(lblTar);
            cbTariff = UIStyleHelper.CreateComboBox();
            cbTariff.Location = new Point(tx + tfw + tlw + 40 + tlw + 10, ty - 3);
            cbTariff.Size = new Size(tfw, tfh);
            cbTariff.SelectedIndexChanged += (s, e) => Recalculate();
            gbTT.Controls.Add(cbTariff);

            y += 85;

            // ===== Блок: Параметры перевозки =====
            GroupBox gbParams = UIStyleHelper.CreateGroupBox("Параметры перевозки");
            gbParams.Location = new Point(x, y);
            gbParams.Size = new Size(790, 75);

            int px = 15, py = 22, plw = 120, pfw = 120, pfh = 24, prs = 32;

            AddFieldToGroup(gbParams, "Расстояние (км):", ref txtDistance, px, ref py, plw, pfw, pfh, prs);
            txtDistance.ReadOnly = true; // авто-подставляется
            txtDistance.BackColor = Color.FromArgb(240, 240, 240);

            AddFieldToGroup(gbParams, "Масса груза (т):", ref txtWeight, px, ref py, plw, pfw, pfh, prs);

            AddFieldToGroup(gbParams, "Часы простоя:", ref txtIdleHours, px, ref py, plw, pfw, pfh, prs);

            y += 85;

            // ===== Блок: Расчёт стоимости =====
            GroupBox gbCalc = UIStyleHelper.CreateGroupBox("Расчёт стоимости");
            gbCalc.Location = new Point(x, y);
            gbCalc.Size = new Size(790, 140);

            lblCalcBreakdown = UIStyleHelper.CreateLabel("Выберите тариф, транспорт и маршрут для расчёта...");
            lblCalcBreakdown.Location = new Point(15, 22);
            lblCalcBreakdown.Size = new Size(500, 80);
            lblCalcBreakdown.Font = new Font("Consolas", 9F);
            gbCalc.Controls.Add(lblCalcBreakdown);

            Label lblCalcTotal = UIStyleHelper.CreateLabel("Расчётная сумма:");
            lblCalcTotal.Location = new Point(550, 22);
            gbCalc.Controls.Add(lblCalcTotal);

            txtCalcTotal = UIStyleHelper.CreateTextBox();
            txtCalcTotal.Location = new Point(550, 44);
            txtCalcTotal.Size = new Size(180, 28);
            txtCalcTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            txtCalcTotal.ForeColor = UIStyleHelper.PrimaryColor;
            txtCalcTotal.ReadOnly = true;
            txtCalcTotal.TextAlign = HorizontalAlignment.Right;
            txtCalcTotal.BackColor = Color.FromArgb(240, 248, 255);
            gbCalc.Controls.Add(txtCalcTotal);

            Label lblFinal = UIStyleHelper.CreateLabel("Итоговая сумма:");
            lblFinal.Location = new Point(550, 78);
            gbCalc.Controls.Add(lblFinal);

            Label lblAdjReason = UIStyleHelper.CreateLabel("Причина коррекции:");
            lblAdjReason.Visible = false;
            lblAdjReason.Location = new Point(15, 100);
            gbCalc.Controls.Add(lblAdjReason);

            txtFinalAmount = UIStyleHelper.CreateTextBox();
            txtFinalAmount.Location = new Point(550, 100);
            txtFinalAmount.Size = new Size(180, 28);
            txtFinalAmount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            txtFinalAmount.ForeColor = UIStyleHelper.SuccessColor;
            txtFinalAmount.TextAlign = HorizontalAlignment.Right;
            txtFinalAmount.TextChanged += (s, e) => { if (txtFinalAmount.Text != txtCalcTotal.Text) lblAdjReason.Visible = true; else lblAdjReason.Visible = false; };
            gbCalc.Controls.Add(txtFinalAmount);

            txtAdjustReason = UIStyleHelper.CreateTextBox();
            txtAdjustReason.Location = new Point(15, 118);
            txtAdjustReason.Size = new Size(500, 24);
            txtAdjustReason.Visible = false;
            gbCalc.Controls.Add(txtAdjustReason);

            // Кнопка пересчёта
            btnRecalc = new Button();
            btnRecalc.Text = "🔄 Пересчитать";
            btnRecalc.Location = new Point(550, 100);
            btnRecalc.Size = new Size(180, 32);
            UIStyleHelper.StyleButton(btnRecalc, Color.FromArgb(142, 68, 173));
            btnRecalc.Click += (s, e) => Recalculate();
            // Уже добавлена выше как txtFinalAmount — переместим
            gbCalc.Controls.Add(btnRecalc);
            btnRecalc.Visible = false; // скроем, пересчёт автоматический

            y += 150;

            // ===== Блок: Дополнительные услуги =====
            GroupBox gbSvc = UIStyleHelper.CreateGroupBox("Дополнительные услуги");
            gbSvc.Location = new Point(x, y);
            gbSvc.Size = new Size(790, 160);

            Label lblSvc = UIStyleHelper.CreateLabel("Добавить услугу:");
            lblSvc.Location = new Point(15, 22);
            gbSvc.Controls.Add(lblSvc);

            cbAddService = UIStyleHelper.CreateComboBox();
            cbAddService.Location = new Point(120, 19);
            cbAddService.Size = new Size(250, 26);
            gbSvc.Controls.Add(cbAddService);

            Label lblQty = UIStyleHelper.CreateLabel("Кол-во:");
            lblQty.Location = new Point(380, 22);
            gbSvc.Controls.Add(lblQty);

            nudSvcQty = new NumericUpDown();
            nudSvcQty.Location = new Point(430, 19);
            nudSvcQty.Size = new Size(70, 26);
            nudSvcQty.Minimum = 1;
            nudSvcQty.Maximum = 1000;
            nudSvcQty.Value = 1;
            nudSvcQty.Font = new Font("Segoe UI", 9F);
            gbSvc.Controls.Add(nudSvcQty);

            btnAddService = new Button();
            btnAddService.Text = "➕ Добавить";
            btnAddService.Location = new Point(510, 17);
            btnAddService.Size = new Size(110, 30);
            UIStyleHelper.StyleButton(btnAddService, UIStyleHelper.SuccessColor);
            btnAddService.Click += BtnAddService_Click;
            gbSvc.Controls.Add(btnAddService);

            btnRemoveService = new Button();
            btnRemoveService.Text = "🗑️ Удалить";
            btnRemoveService.Location = new Point(630, 17);
            btnRemoveService.Size = new Size(100, 30);
            UIStyleHelper.StyleButton(btnRemoveService, UIStyleHelper.DangerColor);
            btnRemoveService.Click += BtnRemoveService_Click;
            gbSvc.Controls.Add(btnRemoveService);

            dgvServices = new DataGridView();
            dgvServices.Location = new Point(15, 55);
            dgvServices.Size = new Size(760, 90);
            dgvServices.Columns.Add("ServiceName", "Услуга");
            dgvServices.Columns.Add("Quantity", "Кол-во");
            dgvServices.Columns.Add("Price", "Цена");
            dgvServices.Columns.Add("Total", "Сумма");
            UIStyleHelper.StyleDataGridView(dgvServices);
            dgvServices.ReadOnly = true;
            dgvServices.AllowUserToAddRows = false;
            gbSvc.Controls.Add(dgvServices);

            y += 170;

            // ===== Примечание =====
            Label lblNote = UIStyleHelper.CreateLabel("Примечание к заказу:");
            lblNote.Location = new Point(x, y);
            content.Controls.Add(lblNote);
            y += 22;

            txtNotes = new TextBox();
            txtNotes.Font = new Font("Segoe UI", 9F);
            txtNotes.BorderStyle = BorderStyle.FixedSingle;
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Height = 50;
            txtNotes.Location = new Point(x, y);
            txtNotes.Size = new Size(790, 50);
            content.Controls.Add(txtNotes);
            y += 60;

            // ===== Кнопки сохранения =====
            btnSave = new Button();
            btnSave.Text = _isNew ? "📋 Создать заказ" : "💾 Сохранить изменения";
            btnSave.Location = new Point(x, y + 10);
            btnSave.Size = new Size(180, 40);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor);
            btnSave.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;
            content.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(x + 200, y + 10);
            btnCancel.Size = new Size(140, 40);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150));
            btnCancel.Font = new Font("Segoe UI", 11F);
            btnCancel.Click += (s, e) => this.Close();
            content.Controls.Add(btnCancel);

            this.Controls.Add(content);
        }

        private void SetCtrlWidth(Control ctrl, int width) { ctrl.Width = width; }

        private void AddFieldToGroup(GroupBox gb, string label, ref TextBox txt, int x, ref int y, int lw, int fw, int fh, int rs)
        {
            if (label.Contains("дата", StringComparison.OrdinalIgnoreCase))
            {
                // Special handling for date
                return;
            }
            Label lbl = UIStyleHelper.CreateLabel(label);
            lbl.Location = new Point(x, y); lbl.Width = lw; gb.Controls.Add(lbl);
            txt = UIStyleHelper.CreateTextBox();
            txt.Location = new Point(x + lw + 10, y - 3);
            txt.Size = new Size(fw, fh);
            txt.TextChanged += (s, e) => Recalculate();
            gb.Controls.Add(txt);
            y += rs;
        }

        private void AddFieldToGroup(GroupBox gb, string label, ref DateTimePicker dtp, int x, ref int y, int lw, int fw, int fh, int rs)
        {
            Label lbl = UIStyleHelper.CreateLabel(label);
            lbl.Location = new Point(x, y); lbl.Width = lw; gb.Controls.Add(lbl);
            dtp = UIStyleHelper.CreateDateTimePicker();
            dtp.Location = new Point(x + lw + 10, y - 3);
            dtp.Size = new Size(fw, fh);
            gb.Controls.Add(dtp);
            y += rs;
        }

        private void LoadComboData()
        {
            // Клиенты
            DataTable dtClients = _clientRepo.GetClientsForCombo();
            cbClient.DataSource = dtClients.Copy();
            cbClient.DisplayMember = "Name";
            cbClient.ValueMember = "ClientId";

            // Геоточки
            DataTable dtGeo = _geoRepo.GetAllForCombo();
            cbPointFrom.DataSource = dtGeo.Copy();
            cbPointFrom.DisplayMember = "PointName";
            cbPointFrom.ValueMember = "GeoPointId";

            cbPointTo.DataSource = dtGeo.Copy();
            cbPointTo.DisplayMember = "PointName";
            cbPointTo.ValueMember = "GeoPointId";

            // Транспорт (свободный)
            DataTable dtTransport = _isNew ? _transportRepo.GetAvailableTransport() : _transportRepo.GetAllForCombo();
            cbTransport.DataSource = dtTransport.Copy();
            cbTransport.DisplayMember = "TransportInfo";
            cbTransport.ValueMember = "TransportId";

            // Тарифы (активные)
            DataTable dtTariffs = _tariffRepo.GetActiveTariffs();
            cbTariff.DataSource = dtTariffs.Copy();
            cbTariff.DisplayMember = "TariffName";
            cbTariff.ValueMember = "TariffId";

            // Доп. услуги (для добавления)
            DataTable dtServices = _serviceRepo.GetActiveServices();
            cbAddService.DataSource = dtServices.Copy();
            cbAddService.DisplayMember = "ServiceName";
            cbAddService.ValueMember = "ServiceId";
        }

        private void LoadOrderData()
        {
            if (_isNew)
            {
                txtOrderNumber.Text = _orderRepo.GenerateOrderNumber();
                dtpOrderDate.Value = DateTime.Now;
                cbStatus.SelectedIndex = 0;
                txtWeight.Text = "0";
                txtIdleHours.Text = "0";
                txtDistance.Text = "0";
                txtCalcTotal.Text = "0.00";
                txtFinalAmount.Text = "0.00";
                return;
            }

            try
            {
                OrderModel order = _orderRepo.GetOrderById(_orderId);
                if (order == null) { ValidationHelper.ShowError("Заказ не найден."); this.Close(); return; }

                txtOrderNumber.Text = order.OrderNumber;
                dtpOrderDate.Value = order.OrderDate;

                // Установить ComboBox-ы
                SetComboValue(cbClient, order.ClientId);
                SetComboValue(cbPointFrom, order.PointFromId);
                SetComboValue(cbPointTo, order.PointToId);
                SetComboValue(cbTransport, order.TransportId);
                SetComboValue(cbTariff, order.TariffId);

                // Статус
                int statusIdx = GetStatusIndex(order.Status);
                cbStatus.SelectedIndex = statusIdx >= 0 ? statusIdx : 0;

                txtDistance.Text = order.DistanceKm.ToString();
                txtWeight.Text = order.CargoWeight.HasValue ? order.CargoWeight.Value.ToString() : "0";
                txtIdleHours.Text = order.IdleHours.HasValue ? order.IdleHours.Value.ToString() : "0";
                txtCalcTotal.Text = order.CalculatedAmount.ToString("N2");
                txtFinalAmount.Text = order.FinalAmount.ToString("N2");
                txtNotes.Text = order.Notes ?? "";
                txtAdjustReason.Text = order.ManualAdjustmentReason ?? "";

                if (!string.IsNullOrEmpty(order.ManualAdjustmentReason))
                {
                    // Показать поля ручной корректировки
                    foreach (Control ctrl in this.GetAllControls())
                    {
                        if (ctrl.Text.Contains("Причина коррекции")) { ctrl.Visible = true; }
                    }
                }

                // Загрузить доп. услуги
                _orderServices = _orderRepo.GetOrderServices(_orderId);
                RefreshServicesGrid();
            }
            catch (Exception ex) { ValidationHelper.ShowError("Ошибка загрузки: " + ex.Message); }
        }

        private void SetComboValue(ComboBox cb, int value)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                DataRowView row = cb.Items[i] as DataRowView;
                if (row != null && (int)row[cb.ValueMember] == value)
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }
        }

        private int GetStatusIndex(string status)
        {
            string[] statuses = { "Новый", "Подтвержден", "В пути", "Завершен", "Отменен" };
            for (int i = 0; i < statuses.Length; i++)
                if (statuses[i] == status) return i;
            return -1;
        }

        private IEnumerable<Control> GetAllControls()
        {
            List<Control> all = new List<Control>();
            CollectControls(this, all);
            return all;
        }

        private void CollectControls(Control parent, List<Control> list)
        {
            foreach (Control c in parent.Controls)
            {
                list.Add(c);
                if (c.HasChildren) CollectControls(c, list);
            }
        }

        /// <summary>
        /// При изменении маршрута — подтянуть расстояние
        /// </summary>
        private void OnRouteChanged()
        {
            if (cbPointFrom.SelectedValue == null || cbPointTo.SelectedValue == null)
                return;

            int fromId = (int)cbPointFrom.SelectedValue;
            int toId = (int)cbPointTo.SelectedValue;

            decimal? dist = _distRepo.GetDistanceBetween(fromId, toId);
            if (dist.HasValue)
            {
                txtDistance.Text = dist.Value.ToString();
            }
            else
            {
                txtDistance.Text = "0";
                if (!_isNew) // не показывать при создании нового
                    ValidationHelper.ShowWarning("Расстояние между выбранными точками не найдено в базе.\nВведите расстояние вручную или добавьте его в справочник расстояний.");
            }

            Recalculate();
        }

        /// <summary>
        /// Пересчёт стоимости заказа
        /// </summary>
        private void Recalculate()
        {
            try
            {
                if (cbTariff.SelectedValue == null)
                {
                    lblCalcBreakdown.Text = "Выберите тариф для расчёта...";
                    txtCalcTotal.Text = "0.00";
                    txtFinalAmount.Text = "0.00";
                    return;
                }

                // Получаем тариф
                int tariffId = (int)cbTariff.SelectedValue;
                TariffModel tariff = _tariffRepo.GetById(tariffId);
                if (tariff == null) return;

                // Параметры
                decimal distance = 0;
                decimal.TryParse(txtDistance.Text, out distance);

                decimal weight = 0;
                decimal.TryParse(txtWeight.Text, out weight);

                decimal idleHours = 0;
                decimal.TryParse(txtIdleHours.Text, out idleHours);

                // Сумма доп. услуг
                decimal servicesCost = _calcService.CalculateServicesCost(_orderServices);

                // Расчёт
                CalculationResult result = _calcService.Calculate(tariff, distance, weight, idleHours, servicesCost);

                // Обновляем расшифровку
                lblCalcBreakdown.Text = result.GetBreakdownText();

                // Обновляем поля
                txtCalcTotal.Text = result.CalculatedTotal.ToString("N2");

                // Если итоговая совпадает с расчётной — обновить
                bool isManualAdjusted = false;
                try
                {
                    decimal currentFinal = decimal.Parse(txtFinalAmount.Text.Replace(" ", ""));
                    isManualAdjusted = (currentFinal != result.CalculatedTotal);
                }
                catch { }

                if (!isManualAdjusted)
                {
                    txtFinalAmount.Text = result.CalculatedTotal.ToString("N2");
                }
            }
            catch
            {
                // Тихо игнорируем ошибки расчёта при загрузке
            }
        }

        /// <summary>
        /// Добавить доп. услугу
        /// </summary>
        private void BtnAddService_Click(object sender, EventArgs e)
        {
            if (cbAddService.SelectedValue == null) return;

            int serviceId = (int)cbAddService.SelectedValue;
            var svc = _serviceRepo.GetById(serviceId);
            if (svc == null) return;

            decimal qty = (decimal)nudSvcQty.Value;
            decimal total = svc.Price * qty;

            _orderServices.Add(new OrderServiceModel
            {
                ServiceId = serviceId,
                ServiceName = svc.ServiceName,
                Quantity = qty,
                Price = svc.Price,
                Total = total
            });

            RefreshServicesGrid();
            Recalculate();
        }

        /// <summary>
        /// Удалить доп. услугу
        /// </summary>
        private void BtnRemoveService_Click(object sender, EventArgs e)
        {
            if (dgvServices.SelectedRows.Count == 0) { ValidationHelper.ShowWarning("Выберите услугу."); return; }
            int idx = dgvServices.SelectedRows[0].Index;
            _orderServices.RemoveAt(idx);
            RefreshServicesGrid();
            Recalculate();
        }

        /// <summary>
        /// Обновить таблицу доп. услуг
        /// </summary>
        private void RefreshServicesGrid()
        {
            dgvServices.Rows.Clear();
            foreach (var s in _orderServices)
            {
                dgvServices.Rows.Add(s.ServiceName, s.Quantity, string.Format("{0:N2} руб.", s.Price), string.Format("{0:N2} руб.", s.Total));
            }
        }

        /// <summary>
        /// Сохранение заказа
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (cbClient.SelectedValue == null) { ValidationHelper.ShowWarning("Выберите клиента."); return; }
            if (cbPointFrom.SelectedValue == null) { ValidationHelper.ShowWarning("Выберите точку погрузки."); return; }
            if (cbPointTo.SelectedValue == null) { ValidationHelper.ShowWarning("Выберите точку разгрузки."); return; }
            if (cbTransport.SelectedValue == null) { ValidationHelper.ShowWarning("Выберите транспорт."); return; }
            if (cbTariff.SelectedValue == null) { ValidationHelper.ShowWarning("Выберите тариф."); return; }

            decimal distance;
            if (!decimal.TryParse(txtDistance.Text, out distance) || distance <= 0)
            {
                ValidationHelper.ShowWarning("Расстояние должно быть больше 0.");
                return;
            }

            try
            {
                OrderModel order = new OrderModel();
                order.OrderId = _orderId;
                order.OrderNumber = txtOrderNumber.Text.Trim();
                order.OrderDate = dtpOrderDate.Value;
                order.ClientId = (int)cbClient.SelectedValue;
                order.PointFromId = (int)cbPointFrom.SelectedValue;
                order.PointToId = (int)cbPointTo.SelectedValue;
                order.TransportId = (int)cbTransport.SelectedValue;
                order.TariffId = (int)cbTariff.SelectedValue;
                order.DistanceKm = distance;
                order.CargoWeight = string.IsNullOrWhiteSpace(txtWeight.Text) ? (decimal?)null : decimal.Parse(txtWeight.Text);
                order.IdleHours = string.IsNullOrWhiteSpace(txtIdleHours.Text) ? (decimal?)null : decimal.Parse(txtIdleHours.Text);
                order.CalculatedAmount = decimal.Parse(txtCalcTotal.Text.Replace(" ", ""));
                order.FinalAmount = decimal.Parse(txtFinalAmount.Text.Replace(" ", ""));
                order.ManualAdjustmentReason = string.IsNullOrWhiteSpace(txtAdjustReason.Text) ? null : txtAdjustReason.Text.Trim();
                order.Status = cbStatus.SelectedItem != null ? cbStatus.SelectedItem.ToString() : "Новый";
                order.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                if (_isNew)
                {
                    int newId = _orderRepo.InsertOrder(order);

                    // Добавляем доп. услуги
                    foreach (var svc in _orderServices)
                    {
                        svc.OrderId = newId;
                        _orderRepo.AddOrderService(newId, svc.ServiceId, svc.Quantity, svc.Price, svc.Total);
                    }

                    // Записываем в историю
                    var historyRepo = new OrderStatusHistoryRepository();
                    historyRepo.AddHistory(newId, null, order.Status, SessionHelper.CurrentUser.UserId, "Заказ создан");

                    ValidationHelper.ShowSuccess(string.Format("Заказ \"{0}\" успешно создан.", order.OrderNumber));
                }
                else
                {
                    _orderRepo.UpdateOrder(order);

                    // Обновляем доп. услуги
                    _orderRepo.ReplaceOrderServices(_orderId, _orderServices);

                    ValidationHelper.ShowSuccess(string.Format("Заказ \"{0}\" обновлён.", order.OrderNumber));
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка сохранения:\n{0}", ex.Message));
            }
        }
    }
}
