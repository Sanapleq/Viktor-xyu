using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DispatchLogistics.DataAccess
{
    /// <summary>
    /// Вспомогательный класс для работы с базой данных SQLite
    /// Все запросы параметризованы для защиты от SQL-инъекций
    /// </summary>
    public static class DatabaseHelper
    {
        private static string _dbPath;

        /// <summary>
        /// Путь к файлу базы данных SQLite
        /// Файл создаётся автоматически в папке приложения
        /// </summary>
        public static string DbPath
        {
            get
            {
                if (_dbPath == null)
                {
                    // БД лежит рядом с исполняемым файлом
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    _dbPath = Path.Combine(basePath, "DispatchLogisticsDB.db");
                }
                return _dbPath;
            }
            set { _dbPath = value; }
        }

        /// <summary>
        /// Строка подключения к SQLite
        /// </summary>
        public static string ConnectionString
        {
            get { return string.Format("Data Source={0}", DbPath); }
        }

        /// <summary>
        /// Проверяет, существует ли файл БД
        /// </summary>
        public static bool DatabaseExists()
        {
            return File.Exists(DbPath);
        }

        /// <summary>
        /// Выполняет команду INSERT/UPDATE/DELETE, возвращает кол-во затронутых строк
        /// </summary>
        public static int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            using (SqliteConnection conn = new SqliteConnection(ConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Выполняет запрос и возвращает одно значение (скаляр)
        /// </summary>
        public static object ExecuteScalar(string sql, params SqliteParameter[] parameters)
        {
            using (SqliteConnection conn = new SqliteConnection(ConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Выполняет SELECT и возвращает DataTable для привязки к DataGridView
        /// </summary>
        public static DataTable ExecuteDataTable(string sql, params SqliteParameter[] parameters)
        {
            DataTable table = new DataTable();

            using (SqliteConnection conn = new SqliteConnection(ConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    // Создаём колонки
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                    }

                    // Заполняем строки
                    while (reader.Read())
                    {
                        object[] values = new object[reader.FieldCount];
                        reader.GetValues(values);
                        table.Rows.Add(values);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Выполняет несколько SQL-команд в одной транзакции
        /// </summary>
        public static void ExecuteBatch(params string[] sqlCommands)
        {
            using (SqliteConnection conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                using (SqliteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (string sql in sqlCommands)
                        {
                            using (SqliteCommand cmd = new SqliteCommand(sql, conn, transaction))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Создаёт SqliteParameter с заданным именем и значением
        /// </summary>
        public static SqliteParameter CreateParameter(string name, object value)
        {
            if (value == null)
                return new SqliteParameter(name, DBNull.Value);

            return new SqliteParameter(name, value);
        }

        /// <summary>
        /// Безопасно получает значение из DataRow (если null — возвращает default)
        /// </summary>
        public static T GetFieldValue<T>(DataRow row, string columnName)
        {
            if (row == null || row.IsNull(columnName))
                return default(T);

            return (T)row[columnName];
        }
    }
}
