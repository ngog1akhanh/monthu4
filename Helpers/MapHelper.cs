namespace TourGuideSmart.Helpers
{
    public static class MapHelper
    {
        public static (double lat, double lon) ParseLocation(string input)
        {
            var parts = input?.Split(',');
            if (parts == null || parts.Length != 2) return (0, 0);
            double.TryParse(parts[0], out var lat);
            double.TryParse(parts[1], out var lon);
            return (lat, lon);
        }
    }
}
