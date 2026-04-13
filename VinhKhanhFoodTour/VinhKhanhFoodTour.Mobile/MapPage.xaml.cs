using Mapsui.UI.Maui;
using Mapsui.Tiling;
using Mapsui;
using VinhKhanh.Shared.Models;
using System.Linq;

namespace VinhKhanhFoodTour.Mobile;

public partial class MapPage : ContentPage
{
    public MapPage(List<POI> pois)
    {
        InitializeComponent();

        // Tải bản đồ đường phố OpenStreetMap
        MyMap.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        if (pois != null && pois.Any())
        {
            DisplayPoisOnMap(pois);
        }
    }

    private void DisplayPoisOnMap(List<POI> pois)
    {
        foreach (var poi in pois)
        {
            var pin = new Pin(MyMap)
            {
                Position = new Position(poi.Latitude, poi.Longitude),
                Label = poi.Name,
                Address = poi.Description_VI,
                Type = PinType.Pin,
                Color = Microsoft.Maui.Graphics.Colors.Red
            };

            // Mình đã xóa đoạn CalloutClicked đi để bản đồ tự động xử lý bong bóng thông tin mượt mà hơn

            MyMap.Pins.Add(pin);
        }

        var firstPoi = pois.First();

        // ĐÃ SỬA LỖI CS1503 Ở ĐÂY: Chuyển đổi cặp số (x, y) sang định dạng MPoint chuẩn
        var (x, y) = Mapsui.Projections.SphericalMercator.FromLonLat(firstPoi.Longitude, firstPoi.Latitude);
        var centerPoint = new MPoint(x, y);

        // Di chuyển camera đến MPoint đó
        MyMap.Map?.Navigator?.CenterOnAndZoomTo(centerPoint, MyMap.Map.Navigator.Resolutions[16]);
    }
}