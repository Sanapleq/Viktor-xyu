using System;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.Helpers;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма "О программе" с информацией о проекте
    /// </summary>
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(520, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "О программе";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Верхняя цветная полоса
            Panel headerBar = new Panel();
            headerBar.Dock = DockStyle.Top;
            headerBar.Height = 100;
            headerBar.BackColor = UIStyleHelper.PrimaryColor;

            Label lblIcon = new Label();
            lblIcon.Text = "🚚";
            lblIcon.Font = new Font("Segoe UI", 40F);
            lblIcon.ForeColor = Color.White;
            lblIcon.AutoSize = true;
            lblIcon.Location = new Point(30, 15);

            Label lblAppName = new Label();
            lblAppName.Text = "Диспетчерская логистика";
            lblAppName.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.AutoSize = true;
            lblAppName.Location = new Point(90, 18);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Заказы и тарифы";
            lblSubtitle.Font = new Font("Segoe UI", 12F);
            lblSubtitle.ForeColor = Color.FromArgb(180, 210, 235);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(90, 52);

            headerBar.Controls.Add(lblIcon);
            headerBar.Controls.Add(lblAppName);
            headerBar.Controls.Add(lblSubtitle);

            // Основная информация
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.White;
            contentPanel.Padding = new Padding(30, 20, 30, 20);

            int y = 15;
            int x = 30;

            AddInfoRow(contentPanel, "Версия:", "1.0.0 (учебный проект)", ref y, x);
            AddInfoRow(contentPanel, "Год:", "2026", ref y, x);
            AddInfoRow(contentPanel, "Разработчик:", "Студент колледжа", ref y, x);
            AddInfoRow(contentPanel, "Технологии:", "C#, Windows Forms, SQL Server, ADO.NET", ref y, x);
            AddInfoRow(contentPanel, "IDE:", "Visual Studio 2019", ref y, x);
            AddInfoRow(contentPanel, "СУБД:", "Microsoft SQL Server 2019", ref y, x);

            y += 10;

            // Описание
            Label lblDescTitle = UIStyleHelper.CreateSubHeaderLabel("Описание проекта");
            lblDescTitle.Location = new Point(x, y);
            contentPanel.Controls.Add(lblDescTitle);
            y += 30;

            Label lblDesc = new Label();
            lblDesc.Text = "Система «Диспетчерская логистика: Заказы и тарифы» — это приложение для автоматизации работы диспетчерской службы логистической компании.\r\n\r\n" +
                "Основные возможности:\r\n" +
                "  •  Ведение справочников клиентов, транспорта, тарифов и геоточек\r\n" +
                "  •  Оформление заявок на перевозку\r\n" +
                "  •  Автоматический расчёт стоимости перевозки\r\n" +
                "  •  Хранение истории заказов и статусов\r\n" +
                "  •  Фильтрация и поиск по журналу заказов\r\n" +
                "  •  Формирование отчётов (оборот по клиентам, средний чек, загруженность транспорта)\r\n" +
                "  •  Экспорт данных в CSV (Excel)";
            lblDesc.Font = new Font("Segoe UI", 9F);
            lblDesc.ForeColor = Color.FromArgb(80, 80, 80);
            lblDesc.Location = new Point(x, y);
            lblDesc.Size = new Size(440, 210);
            contentPanel.Controls.Add(lblDesc);

            y += 225;

            // Разделитель
            Panel sep = new Panel();
            sep.Height = 1;
            sep.BackColor = Color.FromArgb(220, 220, 220);
            sep.Dock = DockStyle.Top;
            contentPanel.Controls.Add(sep);

            // Кнопка закрытия
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new Point(350, y + 10);
            btnClose.Size = new Size(120, 36);
            UIStyleHelper.StyleButton(btnClose, UIStyleHelper.PrimaryColor);
            btnClose.Click += (s, e) => this.Close();
            contentPanel.Controls.Add(btnClose);

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerBar);
        }

        private void AddInfoRow(Panel parent, string label, string value, ref int y, int x)
        {
            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(100, 100, 100);
            lbl.AutoSize = true;
            lbl.Location = new Point(x, y);
            parent.Controls.Add(lbl);

            Label val = new Label();
            val.Text = value;
            val.Font = new Font("Segoe UI", 9F);
            val.ForeColor = Color.FromArgb(50, 50, 50);
            val.AutoSize = true;
            val.Location = new Point(x + 130, y);
            parent.Controls.Add(val);

            y += 24;
        }
    }
}
