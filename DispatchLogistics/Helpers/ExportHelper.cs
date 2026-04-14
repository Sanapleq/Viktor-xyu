using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace DispatchLogistics.Helpers
{
    /// <summary>
    /// Вспомогательный класс для экспорта данных в Excel (CSV) и печати
    /// </summary>
    public static class ExportHelper
    {
        /// <summary>
        /// Экспорт DataTable в CSV (открывается в Excel)
        /// </summary>
        public static void ExportToCSV(DataTable table, string title)
        {
            if (table == null || table.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV файлы|*.csv";
            sfd.FileName = string.Format("{0}_{1}.csv", title, DateTime.Now.ToString("yyyyMMdd_HHmm"));

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                // Используем точку с запятой как разделитель (для русского Excel)
                string separator = ";";
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                {
                    // Заголовки
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        sw.Write(table.Columns[i].ColumnName);
                        if (i < table.Columns.Count - 1)
                            sw.Write(separator);
                    }
                    sw.WriteLine();

                    // Данные
                    foreach (DataRow row in table.Rows)
                    {
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            string val = row[i].ToString().Replace("\"", "\"\"");
                            // Если содержит разделитель или кавычки — оборачиваем
                            if (val.Contains(separator) || val.Contains("\"") || val.Contains("\n"))
                                val = "\"" + val + "\"";
                            sw.Write(val);
                            if (i < table.Columns.Count - 1)
                                sw.Write(separator);
                        }
                        sw.WriteLine();
                    }
                }

                MessageBox.Show(string.Format("Файл успешно сохранён:\n{0}", sfd.FileName),
                    "Экспорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка при экспорте:\n{0}", ex.Message),
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Экспорт в текстовый файл (для Word)
        /// </summary>
        public static void ExportToText(string content, string title)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Текстовые файлы|*.txt";
            sfd.FileName = string.Format("{0}_{1}.txt", title, DateTime.Now.ToString("yyyyMMdd_HHmm"));

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(sfd.FileName, content, System.Text.Encoding.UTF8);
                MessageBox.Show(string.Format("Файл сохранён:\n{0}", sfd.FileName),
                    "Экспорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка:\n{0}", ex.Message),
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Печать текста через стандартный диалог печати
        /// </summary>
        public static void PrintText(string content, string documentName)
        {
            // Сохраняем во временный файл и открываем для печати
            string tempFile = Path.Combine(Path.GetTempPath(), documentName + ".txt");
            File.WriteAllText(tempFile, content, System.Text.Encoding.UTF8);

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = tempFile;
                psi.Verb = "print";
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Не удалось отправить на печать:\n{0}", ex.Message),
                    "Ошибка печати", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
