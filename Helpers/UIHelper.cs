namespace TourGuideSmart.Helpers
{
    public static class UIHelper
    {
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
