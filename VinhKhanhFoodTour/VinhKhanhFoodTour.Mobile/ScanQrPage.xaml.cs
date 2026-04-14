using ZXing.Net.Maui;
using VinhKhanh.Shared.Models;

namespace VinhKhanhFoodTour.Mobile;

public partial class ScanQrPage : ContentPage
{
    private readonly List<POI> _danhSachQuanOc;
    private bool _isProcessing = false;

    // Nhận danh sách quán ốc từ Trang chủ truyền sang
    public ScanQrPage(List<POI> danhSachQuanOc)
    {
        InitializeComponent();
        _danhSachQuanOc = danhSachQuanOc;

        // Cấu hình máy quét: Chỉ quét mã QR (2D) để tăng tốc độ
        CameraReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.TwoDimensional,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CameraReader.IsDetecting = true; // Bật quét khi mở trang
        _isProcessing = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CameraReader.IsDetecting = false; // Tắt quét khi rời đi để tiết kiệm pin
    }

    // Hàm này chạy mỗi khi Camera nhìn thấy một mã vạch/QR
    private void CameraReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Chống chạm 2 lần (chỉ xử lý mã đầu tiên quét được)
        if (_isProcessing) return;

        var result = e.Results?.FirstOrDefault();
        if (result != null)
        {
            _isProcessing = true;

            // Chuyển việc cập nhật giao diện về luồng chính (Main Thread)
            Dispatcher.Dispatch(async () =>
            {
                CameraReader.IsDetecting = false; // Tạm dừng quét
                string qrContent = result.Value; // Lấy chữ trong mã QR (VD: "1")

                // Kiểm tra xem nội dung QR có phải là số (ID của quán) không
                if (int.TryParse(qrContent, out int poiId))
                {
                    // Tìm quán ốc trong danh sách có ID trùng khớp
                    var matchedPoi = _danhSachQuanOc.FirstOrDefault(p => p.Id == poiId);

                    if (matchedPoi != null)
                    {
                        // Quét đúng ID -> Mở trang chi tiết
                        await Navigation.PushAsync(new PoiDetailPage(matchedPoi));

                        // Xóa trang Camera hiện tại để khi ấn nút Back, quay thẳng về Trang chủ
                        Navigation.RemovePage(this);
                    }
                    else
                    {
                        await DisplayAlert("Lỗi", "Mã QR này không thuộc hệ thống nhà hàng của chúng tôi.", "OK");
                        _isProcessing = false;
                        CameraReader.IsDetecting = true; // Cho phép quét lại
                    }
                }
                else
                {
                    await DisplayAlert("Không hợp lệ", $"Mã QR không đúng định dạng. Nội dung: {qrContent}", "OK");
                    _isProcessing = false;
                    CameraReader.IsDetecting = true;
                }
            });
        }
    }
}