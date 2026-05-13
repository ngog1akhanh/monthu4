using System.Net.Http.Json;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Mobile.Models;

namespace Mobile.Services;

public sealed class PoiService
{
    private const string BackendBaseUrlPreferenceKey = "BackendBaseUrl";
    private const string DeviceIdPreferenceKey = "TourGuideDeviceId";
    private const string VisitorIdPreferenceKey = "TourGuideVisitorId";
    private const string SessionIdPreferenceKey = "TourGuideSessionId";
    private const string SessionLastSeenPreferenceKey = "TourGuideSessionLastSeen";
    private const int SessionIdleRotationMinutes = 30;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly OfflineStore _offlineStore;

    public PoiService()
        : this(new OfflineStore())
    {
    }

    public PoiService(OfflineStore offlineStore)
    {
        _offlineStore = offlineStore;
        _httpClient = new HttpClient(CreateHttpMessageHandler())
        {
            Timeout = TimeSpan.FromSeconds(20),
        };
    }

    public string DeviceId => EnsurePreference(DeviceIdPreferenceKey, "device");
    public string VisitorId => EnsurePreference(VisitorIdPreferenceKey, "visitor");
    public string SessionId => EnsureSessionPreference();

    public static string BackendBaseUrlOverride => Preferences.Default.Get(BackendBaseUrlPreferenceKey, string.Empty);

    public static string SaveBackendBaseUrl(string value)
    {
        var normalized = NormalizeBackendBaseUrl(value)
            ?? throw new ArgumentException("Backend URL must be an HTTP or HTTPS absolute URL.", nameof(value));
        Preferences.Default.Set(BackendBaseUrlPreferenceKey, normalized);
        return normalized;
    }

    public static void ClearBackendBaseUrl()
    {
        Preferences.Default.Remove(BackendBaseUrlPreferenceKey);
    }

    public static async Task<(bool Ok, string Message, string BaseUrl)> TestBackendConnectionAsync(string? baseUrl)
    {
        var targets = string.IsNullOrWhiteSpace(baseUrl)
            ? ResolveBackendBaseUrls()
            : new[] { NormalizeBackendBaseUrl(baseUrl) ?? string.Empty };

        foreach (var target in targets.Where(target => !string.IsNullOrWhiteSpace(target)))
        {
            try
            {
                using var client = new HttpClient(CreateHttpMessageHandler())
                {
                    Timeout = TimeSpan.FromSeconds(8),
                };
                using var response = await client.GetAsync(BuildUri(target, "api/health/live"));
                if (response.IsSuccessStatusCode)
                {
                    return (true, "API connection is healthy.", target);
                }

                return (false, $"API returned {(int)response.StatusCode}.", target);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                if (target == targets.Last())
                {
                    return (false, ex.Message, target);
                }
            }
        }

        return (false, "Backend URL is invalid.", string.Empty);
    }

    public async Task<List<PoiModel>> GetPoisAsync()
    {
        try
        {
            var page = await GetFromJsonWithFallbackAsync<PagedResponse<PoiModel>>(
                "api/poi/approved?page=1&pageSize=100",
                JsonOptions);

            var items = page?.Items?.Select(NormalizePoi).ToList() ?? new List<PoiModel>();
            await _offlineStore.SavePoisAsync(items);
            _ = FlushPendingEventsAsync();
            return items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load approved POIs: {ex.Message}");
            return await _offlineStore.GetApprovedPoisAsync();
        }
    }

    public async Task<PoiModel?> GetPoiByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        try
        {
            using var response = await GetWithFallbackAsync($"api/poi/details/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var detail = await response.Content.ReadFromJsonAsync<PoiPublicDetailResponse>(JsonOptions);
            if (detail == null)
            {
                return null;
            }

            var model = MapPublicDetail(detail);
            await _offlineStore.SavePoiAsync(model);
            return model;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load POI detail: {ex.Message}");
            return await _offlineStore.GetPoiAsync(id);
        }
    }

    public async Task<List<PoiModel>> GetNearbyPoisAsync(double longitude, double latitude, double maxDistanceInMeters)
    {
        try
        {
            var url = $"api/poi/nearby?longitude={longitude}&latitude={latitude}&maxDistance={maxDistanceInMeters}";
            var items = await GetFromJsonWithFallbackAsync<List<PoiModel>>(url, JsonOptions);
            var normalized = items?.Select(NormalizePoi).ToList() ?? new List<PoiModel>();
            await _offlineStore.SavePoisAsync(normalized);
            return normalized;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load nearby POIs: {ex.Message}");
            var cached = await _offlineStore.GetApprovedPoisAsync();
            return cached
                .Where(poi => poi.Latitude != 0 || poi.Longitude != 0)
                .Select(poi =>
                {
                    poi.Distance = Location.CalculateDistance(latitude, longitude, poi.Latitude, poi.Longitude, DistanceUnits.Kilometers) * 1000;
                    return poi;
                })
                .Where(poi => poi.Distance <= maxDistanceInMeters)
                .OrderBy(poi => poi.Distance)
                .ToList();
        }
    }

