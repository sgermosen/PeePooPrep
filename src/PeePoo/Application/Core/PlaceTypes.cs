using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Core
{
    public static class PlaceTypes
    {
        public const string Accessible = "Accessible";

        public static readonly IReadOnlyList<string> All = new[] { "Unisex", "Men", "Women", "Family", Accessible };

        /// <summary>Returns the canonical spelling of a known type, or null.</summary>
        public static string Normalize(string type) =>
            All.FirstOrDefault(t => string.Equals(t, type?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
