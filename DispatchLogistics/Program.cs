using System;
using System.Windows.Forms;
using DispatchLogistics.DataAccess;
using DispatchLogistics.Forms;

namespace DispatchLogistics
{
    /// <summary>
    /// Точка входа в приложение
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Автоматическое создание БД SQLite при первом запуске
                DatabaseInitializer.Initialize();

                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Критическая ошибка приложения:\n\n{0}\n\nСтек:\n{1}",
                        ex.Message, ex.StackTrace),
                    "Ошибка приложения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