    public async Task<QrScanResult?> RecordQrScanAsync(string poiId)
    {
        try
        {
            using var response = await PostAsJsonWithFallbackAsync(
                "api/qr/scan",
                new
                {
                    poiId,
                    visitorId = VisitorId,
                    sessionId = SessionId,
                    triggerSource = "MobileQR",
                });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<QrScanResult>(JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to record QR scan: {ex.Message}");
            return null;
        }
    }

    public async Task<NarrationPlayResult?> StartNarrationAsync(string poiId, string language = "VI")
    {
        try
        {
            using var response = await PostAsJsonWithFallbackAsync(
                "api/narration/play",
                new
                {
                    poiId,
                    visitorId = VisitorId,
                    sessionId = SessionId,
                    language,
                    triggerSource = "MobileTTS",
                });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<NarrationPlayResult>(JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start narration: {ex.Message}");
            return null;
        }
    }

    public async Task FinishNarrationAsync(string logId, string status, int dwellTimeSeconds, string errorCode = "")
    {
        if (string.IsNullOrWhiteSpace(logId))
        {
            return;
        }

        try
        {
            using var response = await PostAsJsonWithFallbackAsync(
                "api/narration/finish",
                new
                {
                    logId,
                    status,
                    dwellTimeSeconds,
                    errorCode,
                });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to finish narration: {ex.Message}");
            await _offlineStore.EnqueueEventAsync(
                "narration_finish",
                "api/narration/finish",
                new
                {
                    logId,
                    status,
                    dwellTimeSeconds,
                    errorCode,
                });
        }
    }

    public async Task SendPingAsync(Location? location = null)
    {
        double latitude = 0;
        double longitude = 0;
        double speed = 0;

        try
        {
            location ??= await Geolocation.Default.GetLastKnownLocationAsync()
                ?? await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));

            if (location != null)
            {
                latitude = location.Latitude;
                longitude = location.Longitude;
                speed = location.Speed ?? 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location unavailable for ping: {ex.Message}");
        }

        var payload = new
        {
            deviceId = DeviceId,
            userId = "",
            sessionId = SessionId,
            latitude,
            longitude,
            speed,
            platform = DeviceInfo.Platform.ToString(),
            appVersion = AppInfo.VersionString,
            deviceName = DeviceInfo.Name,
        };

        try
        {
            using var response = await PostAsJsonWithFallbackAsync(
                "api/tracking/ping",
                payload);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send ping: {ex.Message}");
            await _offlineStore.EnqueueEventAsync("tracking_ping", "api/tracking/ping", payload);
        }
    }

    public async Task FlushPendingEventsAsync()
    {
        var events = await _offlineStore.GetPendingEventsAsync();
        foreach (var pending in events)
        {
            try
            {
                using var response = await PostJsonStringWithFallbackAsync(pending.RelativeUrl, pending.PayloadJson);
                if (!response.IsSuccessStatusCode)
                {
                    await _offlineStore.MarkPendingEventAttemptAsync(pending, response.StatusCode.ToString());
                    continue;
                }

                await _offlineStore.DeletePendingEventAsync(pending.Id);
            }
            catch (Exception ex)
            {
                await _offlineStore.MarkPendingEventAttemptAsync(pending, ex.Message);
                break;
            }
        }
    }

    private async Task<T?> GetFromJsonWithFallbackAsync<T>(string relativeUrl, JsonSerializerOptions options)
    {
        using var response = await GetWithFallbackAsync(relativeUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(options);
    }

    private Task<HttpResponseMessage> GetWithFallbackAsync(string relativeUrl)
    {
        return SendWithFallbackAsync(baseUrl => _httpClient.GetAsync(BuildUri(baseUrl, relativeUrl)));
    }

    private Task<HttpResponseMessage> PostAsJsonWithFallbackAsync<TValue>(string relativeUrl, TValue value)
    {
        return SendWithFallbackAsync(baseUrl => _httpClient.PostAsJsonAsync(BuildUri(baseUrl, relativeUrl), value, JsonOptions));
    }

    private Task<HttpResponseMessage> PostJsonStringWithFallbackAsync(string relativeUrl, string json)
    {
        return SendWithFallbackAsync(baseUrl =>
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return _httpClient.PostAsync(BuildUri(baseUrl, relativeUrl), content);
        });
    }

    private async Task<HttpResponseMessage> SendWithFallbackAsync(Func<string, Task<HttpResponseMessage>> send)
    {
        Exception? lastError = null;
        var baseUrls = ResolveBackendBaseUrls();
        foreach (var baseUrl in baseUrls)
        {
            try
            {
                var response = await send(baseUrl);
                var statusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode || statusCode is >= 400 and < 500)
                {
                    return response;
                }

                lastError = new HttpRequestException($"API returned {(int)response.StatusCode} from {baseUrl}.");
                response.Dispose();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
                System.Diagnostics.Debug.WriteLine($"API base URL failed: {baseUrl} - {ex.Message}");
            }
        }

        throw new HttpRequestException($"Cannot reach TourGuide API. Last error: {lastError?.Message}", lastError);
    }

