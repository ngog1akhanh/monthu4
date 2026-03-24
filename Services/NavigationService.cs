using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TourGuideSmart.Services
{
    public static class NavigationService
    {
        private static Panel? host;
        private static Form? mainForm;
        private static readonly Stack<Control> stack = new Stack<Control>();

        // animation fields
        private static Bitmap? animOld = null;
        private static Bitmap? animNew = null;
        private static System.Windows.Forms.Timer? animTimer = null;
        private static System.Diagnostics.Stopwatch? animSw = null;
        private static int animDuration = 250;
        private static bool isAnimating = false;

        public static event EventHandler? NavigationChanged;

        public static void Initialize(Panel hostPanel, Form main)
        {
            host = hostPanel;
            mainForm = main;
            stack.Clear();
            OnNavigationChanged();
        }

        // Convenience: Navigate to a form by type, creating instance
        public static void NavigateTo<T>() where T : Form, new()
        {
            var form = new T();
            Navigate(form);
        }

        public static void Navigate(Control control, bool animate = true)
        {
            if (host == null) return;
            if (isAnimating)
            {
                // ignore navigation during animation
                return;
            }

            Control? current = host.Controls.Count > 0 ? host.Controls[0] : null;

            // prepare new control
            control.Dock = DockStyle.Fill;

            if (current != null)
            {
                stack.Push(current);
            }

            if (!animate || current == null)
            {
                host.Controls.Clear();
                host.Controls.Add(control);
                OnNavigationChanged();
                return;
            }

            // perform animated slide: current -> new (new slides from right)
            AnimateTransition(current, control, false, () =>
            {
                host.Controls.Clear();
                host.Controls.Add(control);
                OnNavigationChanged();
            });
        }

        public static void Navigate(Form form, bool animate = true)
        {
            if (host == null) return;
            if (isAnimating) return;

            Control? current = host.Controls.Count > 0 ? host.Controls[0] : null;
            if (current != null) stack.Push(current);

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            if (!animate || current == null)
            {
                host.Controls.Clear();
                host.Controls.Add(form);
                form.Show();
                OnNavigationChanged();
                return;
            }

            AnimateTransition(current, form, false, () =>
            {
                host.Controls.Clear();
                host.Controls.Add(form);
                form.Show();
                OnNavigationChanged();
            });
        }

        public static void GoBack(bool animate = true)
        {
            if (host == null) return;
            if (isAnimating) return;
            if (stack.Count == 0)
            {
                OnNavigationChanged();
                return;
            }

            Control? current = host.Controls.Count > 0 ? host.Controls[0] : null;
            var prev = PopNextValidFromStack();

            // if no valid previous (all were disposed), nothing to do
            if (prev == null)
            {
                OnNavigationChanged();
                return;
            }

            if (!animate || current == null)
            {
                host.Controls.Clear();
                prev.Dock = DockStyle.Fill;
                prev.Visible = true;
                host.Controls.Add(prev);
                OnNavigationChanged();
                return;
            }

            // animate back (prev slides from left)
            AnimateTransition(current, prev, true, () =>
            {
                host.Controls.Clear();
                prev.Dock = DockStyle.Fill;
                prev.Visible = true;
                host.Controls.Add(prev);
                OnNavigationChanged();
            });
        }

        public static void GoHome(bool animate = true)
        {
            if (host == null) return;
            if (isAnimating) return;
            if (stack.Count == 0) return;

            // find bottom-most (initial)
            Control initial = stack.Peek();
            while (stack.Count > 1)
            {
                initial = stack.Pop();
            }
            stack.Clear();

            Control? current = host.Controls.Count > 0 ? host.Controls[0] : null;
            if (!animate || current == null)
            {
                host.Controls.Clear();
                initial.Dock = DockStyle.Fill;
                initial.Visible = true;
                host.Controls.Add(initial);
                OnNavigationChanged();
                return;
            }

            AnimateTransition(current, initial, true, () =>
            {
                host.Controls.Clear();
                initial.Dock = DockStyle.Fill;
                initial.Visible = true;
                host.Controls.Add(initial);
                OnNavigationChanged();
            });
        }

        public static bool CanGoBack => stack.Count > 0;

        // pop until a valid non-disposed control is found or stack empty
        private static Control? PopNextValidFromStack()
        {
            while (stack.Count > 0)
            {
                var c = stack.Pop();
                try
                {
                    // check if control handle is created and not disposed
                    if (c != null && !c.IsDisposed)
                        return c;
                }
                catch
                {
                    // continue popping
                }
            }
            return null;
        }

        private static void AnimateTransition(Control oldControl, Control newControl, bool newFromLeft, Action onComplete)
        {
            if (host == null) return;
            try
            {
                isAnimating = true;
                int w = Math.Max(1, host.ClientSize.Width);
                int h = Math.Max(1, host.ClientSize.Height);

                animOld?.Dispose();
                animNew?.Dispose();
                animOld = RenderControlToBitmap(oldControl, w, h);
                animNew = RenderControlToBitmap(newControl, w, h);

                animSw?.Stop();
                animSw = System.Diagnostics.Stopwatch.StartNew();

                animTimer?.Stop();
                animTimer?.Dispose();
                animTimer = new System.Windows.Forms.Timer { Interval = 16 };

                void paintHandler(object s, PaintEventArgs pe)
                {
                    if (animOld == null || animNew == null) return;
                    var elapsed = animSw?.ElapsedMilliseconds ?? animDuration;
                    double t = Math.Min(1.0, (double)elapsed / animDuration);
                    double ease;
                    if (t < 0.5)
                        ease = 4 * t * t * t;
                    else
                    {
                        double f = (2 * t) - 2;
                        ease = 0.5 * f * f * f + 1;
                    }

                    int oldX, newX;
                    if (newFromLeft)
                    {
                        oldX = (int)Math.Round(0 + (w) * ease);
                        newX = (int)Math.Round(-w + (w) * ease);
                    }
                    else
                    {
                        oldX = (int)Math.Round(0 + (-w) * ease);
                        newX = (int)Math.Round(w + (-w) * ease);
                    }

                    pe.Graphics.Clear(host.BackColor);
                    pe.Graphics.DrawImage(animOld, oldX, 0);
                    pe.Graphics.DrawImage(animNew, newX, 0);
                }

                host.Paint += paintHandler;

                animTimer.Tick += (s, e) =>
                {
                    host.Invalidate();
                    var elapsed = animSw?.ElapsedMilliseconds ?? animDuration;
                    if (elapsed >= animDuration)
                    {
                        animTimer.Stop();
                        host.Paint -= paintHandler;
                        animSw?.Stop();
                        // cleanup
                        animOld?.Dispose();
                        animNew?.Dispose();
                        animOld = null;
                        animNew = null;
                        isAnimating = false;
                        onComplete?.Invoke();
                    }
                };

                animTimer.Start();
            }
            catch
            {
                // fallback: immediate
                onComplete?.Invoke();
                isAnimating = false;
            }
        }

        private static Bitmap? RenderControlToBitmap(Control ctrl, int w, int h)
        {
            try
            {
                var bmp = new Bitmap(w, h);
                // ensure control is laid out
                var vis = ctrl.Visible;
                ctrl.Visible = true;
                ctrl.Refresh();
                Application.DoEvents();
                ctrl.DrawToBitmap(bmp, new Rectangle(0, 0, w, h));
                ctrl.Visible = vis;
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private static void OnNavigationChanged()
        {
            NavigationChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
