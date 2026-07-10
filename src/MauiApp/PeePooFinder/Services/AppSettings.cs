namespace PeePooFinder.Services;

public class AppSettings
{
    public ApiSettings Api { get; set; } = new();
    public GoogleMapsSettings GoogleMaps { get; set; } = new();
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = "https://peepoo.azurewebsites.net";
}

public class GoogleMapsSettings
{
    public string ApiKey { get; set; } = string.Empty;
}
