using VinhKhanhFoodTour.Mobile.Services;
using VinhKhanh.Shared.Models; // Khai báo thư viện để dùng được chữ POI

namespace VinhKhanhFoodTour.Mobile;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    // TẠO MỘT CÁI KHO LƯU TẠM DỮ LIỆU
    private List<POI> _danhSachQuanOc;

    public MainPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // CHỈ GỌI API NẾU DANH SÁCH ĐANG TRỐNG
        // Nếu đã có dữ liệu rồi thì giữ nguyên, không tải lại nữa để chống giật lag
        if (_danhSachQuanOc == null || _danhSachQuanOc.Count == 0)
        {
            await LoadData();
        }
    }

    private async Task LoadData()
    {
        // Gọi API 1 lần duy nhất và cất vào kho
        _danhSachQuanOc = await _apiService.GetPOIsAsync();

        // Hiện lên màn hình
        PoisList.ItemsSource = _danhSachQuanOc;
    }

    private async void OnViewMapClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn) btn.IsEnabled = false;

            // KHÔNG GỌI API NỮA! Lấy trực tiếp từ kho ra xài
            if (_danhSachQuanOc != null && _danhSachQuanOc.Count > 0)
            {
                // Chuyển sang trang bản đồ, mang theo danh sách quán ốc
                await Navigation.PushAsync(new MapPage(_danhSachQuanOc));
            }
            else
            {
                await DisplayAlert("Thông báo", "Chưa tải được dữ liệu, vui lòng đợi!", "OK");
            }

            if (sender is Button btn2) btn2.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi Chuyển Trang", ex.Message, "OK");
            if (sender is Button btn3) btn3.IsEnabled = true;
        }
    }
}