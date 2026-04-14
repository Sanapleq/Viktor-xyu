namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель тарифа
    /// </summary>
    public class TariffModel
    {
        public int TariffId { get; set; }
        public string TariffName { get; set; }
        public string CalculationType { get; set; }  // "За км" / "За час" / "За тонну" / "Смешанный"
        public decimal? CostPerKm { get; set; }
        public decimal? CostPerHour { get; set; }
        public decimal? CostPerTon { get; set; }
        public decimal? FuelSurcharge { get; set; }
        public decimal SeasonalCoefficient { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}
