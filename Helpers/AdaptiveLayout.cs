using System;
using System.Drawing;
using System.Windows.Forms;

namespace TourGuideSmart.Helpers
{
    public static class AdaptiveLayout
    {
        // Mobile breakpoint in pixels (width)
        public const int MobileWidthBreakpoint = 600;

        public static void Apply(Form form)
        {
            if (form == null) return;
            bool isMobile = form.ClientSize.Width <= MobileWidthBreakpoint;
            // Apply base theme to form
            form.Font = new Font("Segoe UI", isMobile ? 10 : 9);
            form.BackColor = System.Drawing.Color.WhiteSmoke;
            form.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            ApplyToControl(form, isMobile);
            form.Padding = isMobile ? new Padding(14) : new Padding(12);
        }

        private static void ApplyToControl(Control ctrl, bool isMobile)
        {
            if (ctrl == null) return;

            // base scaling factor
            float scale = isMobile ? 1.25f : 1.0f;

            // adjust fonts and sizes for common controls
            if (ctrl is Button btn)
            {
                btn.Font = new Font("Segoe UI", Math.Max(10, btn.Font.Size * scale), btn.Font.Style);
                btn.Height = isMobile ? Math.Max(44, btn.Height) : Math.Max(32, btn.Height);
                btn.Width = Math.Max(btn.Width, isMobile ? 140 : Math.Max(80, btn.Width));
                btn.Padding = new Padding(8, 6, 8, 6);
                // modern flat style
                try
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
                    btn.ForeColor = System.Drawing.Color.White;
                }
                catch { }
            }
            else if (ctrl is Label lbl)
            {
                lbl.Font = new Font("Segoe UI", Math.Max(10, lbl.Font.Size * scale), lbl.Font.Style);
                lbl.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            }
            else if (ctrl is NumericUpDown nud)
            {
                nud.Font = new Font(nud.Font.FontFamily, Math.Max(10, nud.Font.Size * scale), nud.Font.Style);
                nud.Height = isMobile ? Math.Max(36, nud.Height) : nud.Height;
            }
            else if (ctrl is ListView lv)
            {
                lv.Font = new Font(lv.Font.FontFamily, Math.Max(10, lv.Font.Size * scale), lv.Font.Style);
                lv.HideSelection = false;
                lv.BackColor = System.Drawing.Color.White;
                lv.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
                try { lv.GridLines = true; } catch { }
            }
            else if (ctrl is TextBox tb)
            {
                tb.Font = new Font("Segoe UI", Math.Max(10, tb.Font.Size * scale), tb.Font.Style);
                tb.BackColor = System.Drawing.Color.White;
                try { tb.BorderStyle = BorderStyle.FixedSingle; } catch { }
            }
            else if (ctrl is ComboBox cb)
            {
                cb.Font = new Font("Segoe UI", Math.Max(10, cb.Font.Size * scale), cb.Font.Style);
                cb.BackColor = System.Drawing.Color.White;
                cb.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            }
            else if (ctrl is PictureBox pb)
            {
                // subtle background for empty images
                if (pb.Image == null) pb.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            }

            // recurse for children
            foreach (Control c in ctrl.Controls)
            {
                ApplyToControl(c, isMobile);
            }
        }
    }
}
