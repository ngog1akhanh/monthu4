using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace TourGuideApp.Pages;

public partial class MapsPage : ContentPage
{
    public MapsPage(string url)
    {
        InitializeComponent();

        MapWebView.Source = url;
    }

    async void OnOpenInBrowserClicked(object sender, EventArgs e)
    {
        if (MapWebView?.Source is UrlWebViewSource u)
        {
            await Launcher.OpenAsync(u.Url);
        }
        else
        {
            var src = MapWebView?.Source as WebViewSource;
            if (src != null)
            {
                await Launcher.OpenAsync(src.ToString());
            }
        }
    }
}
