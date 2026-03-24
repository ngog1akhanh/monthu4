namespace TourGuideSmart.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string Category { get; set; }
        // optional image path for mock data and UI
        public string ImagePath { get; set; }
    }
}