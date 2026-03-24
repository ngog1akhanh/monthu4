using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace TourGuideSmart
{
    public class FormWelcome : Form
    {
        Button btnStart = new Button();
        private PictureBox pbBackground = new PictureBox();
        private string[] imageFiles = Array.Empty<string>();
        private int currentIndex = 0;
        private Bitmap? currentBmp = null;

        public FormWelcome()
        {
            this.Text = "Welcome";
            this.Width = 800;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Background picture box (slideshow)
            pbBackground.Dock = DockStyle.Fill;
            pbBackground.SizeMode = PictureBoxSizeMode.StretchImage;
            pbBackground.BackColor = System.Drawing.Color.LightGray;
            pbBackground.Enabled = false; // background only, don't receive input
            this.Controls.Add(pbBackground);
            pbBackground.SendToBack();

            // Start button (overlay)
            btnStart.Text = "Start";
            btnStart.Width = 140;
            btnStart.Height = 40;
            btnStart.Top = this.ClientSize.Height - btnStart.Height - 20;
            btnStart.Left = (this.ClientSize.Width - btnStart.Width) / 2;
            btnStart.Anchor = AnchorStyles.Bottom;
            btnStart.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            btnStart.ForeColor = System.Drawing.Color.White;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click;

            // Add button after background so it sits on top
            this.Controls.Add(btnStart);
            // ensure the start button stays above the background and is centered on resize
            btnStart.BringToFront();
            this.Resize += (s, e) =>
            {
                btnStart.Top = this.ClientSize.Height - btnStart.Height - 20;
                btnStart.Left = (this.ClientSize.Width - btnStart.Width) / 2;
                btnStart.BringToFront();
                // apply adaptive layout
                TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
            };

            // Load images from WelcomeImages folder next to executable
            try
            {
                var imagesDir = Path.Combine(AppContext.BaseDirectory, "WelcomeImages");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                var exts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                imageFiles = Directory.GetFiles(imagesDir)
                    .Where(f => exts.Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();

                if (imageFiles.Length > 0)
                {
                    // show first image
                    currentIndex = 0;
                    LoadCurrentBitmap();
                    if (currentBmp != null)
                    {
                        pbBackground.Image = (Image)currentBmp.Clone();
                    }

                    // show first image (no animation)
                    // currentBmp already loaded and set above
                }
            }
            catch
            {
                // ignore errors loading images
            }
        }
        private void LoadCurrentBitmap()
        {
            // dispose previous
            currentBmp?.Dispose();
            currentBmp = null;
            try
            {
                var path = imageFiles[currentIndex];
                using var img = System.Drawing.Image.FromFile(path);
                currentBmp = new Bitmap(img);
            }
            catch
            {
                currentBmp = null;
            }
        }
        private void BtnStart_Click(object sender, EventArgs e)
        {
            // stop any activity (no timers used anymore)
            // dispose current image
            currentBmp?.Dispose();

            // If the app is running inside FormMain (single-window mode), navigate there instead
            foreach (Form open in Application.OpenForms)
            {
                if (open is FormMain)
                {
                    // navigate within the main host so no new window is created
                    TourGuideSmart.Services.NavigationService.Navigate(new FormHome());
                    return;
                }
            }

            // fallback: standalone mode, open FormHome as a window
            var f = new FormHome();
            f.Show();
            this.Hide();
        }
    }
}
