namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель расстояния между двумя геоточками
    /// </summary>
    public class DistanceModel
    {
        public int DistanceId { get; set; }
        public int PointFromId { get; set; }
        public string PointFromName { get; set; }    // для отображения
        public int PointToId { get; set; }
        public string PointToName { get; set; }      // для отображения
        public decimal DistanceKm { get; set; }
    }
}
