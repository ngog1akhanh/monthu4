using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using TourGuideSmart.Services;
using TourGuideSmart.Models;
// no VisualStyles import to avoid ContentAlignment ambiguity

namespace TourGuideSmart
{
    public class FormPlanner : Form
    {
        private Label lblTitle = new Label();
        private NumericUpDown nudBudget = new NumericUpDown();
        private Button btnSuggest = new Button();
        private Button btnBack = new Button();
        private ListView lvResults = new ListView();
        private Label lblEmpty = new Label();
        private DoubleBufferedPanel contentPanel = new DoubleBufferedPanel();
        private bool isAnimating = false;
        // bitmap-based animation fields
        private Bitmap? animBmpOld = null;
        private Bitmap? animBmpNew = null;
        private System.Windows.Forms.Timer? animTimer = null;
        private System.Diagnostics.Stopwatch? animStopwatch = null;
        private int animDuration = 250;

        private Panel topPanel;

        public FormPlanner()
        {
            this.Text = "Gợi ý Tour";
            this.Width = 600;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterParent;

            // Title
            lblTitle.Text = "Gợi ý Tour theo ngân sách";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 50;

            // Numeric budget
            nudBudget.Minimum = 0;
            nudBudget.Maximum = 10000000;
            nudBudget.Value = 100000;
            nudBudget.ThousandsSeparator = true;
            nudBudget.Width = 200;
            nudBudget.Increment = 1000;
            nudBudget.TextAlign = HorizontalAlignment.Right;
            nudBudget.Font = new Font("Segoe UI", 10);

            // Suggest button
            btnSuggest.Text = "Gợi ý";
            btnSuggest.Width = 120;
            btnSuggest.Height = 36;
            btnSuggest.BackColor = Color.FromArgb(33, 150, 243);
            btnSuggest.ForeColor = Color.White;
            btnSuggest.FlatStyle = FlatStyle.Flat;
            btnSuggest.FlatAppearance.BorderSize = 0;
            btnSuggest.Click += BtnSuggest_Click;

            // Back button
            btnBack.Text = "Quay lại";
            btnBack.Width = 120;
            btnBack.Height = 36;
            btnBack.BackColor = SystemColors.ControlDark;
            btnBack.ForeColor = Color.White;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Margin = new Padding(8, 6, 0, 0);
            btnBack.Click += (s, e) =>
            {
                // navigate back if using NavigationService
                foreach (Form open in Application.OpenForms)
                {
                    if (open is FormMain)
                    {
                        TourGuideSmart.Services.NavigationService.GoBack();
                        return;
                    }
                }

                // fallback: hide
                this.Hide();
                this.Owner?.BringToFront();
            };

            // Results ListView
            lvResults.View = View.Details;
            lvResults.FullRowSelect = true;
            lvResults.GridLines = true;
            lvResults.Columns.Add("Tên", 320);
            lvResults.Columns.Add("Giá (đ)", 120, HorizontalAlignment.Right);
            // don't dock yet — we'll place these inside a content panel so they can be animated
            // lvResults.Dock = DockStyle.Fill;
            lvResults.Font = new Font("Segoe UI", 10);

            // empty state label
            lblEmpty.Text = "Chưa có gợi ý. Nhập ngân sách và bấm Gợi ý";
            lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
            // don't dock yet, put into contentPanel
            // lblEmpty.Dock = DockStyle.Fill;
            lblEmpty.Visible = true; // show empty state initially
            lblEmpty.Font = new Font("Segoe UI", 10, FontStyle.Italic);

            // Layout using a top panel with a FlowLayoutPanel for inputs
            topPanel = new Panel { Height = 120, Dock = DockStyle.Top };
            var inputFlow = new FlowLayoutPanel { Padding = new Padding(10), AutoSize = true };
            inputFlow.FlowDirection = FlowDirection.LeftToRight;
            inputFlow.WrapContents = false;

            var lblBudget = new Label { Text = "Ngân sách:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 8, 8, 0) };
            nudBudget.Margin = new Padding(0, 8, 8, 0);
            btnSuggest.Margin = new Padding(8, 6, 0, 0);

            inputFlow.Controls.Add(lblBudget);
            inputFlow.Controls.Add(nudBudget);
            inputFlow.Controls.Add(btnSuggest);
            inputFlow.Controls.Add(btnBack);
            topPanel.Controls.Add(inputFlow);
            // center inputFlow within topPanel when resized
            topPanel.Resize += (s, e) => CenterInputFlow(topPanel, inputFlow);
            // also center initially
            CenterInputFlow(topPanel, inputFlow);

            // content panel will host the main area where we show results or empty state
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(0);
            contentPanel.BackColor = this.BackColor;
            // add children to content panel and dock them so they fill and are centered
            lvResults.Dock = DockStyle.Fill;
            lblEmpty.Dock = DockStyle.Fill;
            lvResults.Visible = false; // start with empty shown
            contentPanel.Controls.Add(lvResults);
            contentPanel.Controls.Add(lblEmpty);

            // Add controls in z-order so content is at back and inputs sit on top
            this.Controls.Add(contentPanel);
            this.Controls.Add(lblTitle);
            this.Controls.Add(topPanel);
            // ensure top controls (title and input panel) stay above the content panel
            contentPanel.SendToBack();
            lblTitle.BringToFront();
            topPanel.BringToFront();
            // make sure interactive controls are in front
            btnSuggest.BringToFront();
            nudBudget.BringToFront();
            this.Padding = new Padding(10);

            // handle resize to adjust columns and keep input centered
            this.Resize += (s, e) =>
            {
                CenterInputFlow(topPanel, inputFlow);
                AdjustColumns();
                TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
            };
            contentPanel.Resize += (s, e) => AdjustColumns();
            // initial column adjust
            AdjustColumns();
            TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
        }

        private void CenterInputFlow(Panel topPanel, FlowLayoutPanel inputFlow)
        {
            if (topPanel == null || inputFlow == null) return;
            // ensure layout updated
            inputFlow.PerformLayout();
            inputFlow.Left = Math.Max((topPanel.ClientSize.Width - inputFlow.Width) / 2, 0);
            inputFlow.Top = Math.Max((topPanel.ClientSize.Height - inputFlow.Height) / 2, 0);
        }

        private void BtnSuggest_Click(object sender, EventArgs e)
        {
            int budget = (int)nudBudget.Value;
            var tours = new TourService().GetTours();
            var result = new PlannerService().SuggestTour(tours, budget);

            lvResults.Items.Clear();
            if (result == null || result.Count == 0)
            {
                // switch to empty label with animation
                SlideTransition(contentPanel, lvResults, lblEmpty, 250);
                lvResults.Visible = false;
                lblEmpty.Visible = true;
            }
            else
            {
                foreach (var t in result)
                {
                    var item = new ListViewItem(new[] { t.Name, t.Price.ToString("N0") });
                    lvResults.Items.Add(item);
                }
                // switch to results with animation
                SlideTransition(contentPanel, lblEmpty, lvResults, 250);
                lvResults.Visible = true;
                lblEmpty.Visible = false;
            }

            AdjustColumns();
        }

        /// <summary>
        /// Animate a horizontal slide transition inside the given parent container.
        /// oldControl slides left out, newControl slides in from right.
        /// Duration in milliseconds (recommended 200-300ms).
        /// </summary>
        private void SlideTransition(Control parent, Control oldControl, Control newControl, int durationMs = 250)
        {
            if (parent == null || oldControl == null || newControl == null) return;
            if (oldControl == newControl) return;
            if (isAnimating) return; // avoid overlapping animations

            isAnimating = true;

            int interval = 16; // ms per tick (~60 FPS) - matches typical timer resolution
            int steps = Math.Max(1, durationMs / interval);
            int step = 0;

            // ensure both controls are children of the parent
            if (oldControl.Parent != parent) parent.Controls.Add(oldControl);
            if (newControl.Parent != parent) parent.Controls.Add(newControl);

            // prepare bounds and stop docking so we can manipulate positions
            oldControl.Dock = DockStyle.None;
            newControl.Dock = DockStyle.None;
            oldControl.Visible = true;
            newControl.Visible = true;

            int w = Math.Max(1, parent.ClientSize.Width);
            int h = Math.Max(1, parent.ClientSize.Height);

            // capture bitmaps of old and new controls
            animBmpOld?.Dispose();
            animBmpNew?.Dispose();
            animBmpOld = new Bitmap(w, h);
            animBmpNew = new Bitmap(w, h);

            try
            {
                // ensure controls are laid out and visible for rendering
                var oldVis = oldControl.Visible;
                var newVis = newControl.Visible;
                var oldBounds = oldControl.Bounds;
                var newBounds = newControl.Bounds;
                try
                {
                    oldControl.Visible = true;
                    newControl.Visible = true;
                    oldControl.SetBounds(0, 0, w, h);
                    newControl.SetBounds(0, 0, w, h);
                    oldControl.Refresh();
                    newControl.Refresh();
                    Application.DoEvents();

                    // render old control
                    using (var g = Graphics.FromImage(animBmpOld))
                    {
                        var r = new Rectangle(0, 0, w, h);
                        g.Clear(parent.BackColor);
                        var bmpOldControl = RenderControlToBitmap(oldControl, w, h);
                        if (bmpOldControl != null) g.DrawImage(bmpOldControl, r);
                        bmpOldControl?.Dispose();
                    }

                    // render new control
                    using (var g = Graphics.FromImage(animBmpNew))
                    {
                        var r = new Rectangle(0, 0, w, h);
                        g.Clear(parent.BackColor);
                        var bmpNewControl = RenderControlToBitmap(newControl, w, h);
                        if (bmpNewControl != null) g.DrawImage(bmpNewControl, r);
                        bmpNewControl?.Dispose();
                    }
                }
                finally
                {
                    // restore visibility and bounds
                    oldControl.SetBounds(oldBounds.X, oldBounds.Y, oldBounds.Width, oldBounds.Height);
                    newControl.SetBounds(newBounds.X, newBounds.Y, newBounds.Width, newBounds.Height);
                    oldControl.Visible = oldVis;
                    newControl.Visible = newVis;
                }
            }
            catch
            {
                // ignore rendering errors
            }

            // hide actual controls during animation and stop docking
            oldControl.Visible = false;
            newControl.Visible = false;
            oldControl.Dock = DockStyle.Fill;
            newControl.Dock = DockStyle.Fill;

            animDuration = durationMs;
            animStopwatch?.Stop();
            animStopwatch = System.Diagnostics.Stopwatch.StartNew();

            // ensure we have a single timer
            animTimer?.Stop();
            animTimer?.Dispose();
            animTimer = new System.Windows.Forms.Timer { Interval = interval };

            // paint handler draws the two bitmaps offset by easing
            void paintHandler(object s, PaintEventArgs pe)
            {
                if (animBmpOld == null || animBmpNew == null) return;
                var elapsed = animStopwatch?.ElapsedMilliseconds ?? animDuration;
                double t = Math.Min(1.0, (double)elapsed / animDuration);
                double ease;
                if (t < 0.5)
                    ease = 4 * t * t * t;
                else
                {
                    double f = (2 * t) - 2;
                    ease = 0.5 * f * f * f + 1;
                }

                int oldX = (int)Math.Round(0 + (-w) * ease);
                int newX = (int)Math.Round(w + (-w) * ease);

                pe.Graphics.Clear(parent.BackColor);
                pe.Graphics.DrawImage(animBmpOld, oldX, 0);
                pe.Graphics.DrawImage(animBmpNew, newX, 0);

                // no additional painting for top area; layout ordering uses BringToFront/SendToBack
            }

            contentPanel.Paint += paintHandler;

            animTimer.Tick += (s, e) =>
            {
                contentPanel.Invalidate();
                var elapsed = animStopwatch?.ElapsedMilliseconds ?? animDuration;
                if (elapsed >= animDuration)
                {
                    // end animation
                    animTimer.Stop();
                    contentPanel.Paint -= paintHandler;
                    animStopwatch?.Stop();
                    // show the new control
                    newControl.Visible = true;
                    newControl.Dock = DockStyle.Fill;
                    // hide/restore old
                    oldControl.Visible = false;
                    oldControl.Dock = DockStyle.Fill;
                    isAnimating = false;
                    contentPanel.Invalidate();
                    AdjustColumns();
                    // dispose bitmaps
                    animBmpOld?.Dispose();
                    animBmpNew?.Dispose();
                    animBmpOld = null;
                    animBmpNew = null;
                }
            };

            animTimer.Start();
        }

        private Bitmap? RenderControlToBitmap(Control ctrl, int w, int h)
        {
            if (ctrl == null) return null;
            try
            {
                var bmp = new Bitmap(Math.Max(1, w), Math.Max(1, h));
                ctrl.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private void AdjustColumns()
        {
            if (lvResults.Columns.Count < 2) return;
            // make second column a fixed width and first column fill remaining space
            int second = 140;
            int available = Math.Max(0, lvResults.ClientSize.Width - second - SystemInformation.VerticalScrollBarWidth);
            lvResults.Columns[0].Width = Math.Max(100, available);
            lvResults.Columns[1].Width = second;
        }

        // simple double-buffered Panel to reduce flicker during animation
        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            }
        }
    }
}
