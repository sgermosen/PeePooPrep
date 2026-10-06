using System.Collections.Generic;

namespace PeePooFinder.Models;

public class AuthResponse
{
    public string? DisplayName { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }
    public string? Image { get; set; }
    public bool IsAdmin { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class Place
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Observations { get; set; }
    public string? Address { get; set; }
    public string? OpeningHours { get; set; }
    public bool IsAvailable { get; set; }
    public bool HaveBabyChanger { get; set; }
    public bool IsRoomy { get; set; }
    public bool IsAccessible { get; set; }
    public bool IsFree { get; set; }
    public int Urinals { get; set; }
    public int Toilets { get; set; }
    public int Rating { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public double Long { get; set; }
    public double Lat { get; set; }
    public bool IsAproved { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public double? DistanceKm { get; set; }
    public string? OwnerUsername { get; set; }
    public string? Image { get; set; }
    public List<PhotoInfo> Photos { get; set; } = new();
    public int FavoritesCount { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsOwner { get; set; }

    public bool HasDistance => DistanceKm.HasValue;
    public string DistanceLabel => DistanceKm.HasValue
        ? (DistanceKm.Value < 1 ? $"{Math.Round(DistanceKm.Value * 100) * 10:0} m" : $"{DistanceKm.Value:0.0} km")
        : string.Empty;

    public bool HasReviews => ReviewCount > 0;
    public double Score => AverageRating ?? Rating;
    public string ScoreLabel => Score.ToString("0.0");
    public string ReviewCountLabel => ReviewCount == 1 ? "1 review" : $"{ReviewCount} reviews";
    public string TypeLabel => PlaceTypes.Label(Type);
    public bool HasImage => !string.IsNullOrWhiteSpace(Image);
    public bool HasAddress => !string.IsNullOrWhiteSpace(Address);
    public bool HasHours => !string.IsNullOrWhiteSpace(OpeningHours);
    public bool IsClosed => !IsAvailable;

    public string FreshnessLabel => LastVerifiedAt.HasValue
        ? $"Confirmed {DescribeAge(LastVerifiedAt.Value)}"
        : "Not confirmed yet";

    public string ToiletsLabel => Toilets == 1 ? "1 toilet" : $"{Toilets} toilets";
    public string UrinalsLabel => Urinals == 1 ? "1 urinal" : $"{Urinals} urinals";

    internal static string DescribeAge(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        if (span.TotalHours < 1) return "just now";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 2) return "yesterday";
        if (span.TotalDays < 60) return $"{(int)span.TotalDays} days ago";
        return $"on {utc.ToLocalTime():MMM d, yyyy}";
    }
}

public class PhotoInfo
{
    public string? Id { get; set; }
    public string? Url { get; set; }
    public bool IsMain { get; set; }
}

public class Review
{
    public string Id { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public int Rating { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? PlaceId { get; set; }
    public string? PlaceName { get; set; }
    public List<PhotoInfo> Photos { get; set; } = new();
    public bool IsMine { get; set; }

    public bool IsSomeoneElses => !IsMine;
    public string Byline => $"{DisplayName} · {Place.DescribeAge(CreatedAt)}";
    public string? FirstPhoto => Photos.FirstOrDefault()?.Url;
    public bool HasPhoto => !string.IsNullOrWhiteSpace(FirstPhoto);
}

public class Profile
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Image { get; set; }
    public int PlacesCount { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class FavoriteState
{
    public bool IsFavorite { get; set; }
}

public class ApiError
{
    public string? Message { get; set; }
}

public static class PlaceTypes
{
    public static readonly IReadOnlyList<string> All = new[] { "Unisex", "Family", "Women", "Men", "Accessible" };

    public static string Label(string? type) => type switch
    {
        "Men" => "Men",
        "Women" => "Women",
        "Family" => "Family",
        "Accessible" => "Accessible",
        _ => "Unisex"
    };
}
