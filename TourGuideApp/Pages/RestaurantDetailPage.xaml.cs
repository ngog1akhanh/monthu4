using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using TourGuideApp.Database;
using Microsoft.Maui.ApplicationModel;

namespace TourGuideApp.Pages;

public partial class RestaurantDetailPage : ContentPage
{
    DatabaseService db;
    int restaurantId;

    string? speakText;

    public RestaurantDetailPage(int id)
    {
        InitializeComponent();

        restaurantId = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (db == null)
            {
                db = new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "tourguide.db"));
            }

            var restaurant = await db.GetRestaurant(restaurantId);
            var translation = await db.GetRestaurantTranslation(restaurantId);

            if (translation != null)
            {
                NameLabel.Text = translation.Name;
                DescriptionLabel.Text = translation.Description;
                speakText = translation.Description ?? translation.Name;
            }
            else
            {
                // No translation available — use name if available or leave speakText null
                NameLabel.Text = restaurant?.AudioFile ?? string.Empty;
                DescriptionLabel.Text = string.Empty;
                speakText = restaurant?.AudioFile;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RestaurantDetailPage.OnAppearing error: {ex}");
            await DisplayAlert("Error", "Không thể tải chi tiết. " + ex.Message, "OK");
        }
    }

    async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(speakText))
            return;

        try
        {
            await TextToSpeech.SpeakAsync(speakText);
        }
        catch (Exception)
        {
            // optionally show a message to user
        }
    }

    async void OnNavigateClicked(object sender, EventArgs e)
    {
        // get restaurant coordinates
        var restaurant = await db.GetRestaurant(restaurantId);
        if (restaurant == null)
        {
            await DisplayAlert("Error", "Restaurant coordinates not available.", "OK");
            return;
        }

        double lat = restaurant.Latitude;
        double lng = restaurant.Longitude;

        if (lat == 0 && lng == 0)
        {
            await DisplayAlert("Error", "Invalid coordinates.", "OK");
            return;
        }

        // Try to open Google Maps app with directions. If not installed, open web link.
        string gmapsUri = $"google.navigation:q={lat},{lng}"; // android navigation
        string mapsWeb = $"https://www.google.com/maps/dir/?api=1&destination={lat},{lng}";

        // Open MapsPage with Google Maps directions URL inside WebView
        var mapsUrl = mapsWeb; // use web directions URL
        await Navigation.PushAsync(new MapsPage(mapsUrl));
    }
}
