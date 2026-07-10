using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PeePooFinder.Models;

namespace PeePooFinder.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}

public class SubmitPlaceData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public int Rating { get; set; }
    public double Long { get; set; }
    public double Lat { get; set; }
    public bool HaveBabyChanger { get; set; }
    public bool IsRoomy { get; set; }
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
    public string? Type { get; set; }
    public bool? BabyChanger { get; set; }
    public bool? Roomy { get; set; }
    public bool? AvailableOnly { get; set; }

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
        Add("type", Type);
        if (BabyChanger == true) Add("babyChanger", "true");
        if (Roomy == true) Add("roomy", "true");
        if (AvailableOnly == true) Add("availableOnly", "true");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public interface IPeePooApi
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<List<Place>> GetPlacesAsync(PlaceQuery? query = null);
    Task<Place?> GetPlaceAsync(string id);
    Task<List<Review>> GetReviewsAsync(string placeId);
    Task CreatePlaceAsync(SubmitPlaceData data);
    Task CreateReviewAsync(SubmitReviewData data);
    Task ToggleFavoriteAsync(string placeId);
    Task VerifyPlaceAsync(string placeId);
    Task ReportPlaceAsync(string placeId, string reason);
    Task ReportReviewAsync(string reviewId, string reason);
    Task BlockUserAsync(string username);
    Task<Profile?> GetProfileAsync(string username);
    Task UpdateProfileAsync(string displayName, string bio);
    Task DeleteAccountAsync();
}

public class PeePooApiClient : IPeePooApi
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PeePooApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/account/login", request, JsonOptions);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions))!;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/account/register", request, JsonOptions);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions))!;
    }

    public async Task<List<Place>> GetPlacesAsync(PlaceQuery? query = null)
    {
        var url = "api/places" + (query?.ToQueryString() ?? string.Empty);
        return await _http.GetFromJsonAsync<List<Place>>(url, JsonOptions) ?? new List<Place>();
    }

    public async Task<Place?> GetPlaceAsync(string id)
    {
        var response = await _http.GetAsync($"api/places/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<Place>(JsonOptions);
    }

    public async Task<List<Review>> GetReviewsAsync(string placeId)
    {
        return await _http.GetFromJsonAsync<List<Review>>($"api/Visits/visitsFromPlace/{placeId}", JsonOptions)
               ?? new List<Review>();
    }

    public async Task CreatePlaceAsync(SubmitPlaceData data)
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(data.Id), "Id" },
            { new StringContent(data.Name), "Name" },
            { new StringContent(data.Description), "Description" },
            { new StringContent(data.Type), "Type" },
            { new StringContent(data.Observations), "Observations" },
            { new StringContent(data.Rating.ToString()), "Rating" },
            { new StringContent(data.Long.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Long" },
            { new StringContent(data.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Lat" },
            { new StringContent(data.HaveBabyChanger.ToString()), "HaveBabyChanger" },
            { new StringContent(data.IsRoomy.ToString()), "IsRoomy" },
            { new StringContent(data.IsAvailable.ToString()), "IsAvailable" },
            { new StringContent(data.Urinals.ToString()), "Urinals" },
            { new StringContent(data.Toilets.ToString()), "Toilets" },
        };
        AddImage(content, data.ImageBytes, data.ImageName);

        var response = await _http.PostAsync("api/places", content);
        await EnsureSuccess(response);
    }

    public async Task CreateReviewAsync(SubmitReviewData data)
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(data.Id), "Id" },
            { new StringContent(data.PlaceId), "PlaceId" },
            { new StringContent(data.Title), "Title" },
            { new StringContent(data.Description), "Description" },
            { new StringContent(data.Rating.ToString()), "Rating" },
        };
        AddImage(content, data.ImageBytes, data.ImageName);

        var response = await _http.PostAsync("api/visits", content);
        await EnsureSuccess(response);
    }

    public async Task ToggleFavoriteAsync(string placeId)
    {
        var response = await _http.PostAsync($"api/places/{placeId}/favorite", null);
        await EnsureSuccess(response);
    }

    public async Task VerifyPlaceAsync(string placeId)
    {
        var response = await _http.PostAsync($"api/places/{placeId}/verify", null);
        await EnsureSuccess(response);
    }

    public async Task ReportPlaceAsync(string placeId, string reason)
    {
        var response = await _http.PostAsJsonAsync($"api/places/{placeId}/report", new { reason }, JsonOptions);
        await EnsureSuccess(response);
    }

    public async Task ReportReviewAsync(string reviewId, string reason)
    {
        var response = await _http.PostAsJsonAsync($"api/visits/{reviewId}/report", new { reason }, JsonOptions);
        await EnsureSuccess(response);
    }

    public async Task BlockUserAsync(string username)
    {
        var response = await _http.PostAsync($"api/profiles/{Uri.EscapeDataString(username)}/block", null);
        await EnsureSuccess(response);
    }

    public async Task DeleteAccountAsync()
    {
        var response = await _http.DeleteAsync("api/account");
        await EnsureSuccess(response);
    }

    public async Task<Profile?> GetProfileAsync(string username)
    {
        var response = await _http.GetAsync($"api/profiles/{username}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<Profile>(JsonOptions);
    }

    public async Task UpdateProfileAsync(string displayName, string bio)
    {
        var response = await _http.PutAsJsonAsync("api/profiles", new { displayName, bio }, JsonOptions);
        await EnsureSuccess(response);
    }

    private static void AddImage(MultipartFormDataContent content, byte[]? bytes, string? name)
    {
        if (bytes is null || bytes.Length == 0) return;
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "File", string.IsNullOrEmpty(name) ? "photo.jpg" : name);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var message = "Something went wrong. Please try again.";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
            if (!string.IsNullOrWhiteSpace(error?.Message))
                message = error!.Message!;
        }
        catch
        {
            // response body was not a JSON error payload
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized && message.StartsWith("Something"))
            message = "Your session has expired. Please sign in again.";

        throw new ApiException(message);
    }
}
