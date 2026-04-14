using System;
using System.Data;
using Microsoft.Data.Sqlite;
using DispatchLogistics.Models;

namespace DispatchLogistics.DataAccess
{
    public class AdditionalServiceRepository
    {
        public DataTable GetAllServices()
        {
            string sql = "SELECT ServiceId AS [ID], ServiceName AS [Название], " +
                         "Price AS [Цена], ChargeType AS [Тип начисления], " +
                         "UnitName AS [Ед. изм.], IsActive AS [Активна] " +
                         "FROM AdditionalServices ORDER BY ServiceName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public AdditionalServiceModel GetById(int serviceId)
        {
            string sql = "SELECT * FROM AdditionalServices WHERE ServiceId = $Id";
            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", serviceId));
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public DataTable GetActiveServices()
        {
            string sql = "SELECT ServiceId, ServiceName, Price, ChargeType, UnitName " +
                         "FROM AdditionalServices WHERE IsActive = 1 ORDER BY ServiceName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int InsertService(AdditionalServiceModel s)
        {
            string sql = "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) " +
                         "VALUES ($Name, $Price, $ChargeType, $Unit, $Active); SELECT last_insert_rowid()";
            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$Name", s.ServiceName),
                DatabaseHelper.CreateParameter("$Price", s.Price),
                DatabaseHelper.CreateParameter("$ChargeType", s.ChargeType),
                DatabaseHelper.CreateParameter("$Unit", (object)s.UnitName ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Active", s.IsActive ? 1 : 0));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateService(AdditionalServiceModel s)
        {
            string sql = "UPDATE AdditionalServices SET ServiceName = $Name, " +
                         "Price = $Price, ChargeType = $ChargeType, UnitName = $Unit, " +
                         "IsActive = $Active WHERE ServiceId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", s.ServiceId),
                DatabaseHelper.CreateParameter("$Name", s.ServiceName),
                DatabaseHelper.CreateParameter("$Price", s.Price),
                DatabaseHelper.CreateParameter("$ChargeType", s.ChargeType),
                DatabaseHelper.CreateParameter("$Unit", (object)s.UnitName ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Active", s.IsActive ? 1 : 0));
        }

        public void DeleteService(int serviceId)
        {
            string sql = "DELETE FROM AdditionalServices WHERE ServiceId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", serviceId));
        }

        private AdditionalServiceModel MapRow(DataRow row)
        {
            return new AdditionalServiceModel
            {
                ServiceId = Convert.ToInt32(row["ServiceId"]),
                ServiceName = row["ServiceName"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                ChargeType = row["ChargeType"].ToString(),
                UnitName = row["UnitName"] != DBNull.Value ? row["UnitName"].ToString() : null,
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1
            };
        }
    }
}
