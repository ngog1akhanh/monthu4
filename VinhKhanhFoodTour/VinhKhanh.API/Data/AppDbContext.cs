using Microsoft.EntityFrameworkCore;
using VinhKhanh.Shared.Models;

namespace VinhKhanh.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Khai báo bảng POIs
        public DbSet<POI> POIs { get; set; }
    }
}