using SQLite;

namespace TourGuideApp.Models;

public class RestaurantTranslations
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string? LanguageCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
