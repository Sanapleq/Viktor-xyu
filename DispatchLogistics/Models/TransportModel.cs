namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель транспорта
    /// </summary>
    public class TransportModel
    {
        public int TransportId { get; set; }
        public string VehicleNumber { get; set; }
        public string Model { get; set; }
        public string BodyType { get; set; }
        public decimal CapacityTons { get; set; }
        public decimal FuelConsumption { get; set; }
        public decimal CostPerKm { get; set; }
        public decimal IdleHourCost { get; set; }
        public string Status { get; set; }          // "Свободен" / "В рейсе" / "На ремонте"
        public string Notes { get; set; }
    }
}
