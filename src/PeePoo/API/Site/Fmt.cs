using System;
using System.Globalization;

namespace API.Site
{
    /// <summary>Spanish formatting helpers for the public pages.</summary>
    public static class Fmt
    {
        public static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-DO");

        public static string TypeName(string type) => type switch
        {
            "Men" => "Hombres",
            "Women" => "Mujeres",
            "Family" => "Familiar",
            "Accessible" => "Accesible",
            _ => "Mixto"
        };

        public static string Stars(double rating)
        {
            var full = (int)Math.Round(Math.Clamp(rating, 0, 5), MidpointRounding.AwayFromZero);
            return new string('★', full) + new string('☆', 5 - full);
        }

        public static string Rating(double rating) => rating.ToString("0.0", Es);

        public static string Distance(double? km) => km switch
        {
            null => null,
            < 1 => $"{Math.Round(km.Value * 1000 / 10) * 10:0} m",
            _ => $"{km.Value.ToString("0.0", Es)} km"
        };

        public static string Ago(DateTime utc)
        {
            var span = DateTime.UtcNow - utc;
            if (span.TotalMinutes < 60) return "hace un momento";
            if (span.TotalHours < 24) return $"hace {(int)span.TotalHours} h";
            if (span.TotalDays < 2) return "ayer";
            if (span.TotalDays < 30) return $"hace {(int)span.TotalDays} días";
            if (span.TotalDays < 60) return "hace un mes";
            if (span.TotalDays < 365) return $"hace {(int)(span.TotalDays / 30)} meses";
            return utc.ToString("MMMM 'de' yyyy", Es);
        }

        public static string Date(DateTime date) => date.ToString("d 'de' MMMM, yyyy", Es);

        public static string Count(int n, string one, string many) => n == 1 ? $"1 {one}" : $"{n.ToString("N0", Es)} {many}";
    }
}
