using System;
using System.Data;
using Microsoft.Data.Sqlite;
using DispatchLogistics.Models;

namespace DispatchLogistics.DataAccess
{
    /// <summary>
    /// Репозиторий для работы с клиентами (SQLite)
    /// </summary>
    public class ClientRepository
    {
        public DataTable GetAllClients()
        {
            string sql = "SELECT ClientId AS [ID], ClientType AS [Тип], Name AS [Название], " +
                         "ContactPerson AS [Контактное лицо], Phone AS [Телефон], Email AS [Email], " +
                         "Address AS [Адрес], ContractNumber AS [Договор №], " +
                         "ContractDate AS [Дата договора] FROM Clients ORDER BY Name";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public ClientModel GetById(int clientId)
        {
            string sql = "SELECT * FROM Clients WHERE ClientId = $Id";
            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", clientId));

            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public DataTable SearchClients(string search)
        {
            string sql = "SELECT ClientId AS [ID], ClientType AS [Тип], Name AS [Название], " +
                         "ContactPerson AS [Контактное лицо], Phone AS [Телефон], Email AS [Email], " +
                         "Address AS [Адрес], ContractNumber AS [Договор №], " +
                         "ContractDate AS [Дата договора] FROM Clients " +
                         "WHERE Name LIKE $Search OR ContactPerson LIKE $Search ORDER BY Name";
            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Search", "%" + search + "%"));
        }

        public int InsertClient(ClientModel client)
        {
            string sql = "INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, " +
                         "Address, ContractNumber, ContractDate, Notes) " +
                         "VALUES ($ClientType, $Name, $ContactPerson, $Phone, $Email, " +
                         "$Address, $ContractNumber, $ContractDate, $Notes); SELECT last_insert_rowid()";

            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$ClientType", client.ClientType),
                DatabaseHelper.CreateParameter("$Name", client.Name),
                DatabaseHelper.CreateParameter("$ContactPerson", (object)client.ContactPerson ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Phone", (object)client.Phone ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Email", (object)client.Email ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Address", (object)client.Address ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$ContractNumber", (object)client.ContractNumber ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$ContractDate", client.ContractDate.HasValue ? (object)client.ContractDate.Value.ToString("yyyy-MM-dd") : DBNull.Value),
                DatabaseHelper.CreateParameter("$Notes", (object)client.Notes ?? DBNull.Value));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateClient(ClientModel client)
        {
            string sql = "UPDATE Clients SET ClientType = $ClientType, Name = $Name, " +
                         "ContactPerson = $ContactPerson, Phone = $Phone, Email = $Email, " +
                         "Address = $Address, ContractNumber = $ContractNumber, " +
                         "ContractDate = $ContractDate, Notes = $Notes WHERE ClientId = $ClientId";

            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$ClientId", client.ClientId),
                DatabaseHelper.CreateParameter("$ClientType", client.ClientType),
                DatabaseHelper.CreateParameter("$Name", client.Name),
                DatabaseHelper.CreateParameter("$ContactPerson", (object)client.ContactPerson ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Phone", (object)client.Phone ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Email", (object)client.Email ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Address", (object)client.Address ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$ContractNumber", (object)client.ContractNumber ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$ContractDate", client.ContractDate.HasValue ? (object)client.ContractDate.Value.ToString("yyyy-MM-dd") : DBNull.Value),
                DatabaseHelper.CreateParameter("$Notes", (object)client.Notes ?? DBNull.Value));
        }

        public bool CanDelete(int clientId)
        {
            string sql = "SELECT COUNT(*) FROM Orders WHERE ClientId = $Id";
            object result = DatabaseHelper.ExecuteScalar(
                sql, DatabaseHelper.CreateParameter("$Id", clientId));
            return result != null && Convert.ToInt32(result) == 0;
        }

        public void DeleteClient(int clientId)
        {
            string sql = "DELETE FROM Clients WHERE ClientId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", clientId));
        }

        public DataTable GetClientsForCombo()
        {
            string sql = "SELECT ClientId, Name FROM Clients ORDER BY Name";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        private ClientModel MapRow(DataRow row)
        {
            return new ClientModel
            {
                ClientId = Convert.ToInt32(row["ClientId"]),
                ClientType = row["ClientType"].ToString(),
                Name = row["Name"].ToString(),
                ContactPerson = row["ContactPerson"] != DBNull.Value ? row["ContactPerson"].ToString() : null,
                Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : null,
                Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : null,
                ContractNumber = row["ContractNumber"] != DBNull.Value ? row["ContractNumber"].ToString() : null,
                ContractDate = row["ContractDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(row["ContractDate"].ToString()) : null,
                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null
            };
        }
    }
}
