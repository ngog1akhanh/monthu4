using VinhKhanh.Shared.Models;

namespace VinhKhanhFoodTour.Mobile.Services;

public class TourEngine
{
    private List<POI> _pois;
    private bool _isRunning = false;
    private CancellationTokenSource _cancelTokenSource;

    public TourEngine(List<POI> pois)
    {
        _pois = pois;
        // Đảm bảo khi bắt đầu tour, chưa có quán nào bị đánh dấu là "đã phát"
        foreach (var poi in _pois) poi.IsPlayed = false;
    }

    // BẬT ĐỘNG CƠ QUÉT
    public void StartTour()
    {
        if (_isRunning) return;
        _isRunning = true;
        _cancelTokenSource = new CancellationTokenSource();

        // Chạy vòng lặp kiểm tra vị trí ngầm (không làm đơ giao diện)
        Task.Run(() => TrackingLoop(_cancelTokenSource.Token));
    }

    // TẮT ĐỘNG CƠ
    public void StopTour()
    {
        _isRunning = false;
        _cancelTokenSource?.Cancel();
    }

    private async Task TrackingLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 1. Lấy vị trí hiện tại của người dùng (Độ chính xác cao)
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
                var userLocation = await Geolocation.Default.GetLocationAsync(request, token);

                if (userLocation != null)
                {
                    // 2. So sánh vị trí người dùng với TẤT CẢ các quán ốc chưa được phát
                    foreach (var poi in _pois.Where(p => !p.IsPlayed))
                    {
                        var poiLocation = new Location(poi.Latitude, poi.Longitude);

                        // Tính khoảng cách (ra đơn vị Kilomet, đổi sang Mét)
                        double distanceInMeters = Location.CalculateDistance(userLocation, poiLocation, DistanceUnits.Kilometers) * 1000;

                        // 3. NẾU BƯỚC VÀO VÙNG BÁN KÍNH -> PHÁT ÂM THANH!
                        if (distanceInMeters <= poi.Radius)
                        {
                            poi.IsPlayed = true; // Đánh dấu đã phát để không nói lại
                            await PlayAudioForPoi(poi);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GPS: {ex.Message}");
            }

            // Nghỉ 5 giây rồi mới quét tiếp cho đỡ tốn pin
            await Task.Delay(5000, token);
        }
    }

    private async Task PlayAudioForPoi(POI poi)
    {
        // Chuyển về luồng chính để an toàn
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Tạm thời fix cứng đọc Tiếng Việt cho tính năng tự động
            string text = $"Bạn đang đi ngang qua {poi.Name}. {poi.Description_VI}";
            await TextToSpeech.Default.SpeakAsync(text);
        });
    }
}