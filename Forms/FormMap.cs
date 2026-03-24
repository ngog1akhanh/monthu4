using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace TourGuideSmart
{
    public class FormMap : Form
    {
        private Button btnClose = new Button();
        private Button btnOpenExternal = new Button();

        public FormMap(string url)
        {
            this.Text = "Bản đồ";
            this.Width = 900;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;

            btnClose.Text = "Đóng";
            btnClose.Height = 32;
            btnClose.Width = 80;
            btnClose.Top = 8;
            btnClose.Left = 8;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnClose.Click += (s, e) =>
            {
                // if hosted in NavigationService, go back; otherwise close
                foreach (Form open in Application.OpenForms)
                {
                    if (open is FormMain)
                    {
                        TourGuideSmart.Services.NavigationService.GoBack();
                        return;
                    }
                }
                this.Close();
            };

            // Button to open the map in the system default browser (shown when embedding isn't available)
            btnOpenExternal.Text = "Mở trên trình duyệt";
            btnOpenExternal.Height = 32;
            btnOpenExternal.Width = 140;
            btnOpenExternal.Top = 8;
            btnOpenExternal.Left = Math.Max(8, this.ClientSize.Width - btnOpenExternal.Width - 8);
            btnOpenExternal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenExternal.Visible = false;
            btnOpenExternal.Click += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    try { MessageBox.Show("Không thể mở trình duyệt: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
                }
            };

            // Add buttons to the form now so they are always available
            this.Controls.Add(btnClose);
            this.Controls.Add(btnOpenExternal);

            // Prefer embedding with WebView2 (requires Microsoft.Web.WebView2 package + runtime).
            try
            {
                var webView = new WebView2 { Dock = DockStyle.Fill };
                // ensure external-open button is on top
                btnOpenExternal.Visible = false;
                this.Controls.Add(webView);

                webView.CoreWebView2InitializationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                    {
                        try { webView.CoreWebView2.Settings.IsScriptEnabled = true; } catch { }
                        try { webView.CoreWebView2.Navigate(url); } catch { }
                    }
                    else
                    {
                        try
                        {
                            var msg = "WebView2 initialization failed. You can open the map in your browser with the 'Mở trên trình duyệt' button. Consider installing the Microsoft Edge WebView2 Runtime.";
                            MessageBox.Show(msg, "WebView2 init failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        catch { }

                        // show button so user can open external browser manually
                        try { btnOpenExternal.Visible = true; btnOpenExternal.BringToFront(); } catch { }
                    }
                };

                try
                {
                    webView.EnsureCoreWebView2Async();
                }
                catch (Exception ex)
                {
                    try { MessageBox.Show("WebView2 failed to start: " + ex.Message + "\nYou can open the map in your browser using the 'Mở trên trình duyệt' button.", "WebView2 error", MessageBoxButtons.OK, MessageBoxIcon.Warning); } catch { }
                    try { btnOpenExternal.Visible = true; btnOpenExternal.BringToFront(); } catch { }
                }

                TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
                return;
            }
            catch
            {
                // If embedding fails, try opening in the system browser.
            }

            // If embedding failed above, show the external button and attempt an in-app WebBrowser fallback.
            try
            {
                btnOpenExternal.Visible = true;
                btnOpenExternal.BringToFront();
            }
            catch { }

            // Finally fallback to old WebBrowser control if embedding and external open are not used.
            var browser = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = true };
            this.Controls.Add(browser);
            try
            {
                browser.Navigate(url);
            }
            catch
            {
                browser.DocumentText = "<html><body><h3>Không thể mở bản đồ</h3></body></html>";
            }

            TourGuideSmart.Helpers.AdaptiveLayout.Apply(this);
        }
    }
}
