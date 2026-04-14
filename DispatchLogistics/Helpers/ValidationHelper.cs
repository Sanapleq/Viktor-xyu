using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DispatchLogistics.Helpers
{
    /// <summary>
    /// Вспомогательный класс для валидации полей ввода
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Проверяет, что поле не пустое
        /// </summary>
        public static bool IsNotEmpty(Control control, string fieldName)
        {
            if (control is TextBox)
            {
                if (string.IsNullOrWhiteSpace((control as TextBox).Text))
                {
                    MessageBox.Show(
                        string.Format("Поле \"{0}\" обязательно для заполнения.", fieldName),
                        "Ошибка валидации",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    control.Focus();
                    return false;
                }
            }
            else if (control is ComboBox)
            {
                if ((control as ComboBox).SelectedItem == null)
                {
                    MessageBox.Show(
                        string.Format("Поле \"{0}\" обязательно для выбора.", fieldName),
                        "Ошибка валидации",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    control.Focus();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Проверяет корректность телефона
        /// </summary>
        public static bool IsValidPhone(string phone, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // телефон не обязателен

            // Разрешаем: цифры, скобки, дефис, плюс, пробел
            string pattern = @"^[\+]?[\d\s\(\)\-]{5,20}$";
            if (!Regex.IsMatch(phone, pattern))
            {
                MessageBox.Show(
                    string.Format("Поле \"{0}\" содержит некорректный номер телефона.\nДопустимый формат: +7 (XXX) XXX-XX-XX", fieldName),
                    "Ошибка валидации",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверяет корректность email
        /// </summary>
        public static bool IsValidEmail(string email, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // email не обязателен

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                MessageBox.Show(
                    string.Format("Поле \"{0}\" содержит некорректный email.", fieldName),
                    "Ошибка валидации",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
        }

        /// <summary>
        /// Проверяет, что значение — положительное число
        /// </summary>
        public static bool IsPositiveDecimal(decimal? value, string fieldName)
        {
            if (value.HasValue && value.Value < 0)
            {
                MessageBox.Show(
                    string.Format("Поле \"{0}\" должно быть положительным числом.", fieldName),
                    "Ошибка валидации",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Подтверждение удаления
        /// </summary>
        public static bool ConfirmDelete(string objectName)
        {
            var result = MessageBox.Show(
                string.Format("Вы действительно хотите удалить \"{0}\"?\nЭто действие нельзя отменить.", objectName),
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Показывает сообщение об успехе
        /// </summary>
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Показывает сообщение об ошибке
        /// </summary>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Показывает предупреждение
        /// </summary>
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
