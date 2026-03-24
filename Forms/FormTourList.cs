using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TourGuideSmart.Services;
using TourGuideSmart.Models;
using TourGuideSmart.Helpers;

namespace TourGuideSmart
{
    public class FormTourList : Form
    {
        private FlowLayoutPanel flow = new FlowLayoutPanel();

        public FormTourList()
        {
            this.Text = "Danh sách quán";
            this.Width = 700;
            this.Height = 500;

            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.TopDown;
            flow.WrapContents = false;
            flow.AutoScroll = true;
            flow.Padding = new Padding(10);
            this.Controls.Add(flow);

            var tours = new TourService().GetTours();
            BuildList(tours);

            this.Resize += (s, e) => LayoutItems();
            this.Shown += (s, e) => AdaptiveLayout.Apply(this);
        }

        private void BuildList(System.Collections.Generic.IEnumerable<Tour> tours)
        {
            flow.Controls.Clear();
            foreach (var t in tours)
            {
                var panel = CreateItemPanel(t);
                flow.Controls.Add(panel);
            }
            LayoutItems();
        }

        private Panel CreateItemPanel(Tour t)
        {
            var p = new Panel();
            p.Height = 100;
            p.Width = Math.Max(300, this.ClientSize.Width - 40);
            p.BackColor = Color.WhiteSmoke;
            p.Padding = new Padding(8);
            p.Margin = new Padding(6);
            p.BorderStyle = BorderStyle.FixedSingle;

            var pic = new PictureBox();
            pic.Width = 84;
            pic.Height = 84;
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            pic.Left = 8;
            pic.Top = 8;
            try
            {
                if (!string.IsNullOrEmpty(t.ImagePath) && File.Exists(t.ImagePath))
                {
                    using var img = Image.FromFile(t.ImagePath);
                    pic.Image = new Bitmap(img);
                }
                else
                {
                    pic.Image = CreatePlaceholderImage(pic.Width, pic.Height);
                }
            }
            catch
            {
                pic.Image = CreatePlaceholderImage(pic.Width, pic.Height);
            }

            var lblName = new Label();
            lblName.Text = t.Name;
            lblName.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblName.AutoSize = false;
            lblName.Left = pic.Right + 12;
            lblName.Top = 12;
            lblName.Width = p.Width - lblName.Left - 16;
            lblName.Height = 24;

            var lblDesc = new Label();
            lblDesc.Text = t.Description;
            lblDesc.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblDesc.AutoEllipsis = true;
            lblDesc.Left = pic.Right + 12;
            lblDesc.Top = lblName.Bottom + 4;
            lblDesc.Width = p.Width - lblDesc.Left - 16;
            lblDesc.Height = 48;

            // click handler to open detail via NavigationService when possible
            void OpenDetail(object s, EventArgs e)
            {
                var detail = new FormDetail(t);
                foreach (Form open in Application.OpenForms)
                {
                    if (open is FormMain)
                    {
                        NavigationService.Navigate(detail);
                        return;
                    }
                }
                detail.Show();
            }

            p.Controls.Add(pic);
            p.Controls.Add(lblName);
            p.Controls.Add(lblDesc);

            // attach click to panel and children
            p.Click += OpenDetail;
            foreach (Control c in p.Controls) c.Click += OpenDetail;

            return p;
        }

        private void LayoutItems()
        {
            foreach (Control c in flow.Controls)
            {
                if (c is Panel p)
                {
                    p.Width = Math.Max(300, this.ClientSize.Width - 40);
                    // adjust child widths
                    foreach (Control ch in p.Controls)
                    {
                        if (ch is Label lbl)
                        {
                            lbl.Width = p.Width - lbl.Left - 16;
                        }
                    }
                }
            }
        }

        private Image CreatePlaceholderImage(int w, int h)
        {
            var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.LightGray);
            using var f = new Font("Segoe UI", 10, FontStyle.Regular);
            var txt = "No Image";
            var sz = g.MeasureString(txt, f);
            g.DrawString(txt, f, Brushes.DarkGray, (w - sz.Width) / 2, (h - sz.Height) / 2);
            return bmp;
        }
    }
}
