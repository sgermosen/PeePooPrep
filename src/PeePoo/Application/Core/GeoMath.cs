using System;

namespace Application.Core
{
    public static class GeoMath
    {
        public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371d;
            var dLat = (lat2 - lat1) * Math.PI / 180d;
            var dLon = (lon2 - lon1) * Math.PI / 180d;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
