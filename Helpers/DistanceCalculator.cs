using PoligonMaui.Models;

namespace PoligonMaui.Helpers;

public static class DistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;
    private const double EarthRadiusM = 6371000.0;

    public static double CalculateDistanceMeters(GpsPosition from, GpsPosition to)
    {
        return CalculateDistance(from.Latitude, from.Longitude, to.Latitude, to.Longitude, DistanceUnit.Meters);
    }

    public static double CalculateDistanceKilometers(GpsPosition from, GpsPosition to)
    {
        return CalculateDistance(from.Latitude, from.Longitude, to.Latitude, to.Longitude, DistanceUnit.Kilometers);
    }

    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit)
    {
        // Haversine formula
        var lat1Rad = DegreesToRadians(lat1);
        var lat2Rad = DegreesToRadians(lat2);
        var deltaLatRad = DegreesToRadians(lat2 - lat1);
        var deltaLonRad = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return unit switch
        {
            DistanceUnit.Meters => EarthRadiusM * c,
            DistanceUnit.Kilometers => EarthRadiusKm * c,
            _ => EarthRadiusM * c
        };
    }

    public static double CalculateBearing(GpsPosition from, GpsPosition to)
    {
        var lat1Rad = DegreesToRadians(from.Latitude);
        var lat2Rad = DegreesToRadians(to.Latitude);
        var deltaLonRad = DegreesToRadians(to.Longitude - from.Longitude);

        var y = Math.Sin(deltaLonRad) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) - Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(deltaLonRad);

        var bearing = RadiansToDegrees(Math.Atan2(y, x));
        return (bearing + 360) % 360; // Normalize to 0-360
    }

    public static bool IsWithinRadius(GpsPosition center, GpsPosition point, double radiusMeters)
    {
        var distance = CalculateDistanceMeters(center, point);
        return distance <= radiusMeters;
    }

    public static GpsPosition CalculateDestinationPoint(GpsPosition origin, double bearingDegrees, double distanceMeters)
    {
        var bearingRad = DegreesToRadians(bearingDegrees);
        var distanceRad = distanceMeters / EarthRadiusM;

        var lat1Rad = DegreesToRadians(origin.Latitude);
        var lon1Rad = DegreesToRadians(origin.Longitude);

        var lat2Rad = Math.Asin(Math.Sin(lat1Rad) * Math.Cos(distanceRad) +
                               Math.Cos(lat1Rad) * Math.Sin(distanceRad) * Math.Cos(bearingRad));

        var lon2Rad = lon1Rad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(distanceRad) * Math.Cos(lat1Rad),
                                          Math.Cos(distanceRad) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad));

        return new GpsPosition(RadiansToDegrees(lat2Rad), RadiansToDegrees(lon2Rad));
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    private static double RadiansToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}

public enum DistanceUnit
{
    Meters,
    Kilometers
}