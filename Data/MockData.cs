using System.Collections.Generic;
using TourGuideSmart.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace TourGuideSmart.Data
{
    public static class MockData
    {
        public static List<Tour> GetMockTours()
        {
            // ensure WelcomeImages exists and contains a sample image
            var imagesDir = Path.Combine(AppContext.BaseDirectory, "WelcomeImages");
            if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);
            var samplePath = Path.Combine(imagesDir, "sample.png");
            if (!File.Exists(samplePath))
            {
                // create a simple placeholder image
                using var bmp = new Bitmap(640, 360);
                using var g = Graphics.FromImage(bmp);
                g.Clear(Color.LightGray);
                using var f = new Font("Segoe UI", 24, FontStyle.Bold);
                var text = "Sample Image";
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, Brushes.White, (bmp.Width - sz.Width) / 2, (bmp.Height - sz.Height) / 2);
                bmp.Save(samplePath, ImageFormat.Png);
            }

            // Expandable mock dataset for testing UI and planner logic
            return new List<Tour>
            {
                new Tour { Id = 1, Name = "Ốc Oanh", Price = 50000, Category = "Ốc", Description = "Quán ốc nổi tiếng, phục vụ đa dạng các loại ốc.", ImagePath = samplePath },
                new Tour { Id = 2, Name = "Bún Thái", Price = 45000, Category = "Món nước", Description = "Bún Thái cay đậm đà, phù hợp cho những ai thích vị nồng.", ImagePath = samplePath },
                new Tour { Id = 3, Name = "Trà đào", Price = 25000, Category = "Nước", Description = "Trà đào tươi mát, dùng kèm đá xay.", ImagePath = samplePath },
                new Tour { Id = 4, Name = "Cơm Tấm Sài Gòn", Price = 60000, Category = "Cơm", Description = "Cơm tấm sườn bì chả chuẩn vị.", ImagePath = samplePath },
                new Tour { Id = 5, Name = "Phở Bắc", Price = 55000, Category = "Phở", Description = "Phở nước trong, thơm mùi quế.", ImagePath = samplePath },
                new Tour { Id = 6, Name = "Bánh Mì Minh", Price = 30000, Category = "Bánh mì", Description = "Bánh mì giòn, nhân thơm.", ImagePath = samplePath },
                new Tour { Id = 7, Name = "Hải Sản Biển Xanh", Price = 120000, Category = "Hải sản", Description = "Hải sản tươi sống, giá hợp lý.", ImagePath = samplePath },
                new Tour { Id = 8, Name = "Quán Lẩu 99", Price = 200000, Category = "Lẩu", Description = "Lẩu gia đình cho 4 người.", ImagePath = samplePath },
                new Tour { Id = 9, Name = "Gỏi Cuốn Cô Hạnh", Price = 40000, Category = "Ăn vặt", Description = "Gỏi cuốn tôm thịt tươi ngon.", ImagePath = samplePath },
                new Tour { Id = 10, Name = "Cafe Sáng", Price = 30000, Category = "Cafe", Description = "Cafe pha phin sáng sớm.", ImagePath = samplePath }
            };
        }
    }
}