    private static Uri BuildUri(string baseUrl, string relativeUrl)
    {
        return new Uri(new Uri(baseUrl), relativeUrl);
    }

    private static PoiModel NormalizePoi(PoiModel poi)
    {
        if (poi.Location == null && (poi.Latitude != 0 || poi.Longitude != 0))
        {
            poi.Location = new GeoLocation
            {
                Coordinates = new[] { poi.Longitude, poi.Latitude },
            };
        }

        return poi;
    }

    private static PoiModel MapPublicDetail(PoiPublicDetailResponse detail)
    {
        var model = new PoiModel
        {
            Id = detail.Id,
            Name = detail.Name,
            Address = detail.Address,
            Tags = detail.Tags.ToList(),
            ImageUrl = detail.ImageUrl,
            Latitude = detail.Latitude,
            Longitude = detail.Longitude,
            Location = new GeoLocation { Coordinates = new[] { detail.Longitude, detail.Latitude } },
            Description_VI = GetContent(detail, "VI").Description,
            Description_EN = GetContent(detail, "EN").Description,
            Description_KO = GetContent(detail, "KO").Description,
            Description_JA = GetContent(detail, "JA").Description,
            Description_ZH = GetContent(detail, "ZH").Description,
            AudioUrl_VI = GetContent(detail, "VI").AudioUrl,
            AudioUrl_EN = GetContent(detail, "EN").AudioUrl,
            AudioUrl_KO = GetContent(detail, "KO").AudioUrl,
            AudioUrl_JA = GetContent(detail, "JA").AudioUrl,
            AudioUrl_ZH = GetContent(detail, "ZH").AudioUrl,
        };

        model.SourceDescription = model.Description_VI
            ?? model.Description_EN
            ?? model.Description_KO
            ?? model.Description_JA
            ?? model.Description_ZH;

        return model;
    }

    private static LocalizedContent GetContent(PoiPublicDetailResponse detail, string language)
    {
        return detail.Contents.TryGetValue(language, out var content)
            ? content
            : new LocalizedContent();
    }

    private static IReadOnlyList<string> ResolveBackendBaseUrls()
    {
        var configured = Preferences.Default.Get(BackendBaseUrlPreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var normalized = NormalizeBackendBaseUrl(configured);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return new[] { normalized };
            }
        }

        if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.DeviceType == DeviceType.Virtual)
        {
            return new[]
            {
                "http://10.0.2.2:5276/",
                "http://127.0.0.1:5276/",
                "https://10.0.2.2:7095/",
            };
        }

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            return new[]
            {
                "http://127.0.0.1:5276/",
                "http://localhost:5276/",
            };
        }

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            return new[]
            {
                "https://localhost:7095/",
                "http://localhost:5276/",
            };
        }

        return new[] { "https://localhost:7095/" };
    }

    private static HttpMessageHandler CreateHttpMessageHandler()
    {
#if DEBUG
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };
#else
        return new HttpClientHandler();
#endif
    }

    private static string? NormalizeBackendBaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        return trimmed.EndsWith("/", StringComparison.Ordinal) ? trimmed : trimmed + "/";
    }

    private static string EnsurePreference(string key, string prefix)
    {
        var value = Preferences.Default.Get(key, string.Empty);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        value = $"{prefix}-{Guid.NewGuid():N}";
        Preferences.Default.Set(key, value);
        return value;
    }

    private static string EnsureSessionPreference()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var lastSeenText = Preferences.Default.Get(SessionLastSeenPreferenceKey, string.Empty);
        _ = long.TryParse(lastSeenText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lastSeen);

        var value = Preferences.Default.Get(SessionIdPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(value) || now - lastSeen > SessionIdleRotationMinutes * 60)
        {
            value = $"session-{Guid.NewGuid():N}";
            Preferences.Default.Set(SessionIdPreferenceKey, value);
        }

        Preferences.Default.Set(SessionLastSeenPreferenceKey, now.ToString(CultureInfo.InvariantCulture));
        return value;
    }

    private sealed class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
    }

    private sealed class PoiPublicDetailResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string ImageUrl { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Dictionary<string, LocalizedContent> Contents { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class LocalizedContent
    {
        public string Description { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

public sealed class QrScanResult
{
    public bool Counted { get; set; }
    public bool InCooldown { get; set; }
    public DateTime CooldownEndsAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class NarrationPlayResult
{
    public string LogId { get; set; } = string.Empty;
    public bool Counted { get; set; }
    public bool RateLimited { get; set; }
}
