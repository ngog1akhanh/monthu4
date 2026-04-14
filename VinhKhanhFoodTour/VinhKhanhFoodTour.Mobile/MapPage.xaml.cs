using VinhKhanh.Shared.Models;
using VinhKhanhFoodTour.Mobile.Services;
using Mapsui.Tiling;
using Mapsui.Projections;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace VinhKhanhFoodTour.Mobile;

public partial class MapPage : ContentPage
{
    private TourEngine _tourEngine;
    private List<POI> _danhSachGoc;

    // Khai báo bộ đếm nhịp để cập nhật GPS liên tục
    private IDispatcherTimer _gpsTimer;

    public MapPage(List<POI> danhSachQuanOc)
    {
        InitializeComponent();
        _danhSachGoc = danhSachQuanOc;
        _tourEngine = new TourEngine(danhSachQuanOc);

        MyMap.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
        MyMap.Map?.Widgets.Clear();
        MyMap.Map?.Layers.Add(CreatePoiLayer(danhSachQuanOc));

        // ==========================================
        // BẬT TÍNH NĂNG CHẤM XANH (BLUE DOT) CỦA MAPSUI
        // ==========================================
        MyMap.MyLocationEnabled = true;

        MyMap.Info += OnMapInfo;
    }

    private void OnMapInfo(object sender, Mapsui.MapInfoEventArgs e)
    {
        if (MyMap.Map == null) return;

        var mapInfo = e.GetMapInfo(MyMap.Map.Layers);
        var feature = mapInfo?.Feature;

        if (feature != null && feature["POI_ID"] != null)
        {
            int id = (int)feature["POI_ID"];
            var poi = _danhSachGoc.FirstOrDefault(p => p.Id == id);

            if (poi != null)
            {
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Navigation.PushAsync(new PoiDetailPage(poi));
                });
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status == PermissionStatus.Granted)
        {
            _tourEngine.StartTour();

            // BẮT ĐẦU VẼ CHẤM XANH KHI ĐÃ CÓ QUYỀN GPS
            StartTrackingBlueDot();
        }

        if (_tourEngine != null)
        {
            double vinhKhanh_Lon = 106.702588;
            double vinhKhanh_Lat = 10.760824;

            var (x, y) = Mapsui.Projections.SphericalMercator.FromLonLat(vinhKhanh_Lon, vinhKhanh_Lat);
            var centerPoint = new Mapsui.MPoint(x, y);

            MyMap.Map?.Navigator.CenterOnAndZoomTo(centerPoint, 2);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tourEngine.StopTour();

        // TẮT QUÉT GPS KHI THOÁT TRANG ĐỂ ĐỠ TỐT PIN
        _gpsTimer?.Stop();
    }

    // ==========================================
    // RADAR QUÉT VÀ VẼ CHẤM XANH LÊN BẢN ĐỒ
    // ==========================================
    private void StartTrackingBlueDot()
    {
        // Tạo một vòng lặp chạy ngầm mỗi 3 giây
        _gpsTimer = Dispatcher.CreateTimer();
        _gpsTimer.Interval = TimeSpan.FromSeconds(3);
        _gpsTimer.Tick += async (s, e) =>
        {
            try
            {
                // Gọi API của điện thoại để xin tọa độ hiện tại
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(2));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    // Đưa tọa độ vào Mapsui để nó tự cập nhật vị trí Chấm Xanh
                    MyMap.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                }
            }
            catch
            {
                // Mất sóng GPS hoặc vào tầng hầm thì bỏ qua, đợi 3 giây sau quét lại
            }
        };
        _gpsTimer.Start();
    }

    private ILayer CreatePoiLayer(List<POI> pois)
    {
        var features = new List<PointFeature>();

        foreach (var poi in pois)
        {
            var (px, py) = Mapsui.Projections.SphericalMercator.FromLonLat(poi.Longitude, poi.Latitude);
            var mPoint = new Mapsui.MPoint(px, py);

            var feature = new PointFeature(mPoint)
            {
                ["Name"] = poi.Name,
                ["POI_ID"] = poi.Id
            };
            features.Add(feature);
        }

        var pinStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Triangle,
            SymbolRotation = 180,
            SymbolScale = 0.8,
            Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Orange),
            Outline = new Pen(Mapsui.Styles.Color.White, 2)
        };

        return new MemoryLayer
        {
            Name = "DanhSachQuanOc",
            Features = features,
            Style = pinStyle
        };
    }
}