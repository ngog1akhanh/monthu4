using SQLite;

namespace TourGuideApp.Models;

public class Menus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int RestaurantId { get; set; }

    public string? NameVi { get; set; }
    public string? NameEn { get; set; }

    public string? DescriptionVi { get; set; }
    public string? DescriptionEn { get; set; }

    public double Price { get; set; }
}
