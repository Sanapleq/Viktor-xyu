using System;
using System.Data;
using Microsoft.Data.Sqlite;
using DispatchLogistics.Models;

namespace DispatchLogistics.DataAccess
{
    /// <summary>
    /// Репозиторий для работы с пользователями (SQLite)
    /// </summary>
    public class UserRepository
    {
        public UserModel Authenticate(string login, string password)
        {
            string sql = "SELECT UserId, Login, PasswordHash, FullName, Role, IsActive " +
                         "FROM Users WHERE Login = $Login AND PasswordHash = $Password AND IsActive = 1";

            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql,
                DatabaseHelper.CreateParameter("$Login", login),
                DatabaseHelper.CreateParameter("$Password", password));

            if (table.Rows.Count == 0)
                return null;

            return MapRow(table.Rows[0]);
        }

        public DataTable GetAllUsers()
        {
            string sql = "SELECT UserId AS [ID], Login AS [Логин], FullName AS [ФИО], " +
                         "Role AS [Роль], IsActive AS [Активен] FROM Users ORDER BY UserId";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public DataTable GetAllUsersFull()
        {
            string sql = "SELECT UserId, Login, PasswordHash, FullName, Role, IsActive FROM Users ORDER BY UserId";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int InsertUser(string login, string password, string fullName, string role, bool isActive)
        {
            string sql = "INSERT INTO Users (Login, PasswordHash, FullName, Role, IsActive) " +
                         "VALUES ($Login, $PasswordHash, $FullName, $Role, $IsActive); SELECT last_insert_rowid()";

            object result = DatabaseHelper.ExecuteScalar(
                sql,
                DatabaseHelper.CreateParameter("$Login", login),
                DatabaseHelper.CreateParameter("$PasswordHash", password),
                DatabaseHelper.CreateParameter("$FullName", fullName),
                DatabaseHelper.CreateParameter("$Role", role),
                DatabaseHelper.CreateParameter("$IsActive", isActive ? 1 : 0));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateUser(int userId, string login, string password, string fullName, string role, bool isActive)
        {
            string sql = "UPDATE Users SET Login = $Login, PasswordHash = $PasswordHash, " +
                         "FullName = $FullName, Role = $Role, IsActive = $IsActive WHERE UserId = $UserId";

            DatabaseHelper.ExecuteNonQuery(
                sql,
                DatabaseHelper.CreateParameter("$UserId", userId),
                DatabaseHelper.CreateParameter("$Login", login),
                DatabaseHelper.CreateParameter("$PasswordHash", password),
                DatabaseHelper.CreateParameter("$FullName", fullName),
                DatabaseHelper.CreateParameter("$Role", role),
                DatabaseHelper.CreateParameter("$IsActive", isActive ? 1 : 0));
        }

        public void DeleteUser(int userId)
        {
            string sql = "DELETE FROM Users WHERE UserId = $UserId";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$UserId", userId));
        }

        private UserModel MapRow(DataRow row)
        {
            return new UserModel
            {
                UserId = Convert.ToInt32(row["UserId"]),
                Login = row["Login"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                FullName = row["FullName"].ToString(),
                Role = row["Role"].ToString(),
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1
            };
        }
    }
}
