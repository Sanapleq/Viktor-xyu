using System.Drawing;
using System.Windows.Forms;

namespace DispatchLogistics.Helpers
{
    /// <summary>
    /// Единый стиль оформления для всех форм приложения
    /// </summary>
    public static class UIStyleHelper
    {
        // Цветовая схема
        public static readonly Color PrimaryColor = Color.FromArgb(41, 128, 185);       // Синий акцент
        public static readonly Color PrimaryDark = Color.FromArgb(30, 90, 140);         // Тёмно-синий
        public static readonly Color BackgroundColor = Color.FromArgb(245, 247, 250);   // Светло-серый фон
        public static readonly Color PanelColor = Color.White;                          // Белый для панелей
        public static readonly Color HeaderColor = Color.FromArgb(52, 73, 94);          // Тёмный заголовок
        public static readonly Color SuccessColor = Color.FromArgb(39, 174, 96);        // Зелёный
        public static readonly Color WarningColor = Color.FromArgb(243, 156, 18);       // Оранжевый
        public static readonly Color DangerColor = Color.FromArgb(231, 76, 60);         // Красный
        public static readonly Color GridHeaderColor = Color.FromArgb(52, 73, 94);
        public static readonly Color GridRowSelected = Color.FromArgb(189, 195, 199);

        /// <summary>
        /// Применяет стиль к кнопке
        /// </summary>
        public static void StyleButton(Button btn, Color? bgColor = null, Color? foreColor = null)
        {
            btn.BackColor = bgColor ?? PrimaryColor;
            btn.ForeColor = foreColor ?? Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.MinimumSize = new Size(0, 30);
        }

        /// <summary>
        /// Применяет стиль к DataGridView
        /// </summary>
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = PanelColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.SelectionBackColor = GridRowSelected;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.EnableHeadersVisualStyles = false;
            dgv.GridColor = Color.FromArgb(230, 230, 230);
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            // Чередование строк
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 252);
        }

        /// <summary>
        /// Настраивает панель-карточку
        /// </summary>
        public static Panel CreateCardPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = PanelColor;
            panel.Padding = new Padding(15);
            return panel;
        }

        /// <summary>
        /// Создаёт стилизованную метку-заголовок
        /// </summary>
        public static Label CreateHeaderLabel(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lbl.ForeColor = HeaderColor;
            lbl.AutoSize = true;
            return lbl;
        }

        /// <summary>
        /// Создаёт стилизованную метку-подзаголовок
        /// </summary>
        public static Label CreateSubHeaderLabel(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbl.ForeColor = PrimaryColor;
            lbl.AutoSize = true;
            return lbl;
        }

        /// <summary>
        /// Создаёт стилизованную обычную метку
        /// </summary>
        public static Label CreateLabel(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 9F);
            lbl.ForeColor = Color.FromArgb(80, 80, 80);
            lbl.AutoSize = true;
            return lbl;
        }

        /// <summary>
        /// Создаёт стилизованное текстовое поле
        /// </summary>
        public static TextBox CreateTextBox()
        {
            TextBox txt = new TextBox();
            txt.Font = new Font("Segoe UI", 9F);
            txt.BorderStyle = BorderStyle.FixedSingle;
            return txt;
        }

        /// <summary>
        /// Создаёт стилизованный ComboBox
        /// </summary>
        public static ComboBox CreateComboBox()
        {
            ComboBox cb = new ComboBox();
            cb.Font = new Font("Segoe UI", 9F);
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.Flat;
            return cb;
        }

        /// <summary>
        /// Создаёт DateTimePicker
        /// </summary>
        public static DateTimePicker CreateDateTimePicker()
        {
            DateTimePicker dtp = new DateTimePicker();
            dtp.Font = new Font("Segoe UI", 9F);
            dtp.Format = DateTimePickerFormat.Short;
            return dtp;
        }

        /// <summary>
        /// Создаёт GroupBox
        /// </summary>
        public static GroupBox CreateGroupBox(string text)
        {
            GroupBox gb = new GroupBox();
            gb.Text = text;
            gb.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gb.ForeColor = HeaderColor;
            gb.Padding = new Padding(12);
            return gb;
        }

        /// <summary>
        /// Настраивает основную форму
        /// </summary>
        public static void StyleForm(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = new Font("Segoe UI", 9F);
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// Создаёт Panel-разделитель
        /// </summary>
        public static Panel CreateSeparator()
        {
            Panel panel = new Panel();
            panel.Height = 2;
            panel.BackColor = Color.FromArgb(220, 220, 220);
            panel.Dock = DockStyle.Top;
            return panel;
        }

        /// <summary>
        /// Создаёт карточку-статистику для дашборда
        /// </summary>
        public static Panel CreateStatCard(string title, string value, Color accentColor, Icon icon = null)
        {
            Panel card = new Panel();
            card.BackColor = PanelColor;
            card.MinimumSize = new Size(180, 90);
            card.Padding = new Padding(12);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblTitle.ForeColor = Color.FromArgb(120, 120, 120);
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(150, 20);
            lblTitle.Location = new Point(12, 10);

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblValue.ForeColor = accentColor;
            lblValue.AutoSize = false;
            lblValue.Size = new Size(150, 35);
            lblValue.Location = new Point(12, 30);

            // Цветная полоска слева
            Panel accent = new Panel();
            accent.BackColor = accentColor;
            accent.Width = 4;
            accent.Dock = DockStyle.Left;

            card.Controls.Add(accent);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);

            return card;
        }
    }
}
