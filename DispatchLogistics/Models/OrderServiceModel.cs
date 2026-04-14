namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель связи заказа с дополнительной услугой
    /// </summary>
    public class OrderServiceModel
    {
        public int OrderServiceId { get; set; }
        public int OrderId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }    // для отображения
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
