using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using DispatchLogistics.Models;

namespace DispatchLogistics.DataAccess
{
    public class OrderRepository
    {
        private string SelectOrdersSql()
        {
            return "SELECT o.OrderId AS [ID], o.OrderNumber AS [Номер], " +
                   "o.OrderDate AS [Дата], " +
                   "c.Name AS [Клиент], " +
                   "pf.PointName || ' → ' || pt.PointName AS [Маршрут], " +
                   "t.VehicleNumber AS [Транспорт], " +
                   "o.Status AS [Статус], " +
                   "o.CalculatedAmount AS [Расчётная сумма], " +
                   "o.FinalAmount AS [Итоговая сумма] " +
                   "FROM Orders o " +
                   "JOIN Clients c ON o.ClientId = c.ClientId " +
                   "JOIN GeoPoints pf ON o.PointFromId = pf.GeoPointId " +
                   "JOIN GeoPoints pt ON o.PointToId = pt.GeoPointId " +
                   "JOIN Transport t ON o.TransportId = t.TransportId";
        }

        public DataTable GetAllOrders()
        {
            return DatabaseHelper.ExecuteDataTable(SelectOrdersSql() + " ORDER BY o.OrderDate DESC");
        }

        public OrderModel GetOrderById(int orderId)
        {
            string sql = "SELECT o.*, c.Name AS ClientName, " +
                         "pf.PointName AS PointFromName, pt.PointName AS PointToName, " +
                         "t.VehicleNumber || ' — ' || t.Model AS TransportInfo, " +
                         "tr.TariffName " +
                         "FROM Orders o " +
                         "JOIN Clients c ON o.ClientId = c.ClientId " +
                         "JOIN GeoPoints pf ON o.PointFromId = pf.GeoPointId " +
                         "JOIN GeoPoints pt ON o.PointToId = pt.GeoPointId " +
                         "JOIN Transport t ON o.TransportId = t.TransportId " +
                         "JOIN Tariffs tr ON o.TariffId = tr.TariffId " +
                         "WHERE o.OrderId = $Id";

            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", orderId));
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public DataTable GetOrdersByDateRange(DateTime dateFrom, DateTime dateTo)
        {
            string sql = SelectOrdersSql() +
                         " WHERE o.OrderDate >= $From AND o.OrderDate <= $To ORDER BY o.OrderDate DESC";
            return DatabaseHelper.ExecuteDataTable(
                sql,
                DatabaseHelper.CreateParameter("$From", dateFrom.ToString("yyyy-MM-dd")),
                DatabaseHelper.CreateParameter("$To", dateTo.ToString("yyyy-MM-dd")));
        }

        public DataTable GetOrdersByStatus(string status)
        {
            string sql = SelectOrdersSql() + " WHERE o.Status = $Status ORDER BY o.OrderDate DESC";
            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Status", status));
        }

        public DataTable SearchOrders(string search)
        {
            string sql = SelectOrdersSql() + " WHERE o.OrderNumber LIKE $Search ORDER BY o.OrderDate DESC";
            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Search", "%" + search + "%"));
        }

        public DataTable GetOrdersByClient(int clientId)
        {
            string sql = SelectOrdersSql() + " WHERE o.ClientId = $Id ORDER BY o.OrderDate DESC";
            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", clientId));
        }

        public string GenerateOrderNumber()
        {
            string sql = "SELECT COALESCE(MAX(CAST(SUBSTR(OrderNumber, 9) AS INTEGER)), 0) + 1 " +
                         "FROM Orders WHERE OrderNumber LIKE 'ЗК-%'";
            object result = DatabaseHelper.ExecuteScalar(sql);
            int nextNum = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;
            return string.Format("ЗК-2026-{0:D4}", nextNum);
        }

        public int InsertOrder(OrderModel order)
        {
            string sql = "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, " +
                         "TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, " +
                         "CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) " +
                         "VALUES ($Num, $Date, $Client, $PF, $PT, $Trans, $Tariff, $Dist, $Weight, $Idle, " +
                         "$CalcAmt, $FinalAmt, $Reason, $Status, $Notes); SELECT last_insert_rowid()";

            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$Num", order.OrderNumber),
                DatabaseHelper.CreateParameter("$Date", order.OrderDate.ToString("yyyy-MM-dd")),
                DatabaseHelper.CreateParameter("$Client", order.ClientId),
                DatabaseHelper.CreateParameter("$PF", order.PointFromId),
                DatabaseHelper.CreateParameter("$PT", order.PointToId),
                DatabaseHelper.CreateParameter("$Trans", order.TransportId),
                DatabaseHelper.CreateParameter("$Tariff", order.TariffId),
                DatabaseHelper.CreateParameter("$Dist", order.DistanceKm),
                DatabaseHelper.CreateParameter("$Weight", order.CargoWeight.HasValue ? (object)order.CargoWeight.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Idle", order.IdleHours.HasValue ? (object)order.IdleHours.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$CalcAmt", order.CalculatedAmount),
                DatabaseHelper.CreateParameter("$FinalAmt", order.FinalAmount),
                DatabaseHelper.CreateParameter("$Reason", (object)order.ManualAdjustmentReason ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Status", order.Status),
                DatabaseHelper.CreateParameter("$Notes", (object)order.Notes ?? DBNull.Value));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void UpdateOrder(OrderModel order)
        {
            string sql = "UPDATE Orders SET OrderNumber = $Num, OrderDate = $Date, " +
                         "ClientId = $Client, PointFromId = $PF, PointToId = $PT, " +
                         "TransportId = $Trans, TariffId = $Tariff, DistanceKm = $Dist, " +
                         "CargoWeight = $Weight, IdleHours = $Idle, " +
                         "CalculatedAmount = $CalcAmt, FinalAmount = $FinalAmt, " +
                         "ManualAdjustmentReason = $Reason, Status = $Status, " +
                         "Notes = $Notes WHERE OrderId = $Id";

            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", order.OrderId),
                DatabaseHelper.CreateParameter("$Num", order.OrderNumber),
                DatabaseHelper.CreateParameter("$Date", order.OrderDate.ToString("yyyy-MM-dd")),
                DatabaseHelper.CreateParameter("$Client", order.ClientId),
                DatabaseHelper.CreateParameter("$PF", order.PointFromId),
                DatabaseHelper.CreateParameter("$PT", order.PointToId),
                DatabaseHelper.CreateParameter("$Trans", order.TransportId),
                DatabaseHelper.CreateParameter("$Tariff", order.TariffId),
                DatabaseHelper.CreateParameter("$Dist", order.DistanceKm),
                DatabaseHelper.CreateParameter("$Weight", order.CargoWeight.HasValue ? (object)order.CargoWeight.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Idle", order.IdleHours.HasValue ? (object)order.IdleHours.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$CalcAmt", order.CalculatedAmount),
                DatabaseHelper.CreateParameter("$FinalAmt", order.FinalAmount),
                DatabaseHelper.CreateParameter("$Reason", (object)order.ManualAdjustmentReason ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$Status", order.Status),
                DatabaseHelper.CreateParameter("$Notes", (object)order.Notes ?? DBNull.Value));
        }

        public void UpdateOrderStatus(int orderId, string newStatus)
        {
            string getStatusSql = "SELECT Status FROM Orders WHERE OrderId = $Id";
            object oldStatusObj = DatabaseHelper.ExecuteScalar(
                getStatusSql, DatabaseHelper.CreateParameter("$Id", orderId));
            string oldStatus = oldStatusObj != null && oldStatusObj != DBNull.Value ? oldStatusObj.ToString() : null;

            string sql = "UPDATE Orders SET Status = $Status WHERE OrderId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$Id", orderId),
                DatabaseHelper.CreateParameter("$Status", newStatus));

            var historyRepo = new OrderStatusHistoryRepository();
            historyRepo.AddHistory(orderId, oldStatus, newStatus, null, null);
        }

