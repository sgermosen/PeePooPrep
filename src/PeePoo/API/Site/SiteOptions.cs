namespace API.Site
{
    /// <summary>Public website settings (section "Site"). Store URLs stay empty until the apps are live.</summary>
    public class SiteOptions
    {
        public string AppName { get; set; } = "PeePoo Finder";
        /// <summary>Absolute origin used for canonical links, sitemap and social cards, e.g. https://peepoo.app.</summary>
        public string BaseUrl { get; set; }
        public string SupportEmail { get; set; } = "support@peepoo.app";
        public string PlayStoreUrl { get; set; }
        public string AppStoreUrl { get; set; }
        public string LegalUpdated { get; set; } = "2026";
    }
}
