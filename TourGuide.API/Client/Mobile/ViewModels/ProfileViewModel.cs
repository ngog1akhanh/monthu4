using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private bool _isReady;

    [ObservableProperty]
    public partial string Nickname { get; set; } = "Khách";

    [ObservableProperty]
    public partial string AvatarUrl { get; set; } = "dotnet_bot.png";

    [ObservableProperty]
    public partial string CurrentLanguage { get; set; }

    [ObservableProperty]
    public partial bool IsDarkMode { get; set; }

    [ObservableProperty]
    public partial bool IsDistanceExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsQrExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsFavoritesExpanded { get; set; }

    [ObservableProperty]
    public partial string ApiBaseUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApiConnectionStatus { get; set; } = "Chưa kiểm tra kết nối API.";

    [ObservableProperty]
    public partial bool IsApiChecking { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QrHistoryModel> QrHistories { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<PoiModel> FavoriteList { get; set; } = new();

    public ProfileViewModel()
    {
        CurrentLanguage = Preferences.Default.Get("AppLang", "Tiếng Việt");
        IsDarkMode = Preferences.Default.Get("AppDarkMode", false);
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        ApiBaseUrl = PoiService.BackendBaseUrlOverride;
        _isReady = true;
    }

    partial void OnCurrentLanguageChanged(string value)
    {
        if (_isReady)
        {
            Preferences.Default.Set("AppLang", value);
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        if (!_isReady)
        {
            return;
        }

        Preferences.Default.Set("AppDarkMode", value);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            }
        });
    }

    partial void OnIsQrExpandedChanged(bool value)
    {
        if (value)
        {
            LoadQrHistory();
        }
    }

    partial void OnIsFavoritesExpandedChanged(bool value)
    {
        if (value)
        {
            LoadFavorites();
        }
    }

    [RelayCommand]
    private void RemoveFavorite(string poiId)
    {
        if (string.IsNullOrEmpty(poiId))
        {
            return;
        }

        var item = FavoriteList.FirstOrDefault(x => x.Id == poiId);
        if (item == null)
        {
            return;
        }

        FavoriteList.Remove(item);
        Preferences.Default.Set("MyFavorites", JsonSerializer.Serialize(FavoriteList.ToList()));
    }

    [RelayCommand]
    private void ToggleDistance()
    {
        IsDistanceExpanded = !IsDistanceExpanded;
    }

    [RelayCommand]
    private void ToggleQr()
    {
        IsQrExpanded = !IsQrExpanded;
    }

    [RelayCommand]
    private void ToggleFavorites()
    {
        IsFavoritesExpanded = !IsFavoritesExpanded;
    }

    [RelayCommand]
    private async Task SaveApiBaseUrlAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ApiBaseUrl))
            {
                PoiService.ClearBackendBaseUrl();
                ApiConnectionStatus = "Đã xóa URL riêng. App sẽ dùng cấu hình mặc định theo thiết bị.";
                await Shell.Current.DisplayAlert("API", ApiConnectionStatus, "OK");
                return;
            }

            ApiBaseUrl = PoiService.SaveBackendBaseUrl(ApiBaseUrl);
            ApiConnectionStatus = $"Đã lưu API URL: {ApiBaseUrl}";
            await Shell.Current.DisplayAlert("API", ApiConnectionStatus, "OK");
        }
        catch (Exception ex)
        {
            ApiConnectionStatus = $"URL không hợp lệ: {ex.Message}";
            await Shell.Current.DisplayAlert("API", ApiConnectionStatus, "OK");
        }
    }

    [RelayCommand]
    private async Task TestApiConnectionAsync()
    {
        if (IsApiChecking)
        {
            return;
        }

        IsApiChecking = true;
        try
        {
            var result = await PoiService.TestBackendConnectionAsync(ApiBaseUrl);
            ApiConnectionStatus = result.Ok
                ? $"Kết nối OK: {result.BaseUrl}"
                : $"Không kết nối được: {result.Message}";
        }
        finally
        {
            IsApiChecking = false;
        }
    }

    [RelayCommand]
    private async Task ResetApiBaseUrlAsync()
    {
        PoiService.ClearBackendBaseUrl();
        ApiBaseUrl = string.Empty;
        ApiConnectionStatus = "Đã quay về cấu hình mặc định.";
        await Shell.Current.DisplayAlert("API", ApiConnectionStatus, "OK");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc chắn muốn đăng xuất không?", "OK", "Hủy");
        if (!confirm)
        {
            return;
        }

        Preferences.Default.Clear();

        _isReady = false;
        IsDarkMode = false;
        CurrentLanguage = "Tiếng Việt";
        _isReady = true;

        await Shell.Current.DisplayAlert("Thành công", "Bạn đã đăng xuất khỏi hệ thống.", "OK");
    }

    private void LoadQrHistory()
    {
        var json = Preferences.Default.Get("QrHistory", "[]");
        var list = JsonSerializer.Deserialize<List<QrHistoryModel>>(json) ?? new List<QrHistoryModel>();
        QrHistories = new ObservableCollection<QrHistoryModel>(list);
    }

    private void LoadFavorites()
    {
        var json = Preferences.Default.Get("MyFavorites", "[]");
        var list = JsonSerializer.Deserialize<List<PoiModel>>(json) ?? new List<PoiModel>();
        FavoriteList = new ObservableCollection<PoiModel>(list);
    }
}
