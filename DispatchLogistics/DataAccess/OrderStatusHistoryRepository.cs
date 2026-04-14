using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace DispatchLogistics.DataAccess
{
    public class OrderStatusHistoryRepository
    {
        public void AddHistory(int orderId, string oldStatus, string newStatus, int? userId, string comment)
        {
            string sql = "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) " +
                         "VALUES ($OId, $Old, $New, $ChangedAt, $UserId, $Comment)";

            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.CreateParameter("$OId", orderId),
                DatabaseHelper.CreateParameter("$Old", (object)oldStatus ?? DBNull.Value),
                DatabaseHelper.CreateParameter("$New", newStatus),
                DatabaseHelper.CreateParameter("$ChangedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                DatabaseHelper.CreateParameter("$UserId", userId.HasValue ? (object)userId.Value : DBNull.Value),
                DatabaseHelper.CreateParameter("$Comment", (object)comment ?? DBNull.Value));
        }

        public DataTable GetHistoryByOrderId(int orderId)
        {
            string sql = "SELECT h.HistoryId AS [ID], " +
                         "COALESCE(h.OldStatus, '—') AS [Было], " +
                         "h.NewStatus AS [Стало], " +
                         "h.ChangedAt AS [Дата/время], " +
                         "u.FullName AS [Кем изменено], " +
                         "COALESCE(h.Comment, '—') AS [Комментарий] " +
                         "FROM OrderStatusHistory h " +
                         "LEFT JOIN Users u ON h.ChangedByUserId = u.UserId " +
                         "WHERE h.OrderId = $Id " +
                         "ORDER BY h.ChangedAt";

            return DatabaseHelper.ExecuteDataTable(
                sql, DatabaseHelper.CreateParameter("$Id", orderId));
        }
    }

    public class ReportRepository
    {
        public DataTable GetClientTurnover(DateTime dateFrom, DateTime dateTo)
        {
            string sql = "SELECT c.Name AS [Клиент], " +
                         "COUNT(o.OrderId) AS [Кол-во заказов], " +
                         "SUM(o.FinalAmount) AS [Общая сумма, руб.], " +
                         "ROUND(AVG(o.FinalAmount), 2) AS [Средний чек, руб.] " +
                         "FROM Clients c " +
                         "LEFT JOIN Orders o ON c.ClientId = o.ClientId " +
                         "  AND o.OrderDate >= $From AND o.OrderDate <= $To " +
                         "  AND o.Status <> 'Отменен' " +
                         "GROUP BY c.Name " +
                         "ORDER BY [Общая сумма, руб.] DESC";

            return DatabaseHelper.ExecuteDataTable(
                sql,
                DatabaseHelper.CreateParameter("$From", dateFrom.ToString("yyyy-MM-dd")),
                DatabaseHelper.CreateParameter("$To", dateTo.ToString("yyyy-MM-dd")));
        }

        public DataTable GetAverageCheck(DateTime dateFrom, DateTime dateTo)
        {
            string sql = "SELECT " +
                         "COUNT(*) AS [Всего заказов], " +
                         "SUM(o.FinalAmount) AS [Общая выручка, руб.], " +
                         "ROUND(AVG(o.FinalAmount), 2) AS [Средний чек, руб.], " +
                         "ROUND(MIN(o.FinalAmount), 2) AS [Мин. чек, руб.], " +
                         "ROUND(MAX(o.FinalAmount), 2) AS [Макс. чек, руб.] " +
                         "FROM Orders o " +
                         "WHERE o.OrderDate >= $From AND o.OrderDate <= $To " +
                         "  AND o.Status <> 'Отменен'";

            return DatabaseHelper.ExecuteDataTable(
                sql,
                DatabaseHelper.CreateParameter("$From", dateFrom.ToString("yyyy-MM-dd")),
                DatabaseHelper.CreateParameter("$To", dateTo.ToString("yyyy-MM-dd")));
        }

        public DataTable GetTransportUtilization(DateTime dateFrom, DateTime dateTo)
        {
            string sql = "SELECT t.VehicleNumber AS [Гос. номер], " +
                         "t.Model AS [Марка/Модель], " +
                         "t.Status AS [Текущий статус], " +
                         "COUNT(o.OrderId) AS [Кол-во рейсов], " +
                         "COALESCE(SUM(o.FinalAmount), 0) AS [Общая выручка, руб.] " +
                         "FROM Transport t " +
                         "LEFT JOIN Orders o ON t.TransportId = o.TransportId " +
                         "  AND o.OrderDate >= $From AND o.OrderDate <= $To " +
                         "  AND o.Status <> 'Отменен' " +
                         "GROUP BY t.VehicleNumber, t.Model, t.Status " +
                         "ORDER BY [Общая выручка, руб.] DESC";

            return DatabaseHelper.ExecuteDataTable(
                sql,
                DatabaseHelper.CreateParameter("$From", dateFrom.ToString("yyyy-MM-dd")),
                DatabaseHelper.CreateParameter("$To", dateTo.ToString("yyyy-MM-dd")));
        }

        public DataTable GetOrdersStatusCount()
        {
            string sql = "SELECT Status AS [Статус], COUNT(*) AS [Кол-во] " +
                         "FROM Orders GROUP BY Status";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public DataTable GetDashboardCounters()
        {
            string sql = "SELECT " +
                         "(SELECT COUNT(*) FROM Clients) AS ClientCount, " +
                         "(SELECT COUNT(*) FROM Transport) AS TransportCount, " +
                         "(SELECT COUNT(*) FROM Orders WHERE Status <> 'Отменен') AS ActiveOrders, " +
                         "(SELECT COALESCE(SUM(FinalAmount), 0) FROM Orders WHERE Status = 'Завершен') AS TotalRevenue, " +
                         "(SELECT COUNT(*) FROM Orders WHERE Status = 'Новый') AS NewOrders, " +
                         "(SELECT COUNT(*) FROM Orders WHERE Status = 'В пути') AS InTransitOrders";
            return DatabaseHelper.ExecuteDataTable(sql);
        }
    }
}
