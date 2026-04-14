using System;
using System.Drawing;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Helpers;
using DispatchLogistics.Models;

namespace DispatchLogistics.Forms
{
    /// <summary>
    /// Форма добавления/редактирования клиента
    /// </summary>
    public partial class ClientEditForm : Form
    {
        private ClientRepository _clientRepo = new ClientRepository();
        private int _clientId;    // 0 = новый, >0 = редактирование
        private bool _isNew;

        // Элементы
        private ComboBox cbClientType;
        private TextBox txtName;
        private TextBox txtContactPerson;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private TextBox txtContractNumber;
        private DateTimePicker dtpContractDate;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public ClientEditForm(int clientId = 0)
        {
            _clientId = clientId;
            _isNew = (clientId == 0);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            UIStyleHelper.StyleForm(this);
            this.Size = new Size(550, 620);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = _isNew ? "Новый клиент" : "Редактирование клиента";
            this.StartPosition = FormStartPosition.CenterParent;

            // Заголовок
            Label lblTitle = UIStyleHelper.CreateHeaderLabel(_isNew ? "Новый клиент" : "Редактирование клиента");
            lblTitle.Location = new Point(20, 15);

            // ===== Поля формы =====
            int x = 20;
            int y = 55;
            int labelWidth = 140;
            int fieldWidth = 340;
            int fieldHeight = 26;
            int rowSpacing = 38;

            // Тип клиента
            Label lblType = UIStyleHelper.CreateLabel("Тип клиента:");
            lblType.Location = new Point(x, y);
            lblType.Width = labelWidth;
            this.Controls.Add(lblType);

            cbClientType = UIStyleHelper.CreateComboBox();
            cbClientType.Location = new Point(x + labelWidth + 10, y - 3);
            cbClientType.Size = new Size(fieldWidth, fieldHeight);
            cbClientType.Items.Add("Юр. лицо");
            cbClientType.Items.Add("Физ. лицо");
            this.Controls.Add(cbClientType);
            y += rowSpacing;

            // Название / ФИО
            Label lblName = UIStyleHelper.CreateLabel("Название / ФИО: *");
            lblName.Location = new Point(x, y);
            lblName.Width = labelWidth;
            this.Controls.Add(lblName);

            txtName = UIStyleHelper.CreateTextBox();
            txtName.Location = new Point(x + labelWidth + 10, y - 3);
            txtName.Size = new Size(fieldWidth, fieldHeight);
            this.Controls.Add(txtName);
            y += rowSpacing;

            // Контактное лицо
            Label lblContact = UIStyleHelper.CreateLabel("Контактное лицо:");
            lblContact.Location = new Point(x, y);
            lblContact.Width = labelWidth;
            this.Controls.Add(lblContact);

            txtContactPerson = UIStyleHelper.CreateTextBox();
            txtContactPerson.Location = new Point(x + labelWidth + 10, y - 3);
            txtContactPerson.Size = new Size(fieldWidth, fieldHeight);
            this.Controls.Add(txtContactPerson);
            y += rowSpacing;

            // Телефон
            Label lblPhone = UIStyleHelper.CreateLabel("Телефон:");
            lblPhone.Location = new Point(x, y);
            lblPhone.Width = labelWidth;
            this.Controls.Add(lblPhone);

            txtPhone = UIStyleHelper.CreateTextBox();
            txtPhone.Location = new Point(x + labelWidth + 10, y - 3);
            txtPhone.Size = new Size(fieldWidth, fieldHeight);
            this.Controls.Add(txtPhone);
            y += rowSpacing;

            // Email
            Label lblEmail = UIStyleHelper.CreateLabel("Email:");
            lblEmail.Location = new Point(x, y);
            lblEmail.Width = labelWidth;
            this.Controls.Add(lblEmail);

            txtEmail = UIStyleHelper.CreateTextBox();
            txtEmail.Location = new Point(x + labelWidth + 10, y - 3);
            txtEmail.Size = new Size(fieldWidth, fieldHeight);
            this.Controls.Add(txtEmail);
            y += rowSpacing;

            // Адрес
            Label lblAddress = UIStyleHelper.CreateLabel("Адрес:");
            lblAddress.Location = new Point(x, y);
            lblAddress.Width = labelWidth;
            this.Controls.Add(lblAddress);

            txtAddress = UIStyleHelper.CreateTextBox();
            txtAddress.Location = new Point(x + labelWidth + 10, y - 3);
            txtAddress.Size = new Size(fieldWidth, fieldHeight);
            this.Controls.Add(txtAddress);
            y += rowSpacing;

            // Номер договора
            Label lblContractNum = UIStyleHelper.CreateLabel("Номер договора:");
            lblContractNum.Location = new Point(x, y);
            lblContractNum.Width = labelWidth;
            this.Controls.Add(lblContractNum);

            txtContractNumber = UIStyleHelper.CreateTextBox();
            txtContractNumber.Location = new Point(x + labelWidth + 10, y - 3);
            txtContractNumber.Size = new Size(fieldWidth, fieldHeight);
            this.Controls.Add(txtContractNumber);
            y += rowSpacing;

            // Дата договора
            Label lblContractDate = UIStyleHelper.CreateLabel("Дата договора:");
            lblContractDate.Location = new Point(x, y);
            lblContractDate.Width = labelWidth;
            this.Controls.Add(lblContractDate);

            dtpContractDate = UIStyleHelper.CreateDateTimePicker();
            dtpContractDate.Location = new Point(x + labelWidth + 10, y - 3);
            dtpContractDate.Size = new Size(fieldWidth, fieldHeight);
            dtpContractDate.Checked = false;
            dtpContractDate.ShowCheckBox = true;
            this.Controls.Add(dtpContractDate);
            y += rowSpacing;

            // Примечание
            Label lblNotes = UIStyleHelper.CreateLabel("Примечание:");
            lblNotes.Location = new Point(x, y);
            lblNotes.Width = labelWidth;
            this.Controls.Add(lblNotes);

            txtNotes = new TextBox();
            txtNotes.Font = new Font("Segoe UI", 9F);
            txtNotes.BorderStyle = BorderStyle.FixedSingle;
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Height = 60;
            txtNotes.Location = new Point(x + labelWidth + 10, y - 3);
            txtNotes.Size = new Size(fieldWidth, 60);
            this.Controls.Add(txtNotes);
            y += 80;

            // ===== Кнопки =====
            btnSave = new Button();
            btnSave.Text = _isNew ? "➕ Добавить" : "💾 Сохранить";
            btnSave.Location = new Point(x + labelWidth + 10, y + 10);
            btnSave.Size = new Size(150, 36);
            UIStyleHelper.StyleButton(btnSave, UIStyleHelper.SuccessColor);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(x + labelWidth + 170, y + 10);
            btnCancel.Size = new Size(150, 36);
            UIStyleHelper.StyleButton(btnCancel, Color.FromArgb(150, 150, 150));
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            if (_isNew)
            {
                cbClientType.SelectedIndex = 0;
                return;
            }

            try
            {
                ClientModel client = _clientRepo.GetById(_clientId);
                if (client == null)
                {
                    ValidationHelper.ShowError("Клиент не найден.");
                    this.Close();
                    return;
                }

                cbClientType.SelectedItem = client.ClientType;
                txtName.Text = client.Name;
                txtContactPerson.Text = client.ContactPerson ?? "";
                txtPhone.Text = client.Phone ?? "";
                txtEmail.Text = client.Email ?? "";
                txtAddress.Text = client.Address ?? "";
                txtContractNumber.Text = client.ContractNumber ?? "";

                if (client.ContractDate.HasValue)
                {
                    dtpContractDate.Checked = true;
                    dtpContractDate.Value = client.ContractDate.Value;
                }
                else
                {
                    dtpContractDate.Checked = false;
                }

                txtNotes.Text = client.Notes ?? "";
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError(string.Format("Ошибка загрузки:\n{0}", ex.Message));
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (!ValidationHelper.IsNotEmpty(txtName, "Название / ФИО"))
                return;

            if (!ValidationHelper.IsValidPhone(txtPhone.Text.Trim(), "Телефон"))
                return;

            if (!ValidationHelper.IsValidEmail(txtEmail.Text.Trim(), "Email"))
                return;

            try
            {
                ClientModel client = new ClientModel();
                client.ClientId = _clientId;
                client.ClientType = cbClientType.SelectedItem != null ? cbClientType.SelectedItem.ToString() : "Юр. лицо";
                client.Name = txtName.Text.Trim();
                client.ContactPerson = string.IsNullOrWhiteSpace(txtContactPerson.Text) ? null : txtContactPerson.Text.Trim();
                client.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
                client.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                client.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
                client.ContractNumber = string.IsNullOrWhiteSpace(txtContractNumber.Text) ? null : txtContractNumber.Text.Trim();
                client.ContractDate = dtpContractDate.Checked ? dtpContractDate.Value : (DateTime?)null;
                client.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                if (_isNew)
                {
                    _clientRepo.InsertClient(client);
                    ValidationHelper.ShowSuccess("Клиент успешно добавлен.");
                }
                else
                {
                    _clientRepo.UpdateClient(client);
                    ValidationHelper.ShowSuccess("Данные клиента обновлены.");
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
