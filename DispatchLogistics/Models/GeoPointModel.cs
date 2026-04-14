namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель геоточки (город/адрес)
    /// </summary>
    public class GeoPointModel
    {
        public int GeoPointId { get; set; }
        public string PointName { get; set; }
        public string Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Region)
                ? PointName
                : string.Format("{0} ({1})", PointName, Region);
        }
    }
}
