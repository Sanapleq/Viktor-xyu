using System;
using System.Data;
using Microsoft.Data.Sqlite;
using DispatchLogistics.Models;

namespace DispatchLogistics.DataAccess
{
    public class TariffRepository
    {
        public DataTable GetAllTariffs()
        {
            string sql = "SELECT TariffId AS [ID], TariffName AS [Название], " +
                         "CalculationType AS [Тип расчёта], " +
                         "COALESCE(CAST(CostPerKm AS TEXT), '—') AS [За км], " +
                         "COALESCE(CAST(CostPerHour AS TEXT), '—') AS [За час], " +
                         "COALESCE(CAST(CostPerTon AS TEXT), '—') AS [За тонну], " +
                         "COALESCE(CAST(FuelSurcharge AS TEXT), '—') AS [Топл. сбор], " +
                         "SeasonalCoefficient AS [Сез. коэф.], " +
                         "IsActive AS [Активен] FROM Tariffs ORDER BY TariffName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public TariffModel GetById(int tariffId)
        {
            string sql = "SELECT * FROM Tariffs WHERE TariffId = $Id";
            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", tariffId));
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public DataTable GetActiveTariffs()
        {
            string sql = "SELECT TariffId, TariffName FROM Tariffs WHERE IsActive = 1 ORDER BY TariffName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int InsertTariff(TariffModel t)
        {
            string sql = "INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, " +
                         "CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) " +
                         "VALUES ($Name, $CalcType, $CPK, $CPH, $CPT, $Fuel, $Season, $Active, $Notes); SELECT last_insert_rowid()";
            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$Name", t.TariffName),
                DatabaseHelper.CreateParameter("$CalcType", t.CalculationType),
                DatabaseHelper.CreateParameter("$CPK", t.CostPerKm.HasValue ? (object)t.CostPerKm.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$CPH", t.CostPerHour.HasValue ? (object)t.CostPerHour.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$CPT", t.CostPerTon.HasValue ? (object)t.CostPerTon.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Fuel", t.FuelSurcharge.HasValue ? (object)t.FuelSurcharge.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Season", t.SeasonalCoefficient),
                DatabaseHelper.CreateParameter("$Active", t.IsActive ? 1 : 0),
                DatabaseHelper.CreateParameter("$Notes", (object)t.Notes ?? DBNull.Value));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateTariff(TariffModel t)
        {
            string sql = "UPDATE Tariffs SET TariffName = $Name, CalculationType = $CalcType, " +
                         "CostPerKm = $CPK, CostPerHour = $CPH, CostPerTon = $CPT, " +
                         "FuelSurcharge = $Fuel, SeasonalCoefficient = $Season, " +
                         "IsActive = $Active, Notes = $Notes WHERE TariffId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", t.TariffId),
                DatabaseHelper.CreateParameter("$Name", t.TariffName),
                DatabaseHelper.CreateParameter("$CalcType", t.CalculationType),
                DatabaseHelper.CreateParameter("$CPK", t.CostPerKm.HasValue ? (object)t.CostPerKm.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$CPH", t.CostPerHour.HasValue ? (object)t.CostPerHour.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$CPT", t.CostPerTon.HasValue ? (object)t.CostPerTon.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Fuel", t.FuelSurcharge.HasValue ? (object)t.FuelSurcharge.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Season", t.SeasonalCoefficient),
                DatabaseHelper.CreateParameter("$Active", t.IsActive ? 1 : 0),
                DatabaseHelper.CreateParameter("$Notes", (object)t.Notes ?? DBNull.Value));
        }

        public void DeleteTariff(int tariffId)
        {
            string sql = "DELETE FROM Tariffs WHERE TariffId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", tariffId));
        }

