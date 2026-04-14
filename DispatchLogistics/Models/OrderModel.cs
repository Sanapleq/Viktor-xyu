using System;

namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель заказа
    /// </summary>
    public class OrderModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }           // для отображения
        public int PointFromId { get; set; }
        public string PointFromName { get; set; }        // для отображения
        public int PointToId { get; set; }
        public string PointToName { get; set; }          // для отображения
        public int TransportId { get; set; }
        public string TransportInfo { get; set; }        // для отображения (номер + модель)
        public int TariffId { get; set; }
        public string TariffName { get; set; }           // для отображения
        public decimal DistanceKm { get; set; }
        public decimal? CargoWeight { get; set; }
        public decimal? IdleHours { get; set; }
        public decimal CalculatedAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string ManualAdjustmentReason { get; set; }
        public string Status { get; set; }               // "Новый" / "Подтвержден" / "В пути" / "Завершен" / "Отменен"
        public string Notes { get; set; }
    }
}
