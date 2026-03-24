using System.Diagnostics;

namespace TourGuideSmart.Services
{
    public class MapService
    {
        public void OpenMap(string location)
        {
            var encoded = Uri.EscapeDataString(location);
            var url = "https://www.google.com/maps/search/" + encoded;
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