        private TariffModel MapRow(DataRow row)
        {
            return new TariffModel
            {
                TariffId = Convert.ToInt32(row["TariffId"]),
                TariffName = row["TariffName"].ToString(),
                CalculationType = row["CalculationType"].ToString(),
                CostPerKm = row["CostPerKm"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["CostPerKm"]) : null,
                CostPerHour = row["CostPerHour"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["CostPerHour"]) : null,
                CostPerTon = row["CostPerTon"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["CostPerTon"]) : null,
                FuelSurcharge = row["FuelSurcharge"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["FuelSurcharge"]) : null,
                SeasonalCoefficient = Convert.ToDecimal(row["SeasonalCoefficient"]),
                IsActive = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1,
                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null
            };
        }
    }

    public class GeoPointRepository
    {
        public DataTable GetAllGeoPoints()
        {
            string sql = "SELECT GeoPointId AS [ID], PointName AS [Название], " +
                         "Region AS [Регион], Notes AS [Примечание] FROM GeoPoints ORDER BY PointName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public GeoPointModel GetById(int geoPointId)
        {
            string sql = "SELECT * FROM GeoPoints WHERE GeoPointId = $Id";
            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", geoPointId));
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public DataTable GetAllForCombo()
        {
            string sql = "SELECT GeoPointId, PointName FROM GeoPoints ORDER BY PointName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int InsertGeoPoint(GeoPointModel gp)
        {
            string sql = "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) " +
                         "VALUES ($Name, $Region, $Lat, $Lng, $Notes); SELECT last_insert_rowid()";
            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$Name", gp.PointName),
                DatabaseHelper.CreateParameter("$Region", (object)gp.Region ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Lat", gp.Latitude.HasValue ? (object)gp.Latitude.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Lng", gp.Longitude.HasValue ? (object)gp.Longitude.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Notes", (object)gp.Notes ?? DBNull.Value));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateGeoPoint(GeoPointModel gp)
        {
            string sql = "UPDATE GeoPoints SET PointName = $Name, Region = $Region, " +
                         "Latitude = $Lat, Longitude = $Lng, Notes = $Notes WHERE GeoPointId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", gp.GeoPointId),
                DatabaseHelper.CreateParameter("$Name", gp.PointName),
                DatabaseHelper.CreateParameter("$Region", (object)gp.Region ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Lat", gp.Latitude.HasValue ? (object)gp.Latitude.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Lng", gp.Longitude.HasValue ? (object)gp.Longitude.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Notes", (object)gp.Notes ?? DBNull.Value));
        }

        public void DeleteGeoPoint(int geoPointId)
        {
            string sql = "DELETE FROM GeoPoints WHERE GeoPointId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", geoPointId));
        }

        private GeoPointModel MapRow(DataRow row)
        {
            return new GeoPointModel
            {
                GeoPointId = Convert.ToInt32(row["GeoPointId"]),
                PointName = row["PointName"].ToString(),
                Region = row["Region"] != DBNull.Value ? row["Region"].ToString() : null,
                Latitude = row["Latitude"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["Latitude"]) : null,
                Longitude = row["Longitude"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["Longitude"]) : null,
                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null
            };
        }
    }

    public class DistanceRepository
    {
        public DataTable GetAllDistances()
        {
            string sql = "SELECT d.DistanceId AS [ID], " +
                         "pf.PointName AS [Откуда], pt.PointName AS [Куда], " +
                         "d.DistanceKm AS [Расстояние, км] " +
                         "FROM Distances d " +
                         "JOIN GeoPoints pf ON d.PointFromId = pf.GeoPointId " +
                         "JOIN GeoPoints pt ON d.PointToId = pt.GeoPointId " +
                         "ORDER BY pf.PointName, pt.PointName";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public decimal? GetDistanceBetween(int pointFromId, int pointToId)
        {
            string sql = "SELECT DistanceKm FROM Distances " +
                         "WHERE (PointFromId = $From AND PointToId = $To) " +
                         "   OR (PointFromId = $To AND PointToId = $From)";
            object result = DatabaseHelper.ExecuteScalar(
                sql,
                DatabaseHelper.CreateParameter("$From", pointFromId),
                DatabaseHelper.CreateParameter("$To", pointToId));
            return result != null && result != DBNull.Value ? (decimal?)Convert.ToDecimal(result) : null;
        }

        public int InsertDistance(int pointFromId, int pointToId, decimal distanceKm)
        {
            string sql = "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) " +
                         "VALUES ($From, $To, $Dist); SELECT last_insert_rowid()";
            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$From", pointFromId),
                DatabaseHelper.CreateParameter("$To", pointToId),
                DatabaseHelper.CreateParameter("$Dist", distanceKm));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateDistance(int distanceId, int pointFromId, int pointToId, decimal distanceKm)
        {
            string sql = "UPDATE Distances SET PointFromId = $From, PointToId = $To, " +
                         "DistanceKm = $Dist WHERE DistanceId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", distanceId),
                DatabaseHelper.CreateParameter("$From", pointFromId),
                DatabaseHelper.CreateParameter("$To", pointToId),
                DatabaseHelper.CreateParameter("$Dist", distanceKm));
        }

        public void DeleteDistance(int distanceId)
        {
            string sql = "DELETE FROM Distances WHERE DistanceId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", distanceId));
        }
    }
}
