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
    private List<POI> _danhSachGoc; // LƯU LẠI DANH SÁCH ĐỂ DÙNG KHI NHẤN VÀO GHIM

    public MapPage(List<POI> danhSachQuanOc)
    {
        InitializeComponent();
        _danhSachGoc = danhSachQuanOc; // Gán dữ liệu vào biến toàn cục
        _tourEngine = new TourEngine(danhSachQuanOc);

        // 1. Tải lớp nền đường sá thực tế
        MyMap.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        // 2. Xóa các dòng chữ INFO và bảng FPS
        MyMap.Map?.Widgets.Clear();

        // 3. Đặt lớp ghim lên bản đồ
        MyMap.Map?.Layers.Add(CreatePoiLayer(danhSachQuanOc));

        // ==========================================
        // ĐĂNG KÝ SỰ KIỆN: LẮNG NGHE KHI BẤM VÀO BẢN ĐỒ
        // ==========================================
        MyMap.Info += OnMapInfo;
    }

    // ==========================================
    // HÀM XỬ LÝ KHI NGƯỜI DÙNG BẤM VÀO 1 CÁI GHIM
    // ==========================================
    // ==========================================
    // HÀM XỬ LÝ KHI NGƯỜI DÙNG BẤM VÀO 1 CÁI GHIM
    // ==========================================
    private void OnMapInfo(object sender, Mapsui.MapInfoEventArgs e)
    {
        // Phòng hờ nếu bản đồ chưa kịp load
        if (MyMap.Map == null) return;

        // ĐÃ SỬA Ở ĐÂY: Dùng hàm GetMapInfo() để tự lấy thông tin ghim ở vị trí vừa chạm
        var mapInfo = e.GetMapInfo(MyMap.Map.Layers);

        // Lấy cái ghim ra
        var feature = mapInfo?.Feature;

        // Kiểm tra xem ghim đó có mang thẻ "POI_ID" không
        if (feature != null && feature["POI_ID"] != null)
        {
            int id = (int)feature["POI_ID"];

            // Lục tìm trong danh sách gốc xem ID này là của quán ốc nào
            var poi = _danhSachGoc.FirstOrDefault(p => p.Id == id);

            if (poi != null)
            {
                // Chuyển sang trang chi tiết của quán đó
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
                // GẮN THẺ ID VÀO GHIM ĐỂ LÚC BẤM CÒN BIẾT ĐƯỜNG MỞ TRANG CHI TIẾT
                ["POI_ID"] = poi.Id
            };
            features.Add(feature);
        }

        // ==========================================
        // ĐỔI HÌNH DÁNG GHIM THÀNH GIỌT NƯỚC
        // ==========================================
        var pinStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Triangle, // Dùng hình tam giác
            SymbolRotation = 180, // Lật ngược tam giác lại thành cái mũi nhọn chỉ xuống đất
            SymbolScale = 0.8, // Kích thước vừa phải
            Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Orange), // Chuyển sang màu Cam cho giống Google Maps
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