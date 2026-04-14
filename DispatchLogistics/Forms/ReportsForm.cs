using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма отчётов с тремя типами отчётов
    /// </summary>
    public partial class ReportsForm : Form
    {
        private ReportRepository _reportRepo = new ReportRepository();
        private TabControl tabControl;
        private DateTimePicker dtpFrom, dtpTo;
        private DataGridView[] dgvReports = new DataGridView[3];

        public ReportsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = UIStyleHelper.BackgroundColor;
            this.Text = "Отчёты";

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top; headerPanel.Height = 60;
            headerPanel.BackColor = Color.White; headerPanel.Padding = new Padding(15);

            Label lblTitle = UIStyleHelper.CreateHeaderLabel("Отчёты и аналитика");
            lblTitle.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblTitle);

            // Панель выбора периода
            Panel periodPanel = new Panel();
            periodPanel.Dock = DockStyle.Top;
            periodPanel.Height = 50;
            periodPanel.BackColor = Color.White;
            periodPanel.Padding = new Padding(10, 8, 10, 8);

            Label lblPeriod = UIStyleHelper.CreateLabel("Период:");
            lblPeriod.Location = new Point(10, 14);
            periodPanel.Controls.Add(lblPeriod);

            Label lblFrom = UIStyleHelper.CreateLabel("С:");
            lblFrom.Location = new Point(70, 14);
            periodPanel.Controls.Add(lblFrom);

            dtpFrom = UIStyleHelper.CreateDateTimePicker();
            dtpFrom.Location = new Point(95, 12);
            dtpFrom.Size = new Size(120, 26);
            dtpFrom.Value = DateTime.Now.AddMonths(-1);
            periodPanel.Controls.Add(dtpFrom);

            Label lblTo = UIStyleHelper.CreateLabel("По:");
            lblTo.Location = new Point(225, 14);
            periodPanel.Controls.Add(lblTo);

            dtpTo = UIStyleHelper.CreateDateTimePicker();
            dtpTo.Location = new Point(250, 12);
            dtpTo.Size = new Size(120, 26);
            dtpTo.Value = DateTime.Now.AddDays(1);
            periodPanel.Controls.Add(dtpTo);

            Button btnGenerate = new Button();
            btnGenerate.Text = "📊 Сформировать";
            btnGenerate.Location = new Point(390, 10);
            btnGenerate.Size = new Size(160, 32);
            UIStyleHelper.StyleButton(btnGenerate, UIStyleHelper.PrimaryColor);
            btnGenerate.Click += BtnGenerate_Click;
            periodPanel.Controls.Add(btnGenerate);

            // Вкладки отчётов
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F);

            // Отчёт 1: Оборот по клиентам
            TabPage tab1 = new TabPage("Оборот по клиентам");
            tab1.BackColor = UIStyleHelper.BackgroundColor;
            SetupReportTab(tab1, 0, "Оборот по клиентам");

            // Отчёт 2: Средний чек
            TabPage tab2 = new TabPage("Средний чек перевозки");
            tab2.BackColor = UIStyleHelper.BackgroundColor;
            SetupReportTab(tab2, 1, "Средний чек");

            // Отчёт 3: Загруженность транспорта
            TabPage tab3 = new TabPage("Загруженность транспорта");
            tab3.BackColor = UIStyleHelper.BackgroundColor;
            SetupReportTab(tab3, 2, "Загруженность транспорта");

            tabControl.TabPages.Add(tab1);
            tabControl.TabPages.Add(tab2);
            tabControl.TabPages.Add(tab3);

            this.Controls.Add(tabControl);
            this.Controls.Add(periodPanel);
            this.Controls.Add(headerPanel);

            // Автозагрузка
            BtnGenerate_Click(null, null);
        }

        private void SetupReportTab(TabPage tab, int index, string title)
        {
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top; toolbar.Height = 42;
            toolbar.BackColor = Color.White; toolbar.Padding = new Padding(8, 5, 8, 5);

            Button btnExport = new Button();
            btnExport.Text = "📄 Экспорт в CSV";
            btnExport.Location = new Point(8, 6);
            btnExport.Size = new Size(140, 30);
            UIStyleHelper.StyleButton(btnExport, Color.FromArgb(142, 68, 173));
            int idx = index;
            btnExport.Click += (s, e) =>
            {
                DataTable dt = (DataTable)dgvReports[idx].DataSource;
                ExportHelper.ExportToCSV(dt, title.Replace(" ", "_"));
            };
            toolbar.Controls.Add(btnExport);

            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.Name = "dgvReport" + index;
            UIStyleHelper.StyleDataGridView(dgv);
            dgvReports[index] = dgv;

            tab.Controls.Add(dgv);
            tab.Controls.Add(toolbar);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                // Отчёт 1: Оборот по клиентам
                DataTable dt1 = _reportRepo.GetClientTurnover(dtpFrom.Value.Date, dtpTo.Value.Date);
                dgvReports[0].DataSource = dt1;

                // Отчёт 2: Средний чек
                DataTable dt2 = _reportRepo.GetAverageCheck(dtpFrom.Value.Date, dtpTo.Value.Date);
                dgvReports[1].DataSource = dt2;

                // Отчёт 3: Загруженность транспорта
                DataTable dt3 = _reportRepo.GetTransportUtilization(dtpFrom.Value.Date, dtpTo.Value.Date);
                dgvReports[2].DataSource = dt3;
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError("Ошибка формирования отчётов:\n" + ex.Message);
            }
        }
    }
}
