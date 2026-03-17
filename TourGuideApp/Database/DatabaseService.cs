using SQLite;
using TourGuideApp.Models;

namespace TourGuideApp.Database;

public class DatabaseService
{
    SQLiteAsyncConnection db;

    public DatabaseService(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task<RestaurantTranslations?> GetRestaurantTranslation(int restaurantId)
    {
        return await db.Table<RestaurantTranslations>()
                       .Where(t => t.RestaurantId == restaurantId)
                       .FirstOrDefaultAsync();
    }

    public async Task<List<RestaurantTranslations>> GetRestaurants(string lang)
    {
        return await db.Table<RestaurantTranslations>()
            .Where(r => r.LanguageCode == lang)
            .ToListAsync();
    }

    public async Task<Restaurants> GetRestaurant(int id)
    {
        return await db.Table<Restaurants>()
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Menus>> GetMenus(int restaurantId)
    {
        return await db.Table<Menus>()
            .Where(m => m.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<List<RestaurantImages>> GetImages(int restaurantId)
    {
        return await db.Table<RestaurantImages>()
            .Where(i => i.RestaurantId == restaurantId)
            .ToListAsync();
    }
}