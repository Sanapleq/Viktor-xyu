using System;
using System.Data;
using Microsoft.Data.Sqlite;
using DispatchLogistics.Models;

namespace DispatchLogistics.DataAccess
{
    /// <summary>
    /// Репозиторий для работы с транспортом (SQLite)
    /// </summary>
    public class TransportRepository
    {
        public DataTable GetAllTransport()
        {
            string sql = "SELECT TransportId AS [ID], VehicleNumber AS [Гос. номер], " +
                         "Model AS [Марка/Модель], BodyType AS [Тип кузова], " +
                         "CapacityTons AS [Грузоподъёмность, т], FuelConsumption AS [Расход л/100км], " +
                         "CostPerKm AS [Стоимость км], IdleHourCost AS [Стоимость часа простоя], " +
                         "Status AS [Статус] FROM Transport ORDER BY VehicleNumber";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public TransportModel GetById(int transportId)
        {
            string sql = "SELECT * FROM Transport WHERE TransportId = $Id";
            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", transportId));
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public DataTable GetByStatus(string status)
        {
            string sql = "SELECT TransportId AS [ID], VehicleNumber AS [Гос. номер], " +
                         "Model AS [Марка/Модель], BodyType AS [Тип кузова], " +
                         "CapacityTons AS [Грузоподъёмность, т], FuelConsumption AS [Расход л/100км], " +
                         "CostPerKm AS [Стоимость км], IdleHourCost AS [Стоимость часа простоя], " +
                         "Status AS [Статус] FROM Transport WHERE Status = $Status ORDER BY VehicleNumber";
            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Status", status));
        }

        public DataTable SearchTransport(string search)
        {
            string sql = "SELECT TransportId AS [ID], VehicleNumber AS [Гос. номер], " +
                         "Model AS [Марка/Модель], BodyType AS [Тип кузова], " +
                         "CapacityTons AS [Грузоподъёмность, т], FuelConsumption AS [Расход л/100км], " +
                         "CostPerKm AS [Стоимость км], IdleHourCost AS [Стоимость часа простоя], " +
                         "Status AS [Статус] FROM Transport " +
                         "WHERE VehicleNumber LIKE $Search OR Model LIKE $Search ORDER BY VehicleNumber";
            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Search", "%" + search + "%"));
        }

        public int InsertTransport(TransportModel t)
        {
            string sql = "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, " +
                         "FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) " +
                         "VALUES ($VNum, $Model, $BodyType, $Cap, $Fuel, $CPK, $IHC, $Status, $Notes); SELECT last_insert_rowid()";
            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$VNum", t.VehicleNumber),
                DatabaseHelper.CreateParameter("$Model", t.Model),
                DatabaseHelper.CreateParameter("$BodyType", t.BodyType),
                DatabaseHelper.CreateParameter("$Cap", t.CapacityTons),
                DatabaseHelper.CreateParameter("$Fuel", t.FuelConsumption),
                DatabaseHelper.CreateParameter("$CPK", t.CostPerKm),
                DatabaseHelper.CreateParameter("$IHC", t.IdleHourCost),
                DatabaseHelper.CreateParameter("$Status", t.Status),
                DatabaseHelper.CreateParameter("$Notes", (object)t.Notes ?? DBNull.Value));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateTransport(TransportModel t)
        {
            string sql = "UPDATE Transport SET VehicleNumber = $VNum, Model = $Model, " +
                         "BodyType = $BodyType, CapacityTons = $Cap, FuelConsumption = $Fuel, " +
                         "CostPerKm = $CPK, IdleHourCost = $IHC, Status = $Status, Notes = $Notes " +
                         "WHERE TransportId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", t.TransportId),
                DatabaseHelper.CreateParameter("$VNum", t.VehicleNumber),
                DatabaseHelper.CreateParameter("$Model", t.Model),
                DatabaseHelper.CreateParameter("$BodyType", t.BodyType),
                DatabaseHelper.CreateParameter("$Cap", t.CapacityTons),
                DatabaseHelper.CreateParameter("$Fuel", t.FuelConsumption),
                DatabaseHelper.CreateParameter("$CPK", t.CostPerKm),
                DatabaseHelper.CreateParameter("$IHC", t.IdleHourCost),
                DatabaseHelper.CreateParameter("$Status", t.Status),
                DatabaseHelper.CreateParameter("$Notes", (object)t.Notes ?? DBNull.Value));
        }

        public void DeleteTransport(int transportId)
        {
            string sql = "DELETE FROM Transport WHERE TransportId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", transportId));
        }

        public DataTable GetAvailableTransport()
        {
            string sql = "SELECT TransportId, VehicleNumber || ' — ' || Model AS TransportInfo " +
                         "FROM Transport WHERE Status = 'Свободен' ORDER BY VehicleNumber";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public DataTable GetAllForCombo()
        {
            string sql = "SELECT TransportId, VehicleNumber || ' — ' || Model AS TransportInfo " +
                         "FROM Transport ORDER BY VehicleNumber";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        private TransportModel MapRow(DataRow row)
        {
            return new TransportModel
            {
                TransportId = Convert.ToInt32(row["TransportId"]),
                VehicleNumber = row["VehicleNumber"].ToString(),
                Model = row["Model"].ToString(),
                BodyType = row["BodyType"].ToString(),
                CapacityTons = Convert.ToDecimal(row["CapacityTons"]),
                FuelConsumption = Convert.ToDecimal(row["FuelConsumption"]),
                CostPerKm = Convert.ToDecimal(row["CostPerKm"]),
                IdleHourCost = Convert.ToDecimal(row["IdleHourCost"]),
                Status = row["Status"].ToString(),
                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null
            };
        }
    }
}
