using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PeePooFinder.Models;

namespace PeePooFinder.Services;

public class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode? status = null) : base(message)
    {
        Status = status;
    }

    public HttpStatusCode? Status { get; }
    public bool IsUnauthorized => Status == HttpStatusCode.Unauthorized;
}

public class SubmitPlaceData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public int Rating { get; set; }
    public double Long { get; set; }
    public double Lat { get; set; }
    public bool HaveBabyChanger { get; set; }
    public bool IsRoomy { get; set; }
    public bool IsAccessible { get; set; }
    public bool IsFree { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public int Urinals { get; set; }
    public int Toilets { get; set; }
    public byte[]? ImageBytes { get; set; }
    public string? ImageName { get; set; }
}

public class SubmitReviewData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PlaceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Rating { get; set; }
    public byte[]? ImageBytes { get; set; }
    public string? ImageName { get; set; }
}

public class PlaceQuery
{
    public double? Lat { get; set; }
    public double? Long { get; set; }
    public double? RadiusKm { get; set; }
    public string? Search { get; set; }
    public string? Type { get; set; }
    public bool? BabyChanger { get; set; }
    public bool? Accessible { get; set; }
    public bool? Free { get; set; }
    public bool? Roomy { get; set; }
    public bool? AvailableOnly { get; set; }
    public string? Sort { get; set; }
    public int? Limit { get; set; }

    public string ToQueryString()
    {
        var parts = new List<string>();
        void Add(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                parts.Add($"{key}={Uri.EscapeDataString(value)}");
        }

        if (Lat.HasValue) Add("lat", Lat.Value.ToString(CultureInfo.InvariantCulture));
        if (Long.HasValue) Add("long", Long.Value.ToString(CultureInfo.InvariantCulture));
        if (RadiusKm.HasValue) Add("radiusKm", RadiusKm.Value.ToString(CultureInfo.InvariantCulture));
        Add("q", Search?.Trim());
        Add("type", Type);
        if (BabyChanger == true) Add("babyChanger", "true");
        if (Accessible == true) Add("accessible", "true");
        if (Free == true) Add("free", "true");
        if (Roomy == true) Add("roomy", "true");
        if (AvailableOnly == true) Add("availableOnly", "true");
        Add("sort", Sort);
        if (Limit.HasValue) Add("limit", Limit.Value.ToString(CultureInfo.InvariantCulture));

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public interface IPeePooApi
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> ChangePasswordAsync(string currentPassword, string newPassword);
    Task DeleteAccountAsync();

    Task<List<Place>> GetPlacesAsync(PlaceQuery? query = null);
    Task<Place?> GetPlaceAsync(string id);
    Task<List<Place>> GetSavedPlacesAsync();
    Task<List<Place>> GetMyPlacesAsync();
    Task CreatePlaceAsync(SubmitPlaceData data);
    Task<bool> ToggleFavoriteAsync(string placeId);
    Task SetAvailabilityAsync(string placeId, bool isAvailable);
    Task VerifyPlaceAsync(string placeId);
    Task DeletePlaceAsync(string placeId);
    Task ReportPlaceAsync(string placeId, string reason);

    Task<List<Review>> GetReviewsAsync(string placeId);
    Task<Review?> GetReviewAsync(string reviewId);
    Task<List<Review>> GetMyReviewsAsync();
    Task CreateReviewAsync(SubmitReviewData data);
    Task UpdateReviewAsync(string reviewId, string title, string description, int rating);
    Task DeleteReviewAsync(string reviewId);
    Task ReportReviewAsync(string reviewId, string reason);

    Task<Profile?> GetProfileAsync(string username);
    Task UpdateProfileAsync(string displayName, string bio);
    Task BlockUserAsync(string username);
    Task UnblockUserAsync(string username);
    Task<List<string>> GetBlockedUsersAsync();
}

public class PeePooApiClient : IPeePooApi
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PeePooApiClient(HttpClient http)
    {
        _http = http;
    }

