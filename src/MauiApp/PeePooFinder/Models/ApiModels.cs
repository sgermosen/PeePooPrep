using System.Collections.Generic;

namespace PeePooFinder.Models;

public class AuthResponse
{
    public string? DisplayName { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }
    public string? Image { get; set; }
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
    public bool IsAvailable { get; set; }
    public bool HaveBabyChanger { get; set; }
    public bool IsRoomy { get; set; }
    public int Urinals { get; set; }
    public int Toilets { get; set; }
    public int Rating { get; set; }
    public double Long { get; set; }
    public double Lat { get; set; }
    public bool IsAproved { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public double? DistanceKm { get; set; }
    public string? OwnerUsername { get; set; }
    public string? Image { get; set; }
    public List<FavoriteInfo> Favorites { get; set; } = new();

    public bool HasDistance => DistanceKm.HasValue;
    public string DistanceLabel => DistanceKm.HasValue
        ? (DistanceKm.Value < 1 ? $"{DistanceKm.Value * 1000:0} m away" : $"{DistanceKm.Value:0.0} km away")
        : string.Empty;
    public string FreshnessLabel => LastVerifiedAt.HasValue
        ? $"Verified {DescribeAge(LastVerifiedAt.Value)}"
        : "Not verified yet";

    private static string DescribeAge(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        if (span.TotalHours < 1) return "just now";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return $"{(int)span.TotalDays}d ago";
    }
    public List<PhotoInfo> Photos { get; set; } = new();
    public List<Review> Visits { get; set; } = new();
}

public class FavoriteInfo
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Image { get; set; }
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
    public string? Image { get; set; }
    public string? PlaceId { get; set; }
}

public class Profile
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Image { get; set; }
    public List<PhotoInfo> Photos { get; set; } = new();
}

public class ApiError
{
    public string? Message { get; set; }
}
