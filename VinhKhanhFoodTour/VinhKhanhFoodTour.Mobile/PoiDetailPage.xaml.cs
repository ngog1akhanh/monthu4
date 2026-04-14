using VinhKhanh.Shared.Models;

namespace VinhKhanhFoodTour.Mobile;

public partial class PoiDetailPage : ContentPage
{
    private POI _currentPoi;

    // Biến này dùng để dừng âm thanh nếu người dùng bấm "Dừng" giữa chừng
    private CancellationTokenSource? _audioCancellationToken;

    public PoiDetailPage(POI poi)
    {
        InitializeComponent();
        _currentPoi = poi;
        BindingContext = _currentPoi;
    }

    // ==========================================
    // 1. Hàm xử lý nút Chỉ đường
    // ==========================================
    private async void OnDirectionsClicked(object? sender, EventArgs e)
    {
        try
        {
            var location = new Location(_currentPoi.Latitude, _currentPoi.Longitude);
            var options = new MapLaunchOptions
            {
                Name = _currentPoi.Name,
                NavigationMode = NavigationMode.Driving
            };
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể mở ứng dụng bản đồ: " + ex.Message, "OK");
        }
    }

    // ==========================================
    // 2. Hàm xử lý nút Phát Audio Đa Ngôn Ngữ (Lấy từ DB)
    // ==========================================
    private async void OnPlayAudioClicked(object? sender, EventArgs e)
    {
        if (_audioCancellationToken != null && !_audioCancellationToken.IsCancellationRequested)
        {
            _audioCancellationToken.Cancel();
            BtnPlayAudio.Text = "🎧 Phát";
            BtnPlayAudio.BackgroundColor = Microsoft.Maui.Graphics.Colors.MediumSeaGreen;
            return;
        }

        BtnPlayAudio.Text = "⏹ Dừng";
        BtnPlayAudio.BackgroundColor = Microsoft.Maui.Graphics.Colors.OrangeRed;
        _audioCancellationToken = new CancellationTokenSource();

        string selectedLang = LanguagePicker.SelectedItem?.ToString() ?? "🇻🇳 Tiếng Việt";
        string textToSpeak = "";
        string localeCode = "";

        // 3. TẠO KỊCH BẢN THUYẾT MINH: Ghép lời chào + Nội dung từ DB
        switch (selectedLang)
        {
            case "🇬🇧 English":
                string descEN = !string.IsNullOrEmpty(_currentPoi.Description_EN) ? _currentPoi.Description_EN : "";
                textToSpeak = $"Welcome to {_currentPoi.Name}. {descEN}";
                localeCode = "en";
                break;
            case "🇨🇳 中文 (Chinese)":
                string descZH = !string.IsNullOrEmpty(_currentPoi.Description_ZH) ? _currentPoi.Description_ZH : "";
                textToSpeak = $"欢迎来到 {_currentPoi.Name}. {descZH}";
                localeCode = "zh";
                break;
            case "🇯🇵 日本語 (Japanese)":
                string descJA = !string.IsNullOrEmpty(_currentPoi.Description_JA) ? _currentPoi.Description_JA : "";
                textToSpeak = $"{_currentPoi.Name} へようこそ. {descJA}";
                localeCode = "ja";
                break;
            default: // Tiếng Việt
                textToSpeak = $"Chào mừng bạn đến với {_currentPoi.Name}. {_currentPoi.Description_VI}";
                localeCode = "vi";
                break;
        }

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var selectedLocale = locales.FirstOrDefault(l => l.Language.StartsWith(localeCode, StringComparison.OrdinalIgnoreCase));

            var options = new SpeechOptions()
            {
                Volume = 1.0f,
                Locale = selectedLocale
            };

            await TextToSpeech.Default.SpeakAsync(textToSpeak, options, cancelToken: _audioCancellationToken.Token);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Tính năng đọc không khả dụng: " + ex.Message, "OK");
        }
        finally
        {
            BtnPlayAudio.Text = "🎧 Phát";
            BtnPlayAudio.BackgroundColor = Microsoft.Maui.Graphics.Colors.MediumSeaGreen;
            _audioCancellationToken?.Dispose();
            _audioCancellationToken = null;
        }
    }
}