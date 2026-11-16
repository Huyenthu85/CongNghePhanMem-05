namespace ThuyetMinh.Api.Services;

public static class GeoFencePolicy
{
    private const double EarthRadiusMeters = 6371000;

    public static bool IsAllowed(double shopLat, double shopLon, double radiusMeters,
        double? latitude, double? longitude, double? accuracyMeters,
        DateTimeOffset? capturedAt, DateTimeOffset now)
    {
        if (!latitude.HasValue || !longitude.HasValue ||
            !accuracyMeters.HasValue || !capturedAt.HasValue)
            return false;

        double lat = latitude.Value;
        double lon = longitude.Value;
        double accuracy = accuracyMeters.Value;

        if (!double.IsFinite(shopLat) || !double.IsFinite(shopLon) ||
            !double.IsFinite(lat) || !double.IsFinite(lon) ||
            !double.IsFinite(radiusMeters) || !double.IsFinite(accuracy) ||
            Math.Abs(shopLat) > 90 || Math.Abs(shopLon) > 180 ||
            Math.Abs(lat) > 90 || Math.Abs(lon) > 180 ||
            radiusMeters <= 0 || accuracy <= 0 ||
            accuracy > Math.Max(100, radiusMeters) ||
            capturedAt.Value < now.AddSeconds(-30) ||
            capturedAt.Value > now.AddSeconds(10))
            return false;

        static double Rad(double degrees) => degrees * Math.PI / 180;

        double latDelta = Rad(lat - shopLat);
        double lonDelta = Rad(lon - shopLon);

        double a = Math.Pow(Math.Sin(latDelta / 2), 2) +
            Math.Cos(Rad(shopLat)) * Math.Cos(Rad(lat)) *
            Math.Pow(Math.Sin(lonDelta / 2), 2);

        double distance = 2 * EarthRadiusMeters *
            Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));

        return distance <= radiusMeters;
    }
}