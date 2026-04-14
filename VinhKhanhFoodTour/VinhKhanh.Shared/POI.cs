namespace VinhKhanh.Shared.Models
{
    public class POI
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description_VI { get; set; } // Tiếng Việt
        public string Description_EN { get; set; } // Tiếng Anh (Mới)
        public string Description_ZH { get; set; } // Tiếng Trung (Mới)
        public string Description_JA { get; set; } // Tiếng Nhật (Mới)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? QRCodePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public double Radius { get; set; } = 50; // Bán kính kích hoạt (mặc định 50 mét)
        public bool IsPlayed { get; set; } = false; // Cờ đánh dấu: Đã phát âm thanh chưa?
        public string ImageUrl { get; set; } = "https://via.placeholder.com/300x200.png?text=No+Image";
    }
}