    // ── Account ─────────────────────────────────
    public Task<AuthResponse> LoginAsync(LoginRequest request) => PostForAsync<AuthResponse>("api/account/login", request);

    public Task<AuthResponse> RegisterAsync(RegisterRequest request) => PostForAsync<AuthResponse>("api/account/register", request);

    public Task<AuthResponse> ChangePasswordAsync(string currentPassword, string newPassword) =>
        PostForAsync<AuthResponse>("api/account/password", new { currentPassword, newPassword });

    public Task DeleteAccountAsync() => SendAsync(HttpMethod.Delete, "api/account");

    // ── Places ──────────────────────────────────
    public Task<List<Place>> GetPlacesAsync(PlaceQuery? query = null) =>
        GetListAsync<Place>("api/places" + (query?.ToQueryString() ?? string.Empty));

    public Task<Place?> GetPlaceAsync(string id) => GetOrNullAsync<Place>($"api/places/{id}");

    public Task<List<Place>> GetSavedPlacesAsync() => GetListAsync<Place>("api/places/saved");

    public Task<List<Place>> GetMyPlacesAsync() => GetListAsync<Place>("api/places/mine");

    public async Task CreatePlaceAsync(SubmitPlaceData data)
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(data.Id), "Id" },
            { new StringContent(data.Name), "Name" },
            { new StringContent(data.Description), "Description" },
            { new StringContent(data.Type), "Type" },
            { new StringContent(data.Observations), "Observations" },
            { new StringContent(data.Address), "Address" },
            { new StringContent(data.OpeningHours), "OpeningHours" },
            { new StringContent(data.Rating.ToString(CultureInfo.InvariantCulture)), "Rating" },
            { new StringContent(data.Long.ToString(CultureInfo.InvariantCulture)), "Long" },
            { new StringContent(data.Lat.ToString(CultureInfo.InvariantCulture)), "Lat" },
            { new StringContent(Bool(data.HaveBabyChanger)), "HaveBabyChanger" },
            { new StringContent(Bool(data.IsRoomy)), "IsRoomy" },
            { new StringContent(Bool(data.IsAccessible)), "IsAccessible" },
            { new StringContent(Bool(data.IsFree)), "IsFree" },
            { new StringContent(Bool(data.IsAvailable)), "IsAvailable" },
            { new StringContent(data.Urinals.ToString(CultureInfo.InvariantCulture)), "Urinals" },
            { new StringContent(data.Toilets.ToString(CultureInfo.InvariantCulture)), "Toilets" },
        };
        AddImage(content, data.ImageBytes, data.ImageName);

        var response = await _http.PostAsync("api/places", content);
        await EnsureSuccess(response);
    }

    public async Task<bool> ToggleFavoriteAsync(string placeId)
    {
        var response = await _http.PostAsync($"api/places/{placeId}/favorite", null);
        await EnsureSuccess(response);
        var state = await response.Content.ReadFromJsonAsync<FavoriteState>(JsonOptions);
        return state?.IsFavorite ?? false;
    }

    public Task SetAvailabilityAsync(string placeId, bool isAvailable) =>
        PostAsync($"api/places/{placeId}/availability", new { isAvailable });

    public Task VerifyPlaceAsync(string placeId) => PostAsync($"api/places/{placeId}/verify", null);

    public Task DeletePlaceAsync(string placeId) => SendAsync(HttpMethod.Delete, $"api/places/{placeId}");

    public Task ReportPlaceAsync(string placeId, string reason) => PostAsync($"api/places/{placeId}/report", new { reason });

    // ── Reviews ─────────────────────────────────
    public Task<List<Review>> GetReviewsAsync(string placeId) => GetListAsync<Review>($"api/visits/visitsFromPlace/{placeId}");

    public Task<Review?> GetReviewAsync(string reviewId) => GetOrNullAsync<Review>($"api/visits/{reviewId}");

    public Task<List<Review>> GetMyReviewsAsync() => GetListAsync<Review>("api/visits/mine");

    public async Task CreateReviewAsync(SubmitReviewData data)
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(data.Id), "Id" },
            { new StringContent(data.PlaceId), "PlaceId" },
            { new StringContent(data.Title), "Title" },
            { new StringContent(data.Description), "Description" },
            { new StringContent(data.Rating.ToString(CultureInfo.InvariantCulture)), "Rating" },
        };
        AddImage(content, data.ImageBytes, data.ImageName);

        var response = await _http.PostAsync("api/visits", content);
        await EnsureSuccess(response);
    }

    public async Task UpdateReviewAsync(string reviewId, string title, string description, int rating)
    {
        var response = await _http.PutAsJsonAsync($"api/visits/{reviewId}", new { title, description, rating }, JsonOptions);
        await EnsureSuccess(response);
    }

    public Task DeleteReviewAsync(string reviewId) => SendAsync(HttpMethod.Delete, $"api/visits/{reviewId}");

    public Task ReportReviewAsync(string reviewId, string reason) => PostAsync($"api/visits/{reviewId}/report", new { reason });

    // ── Profiles ────────────────────────────────
    public Task<Profile?> GetProfileAsync(string username) => GetOrNullAsync<Profile>($"api/profiles/{Uri.EscapeDataString(username)}");

    public async Task UpdateProfileAsync(string displayName, string bio)
    {
        var response = await _http.PutAsJsonAsync("api/profiles", new { displayName, bio }, JsonOptions);
        await EnsureSuccess(response);
    }

    public Task BlockUserAsync(string username) => PostAsync($"api/profiles/{Uri.EscapeDataString(username)}/block", null);

    public Task UnblockUserAsync(string username) => SendAsync(HttpMethod.Delete, $"api/profiles/{Uri.EscapeDataString(username)}/block");

    public Task<List<string>> GetBlockedUsersAsync() => GetListAsync<string>("api/profiles/blocked");

    // ── Plumbing ────────────────────────────────
    private static string Bool(bool value) => value ? "true" : "false";

    private async Task<List<T>> GetListAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions) ?? new List<T>();
    }

    private async Task<T?> GetOrNullAsync<T>(string url) where T : class
    {
        var response = await _http.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T> PostForAsync<T>(string url, object body)
    {
        var response = await _http.PostAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    private async Task PostAsync(string url, object? body)
    {
        var response = body == null
            ? await _http.PostAsync(url, null)
            : await _http.PostAsJsonAsync(url, body, JsonOptions);
        await EnsureSuccess(response);
    }

    private async Task SendAsync(HttpMethod method, string url)
    {
        var response = await _http.SendAsync(new HttpRequestMessage(method, url));
        await EnsureSuccess(response);
    }

    private static void AddImage(MultipartFormDataContent content, byte[]? bytes, string? name)
    {
        if (bytes is null || bytes.Length == 0) return;
        var fileName = string.IsNullOrEmpty(name) ? "photo.jpg" : name;
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentTypeFor(fileName));
        content.Add(imageContent, "File", fileName);
    }

    private static string ContentTypeFor(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".heic" => "image/heic",
        ".heif" => "image/heif",
        _ => "image/jpeg"
    };

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        string? message = null;
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
            message = error?.Message;
        }
        catch
        {
            // body was empty or not a JSON error payload
        }

        message = response.StatusCode switch
        {
            _ when !string.IsNullOrWhiteSpace(message) => message,
            HttpStatusCode.Unauthorized => "Please sign in again.",
            HttpStatusCode.Forbidden => "You can't do that.",
            HttpStatusCode.NotFound => "That no longer exists.",
            HttpStatusCode.TooManyRequests => "Too many requests. Please wait a moment.",
            _ => "Something went wrong. Please try again."
        };

        throw new ApiException(message, response.StatusCode);
    }
}
