namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель дополнительной услуги
    /// </summary>
    public class AdditionalServiceModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string ChargeType { get; set; }   // "Фиксированная" / "За единицу"
        public string UnitName { get; set; }
        public bool IsActive { get; set; }
    }
}
