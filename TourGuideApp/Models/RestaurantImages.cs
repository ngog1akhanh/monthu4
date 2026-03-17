using SQLite;

namespace TourGuideApp.Models;

public class RestaurantImages
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string? ImageUrl { get; set; }
}
