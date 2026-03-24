using System;
using System.Drawing;
using System.Windows.Forms;
using TourGuideSmart.Services;

namespace TourGuideSmart
{
    public class FormMain : Form
    {
        private Panel topBar = new Panel();
        private Button btnBack = new Button();
        private Button btnHome = new Button();
        private Panel contentHost = new Panel();

        public FormMain()
        {
            this.Text = "TourGuideSmart";
            this.Width = 1000;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;

            topBar.Height = 48;
            topBar.Dock = DockStyle.Top;
            topBar.BackColor = SystemColors.ControlLight;

            btnBack.Text = "←";
            btnBack.Width = 40;
            btnBack.Height = 32;
            btnBack.Left = 8;
            btnBack.Top = 8;
            btnBack.Click += (s, e) => NavigationService.GoBack();
            btnBack.Enabled = false;

            btnHome.Text = "🏠";
            btnHome.Width = 40;
            btnHome.Height = 32;
            btnHome.Left = 56;
            btnHome.Top = 8;
            btnHome.Click += (s, e) => NavigationService.GoHome();

            topBar.Controls.Add(btnBack);
            topBar.Controls.Add(btnHome);

            contentHost.Dock = DockStyle.Fill;

            this.Controls.Add(contentHost);
            this.Controls.Add(topBar);

            // initialize navigation
            NavigationService.Initialize(contentHost, this);
            // apply adaptive layout on resize
            // apply adaptive layout
            TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
            this.Resize += (s, e) => TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
            NavigationService.NavigationChanged += (s, e) =>
            {
                btnBack.Enabled = NavigationService.CanGoBack;
            };

            // show welcome initially
            NavigationService.Navigate(new FormWelcome());
        }
    }
}