        public void DeleteOrder(int orderId)
        {
            string sql = "DELETE FROM Orders WHERE OrderId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", orderId));
        }

        public List<OrderServiceModel> GetOrderServices(int orderId)
        {
            string sql = "SELECT os.OrderServiceId, os.OrderId, os.ServiceId, " +
                         "s.ServiceName, os.Quantity, os.Price, os.Total " +
                         "FROM OrderServices os " +
                         "JOIN AdditionalServices s ON os.ServiceId = s.ServiceId " +
                         "WHERE os.OrderId = $Id";

            DataTable table = DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", orderId));

            List<OrderServiceModel> list = new List<OrderServiceModel>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new OrderServiceModel
                {
                    OrderServiceId = Convert.ToInt32(row["OrderServiceId"]),
                    OrderId = Convert.ToInt32(row["OrderId"]),
                    ServiceId = Convert.ToInt32(row["ServiceId"]),
                    ServiceName = row["ServiceName"].ToString(),
                    Quantity = Convert.ToDecimal(row["Quantity"]),
                    Price = Convert.ToDecimal(row["Price"]),
                    Total = Convert.ToDecimal(row["Total"])
                });
            }
            return list;
        }

        public int AddOrderService(int orderId, int serviceId, decimal quantity, decimal price, decimal total)
        {
            string sql = "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) " +
                         "VALUES ($OId, $SId, $Qty, $Price, $Total); SELECT last_insert_rowid()";
            object result = DatabaseHelper.ExecuteScalar(sql,
                DatabaseHelper.CreateParameter("$OId", orderId),
                DatabaseHelper.CreateParameter("$SId", serviceId),
                DatabaseHelper.CreateParameter("$Qty", quantity),
                DatabaseHelper.CreateParameter("$Price", price),
                DatabaseHelper.CreateParameter("$Total", total));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void DeleteOrderService(int orderServiceId)
        {
            string sql = "DELETE FROM OrderServices WHERE OrderServiceId = $Id";
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.CreateParameter("$Id", orderServiceId));
        }

        public void ReplaceOrderServices(int orderId, List<OrderServiceModel> services)
        {
            string deleteSql = "DELETE FROM OrderServices WHERE OrderId = $Id";
            DatabaseHelper.ExecuteNonQuery(deleteSql, DatabaseHelper.CreateParameter("$Id", orderId));
            foreach (var s in services)
                AddOrderService(orderId, s.ServiceId, s.Quantity, s.Price, s.Total);
        }

        private OrderModel MapRow(DataRow row)
        {
            return new OrderModel
            {
                OrderId = Convert.ToInt32(row["OrderId"]),
                OrderNumber = row["OrderNumber"].ToString(),
                OrderDate = row["OrderDate"] != DBNull.Value ? DateTime.Parse(row["OrderDate"].ToString()) : DateTime.Now,
                ClientId = Convert.ToInt32(row["ClientId"]),
                ClientName = row["ClientName"].ToString(),
                PointFromId = Convert.ToInt32(row["PointFromId"]),
                PointFromName = row["PointFromName"].ToString(),
                PointToId = Convert.ToInt32(row["PointToId"]),
                PointToName = row["PointToName"].ToString(),
                TransportId = Convert.ToInt32(row["TransportId"]),
                TransportInfo = row["TransportInfo"].ToString(),
                TariffId = Convert.ToInt32(row["TariffId"]),
                TariffName = row["TariffName"].ToString(),
                DistanceKm = Convert.ToDecimal(row["DistanceKm"]),
                CargoWeight = row["CargoWeight"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["CargoWeight"]) : null,
                IdleHours = row["IdleHours"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["IdleHours"]) : null,
                CalculatedAmount = Convert.ToDecimal(row["CalculatedAmount"]),
                FinalAmount = Convert.ToDecimal(row["FinalAmount"]),
                ManualAdjustmentReason = row["ManualAdjustmentReason"] != DBNull.Value ? row["ManualAdjustmentReason"].ToString() : null,
                Status = row["Status"].ToString(),
                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null
            };
        }
    }
}
