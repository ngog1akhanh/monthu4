using System;
using System.Windows.Forms;

namespace TourGuideSmart
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // ensure a sample image exists for mock data
            try
            {
                var imagesDir = System.IO.Path.Combine(AppContext.BaseDirectory, "WelcomeImages");
                if (!System.IO.Directory.Exists(imagesDir)) System.IO.Directory.CreateDirectory(imagesDir);
                var samplePath = System.IO.Path.Combine(imagesDir, "sample.png");
                if (!System.IO.File.Exists(samplePath))
                {
                    using var bmp = new System.Drawing.Bitmap(640, 360);
                    using var g = System.Drawing.Graphics.FromImage(bmp);
                    g.Clear(System.Drawing.Color.DarkSlateGray);
                    using var f = new System.Drawing.Font("Segoe UI", 28, System.Drawing.FontStyle.Bold);
                    var text = "Sample Image";
                    var sz = g.MeasureString(text, f);
                    g.DrawString(text, f, System.Drawing.Brushes.White, (bmp.Width - sz.Width) / 2, (bmp.Height - sz.Height) / 2);
                    bmp.Save(samplePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch { }

            Application.Run(new FormMain());
        }
    }
}
