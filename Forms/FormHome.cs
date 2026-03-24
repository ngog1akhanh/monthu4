using System;
using System.Windows.Forms;

namespace TourGuideSmart
{
    public class FormHome : Form
    {
        Button btnTour = new Button();
        Button btnPlanner = new Button();

        public FormHome()
        {
            this.Text = "Home";
            this.Width = 600;
            this.Height = 400;
            btnTour.Text = "Danh sách quán";
            btnTour.Width = 180;
            btnTour.Height = 40;
            btnTour.Click += (s, e) =>
            {
                // create and navigate
                var f = new FormTourList();
                f.Owner = this;
                foreach (Form open in Application.OpenForms)
                {
                    if (open is FormMain fm)
                    {
                        TourGuideSmart.Services.NavigationService.Navigate(f);
                        return;
                    }
                }
                f.Show();
            };

            btnPlanner.Text = "Gợi ý Tour";
            btnPlanner.Width = 180;
            btnPlanner.Height = 40;
            btnPlanner.Click += (s, e) =>
            {
                var f = new FormPlanner();
                f.Owner = this;
                foreach (Form open in Application.OpenForms)
                {
                    if (open is FormMain fm)
                    {
                        TourGuideSmart.Services.NavigationService.Navigate(f);
                        return;
                    }
                }
                f.Show();
            };

            // center buttons horizontally and stack vertically in the middle area
            this.Controls.Add(btnTour);
            this.Controls.Add(btnPlanner);
            void Reposition()
            {
                int cx = Math.Max(0, (this.ClientSize.Width - btnTour.Width) / 2);
                int startY = Math.Max(20, (this.ClientSize.Height - (btnTour.Height + 12 + btnPlanner.Height)) / 2);
                btnTour.Left = cx;
                btnTour.Top = startY;
                btnPlanner.Left = cx;
                btnPlanner.Top = startY + btnTour.Height + 12;
            }

            this.Resize += (s, e) => Reposition();
            this.Shown += (s, e) => Reposition();
            this.Resize += (s, e) => TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
            this.Shown += (s, e) => TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
        }
    }
}