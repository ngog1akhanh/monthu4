using Newtonsoft.Json;
using VinhKhanh.Shared.Models;

namespace VinhKhanhFoodTour.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // Dùng đường hầm 10.0.2.2 để máy ảo gọi vào máy tính (cổng 7045 HTTPS)
        private string BaseUrl = "https://10.0.2.2:7045/api";

        public ApiService()
        {
            // BẮT ĐẦU CẤP THẺ VIP CHO ANDROID
            var handler = new HttpClientHandler();

            // Dòng code này bảo Android: "Bỏ qua lỗi bảo mật SSL nhé, cứ cho kết nối đi!"
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            };

            _httpClient = new HttpClient(handler);
            // KẾT THÚC CẤP THẺ VIP
        }

        public async Task<List<POI>> GetPOIsAsync()
        {
            try
            {
                // Cho nó 10 giây để chờ dữ liệu, nếu quá lâu thì hủy
                _httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = await _httpClient.GetStringAsync($"{BaseUrl}/POIs");
                return JsonConvert.DeserializeObject<List<POI>>(response) ?? new List<POI>();
            }
            catch (Exception ex)
            {
                // In lỗi ra để lập trình viên đọc
                System.Diagnostics.Debug.WriteLine($"[LỖI KẾT NỐI API]: {ex.Message}");
                return new List<POI>();
            }
        }
    }
}