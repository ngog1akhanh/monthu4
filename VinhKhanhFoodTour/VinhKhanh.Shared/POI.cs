namespace VinhKhanh.Shared.Models
{
    public class POI
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description_VI { get; set; } = string.Empty;
        public string Description_EN { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? QRCodePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}