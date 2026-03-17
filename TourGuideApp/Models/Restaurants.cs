using SQLite;

namespace TourGuideApp.Models;

public class Restaurants
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? AudioFile { get; set; }
}
