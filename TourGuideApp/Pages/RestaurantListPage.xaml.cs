using System.Diagnostics;
using TourGuideApp.Database;
using TourGuideApp.Models;
namespace TourGuideApp.Pages;

public partial class RestaurantListPage : ContentPage
{
    DatabaseService db;

    public RestaurantListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (db == null)
        {
            db = new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "tourguide.db"));
        }

        var restaurants = await db.GetRestaurants("vi");
        RestaurantList.ItemsSource = restaurants;
    }

    async void OnRestaurantClicked(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        int id = (int)btn.CommandParameter;

        await Navigation.PushAsync(new RestaurantDetailPage(id));
    }
}