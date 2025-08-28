using PoligonMaui.Models;

namespace PoligonMaui.Helpers;

public static class CoordinateConverter
{
    public static string FormatCoordinates(GpsPosition position, CoordinateFormat format = CoordinateFormat.DecimalDegrees)
    {
        return format switch
        {
            CoordinateFormat.DecimalDegrees => FormatDecimalDegrees(position),
            CoordinateFormat.DegreesMinutes => FormatDegreesMinutes(position),
            CoordinateFormat.DegreesMinutesSeconds => FormatDegreesMinutesSeconds(position),
            _ => FormatDecimalDegrees(position)
        };
    }

    public static string FormatLatitude(double latitude, CoordinateFormat format = CoordinateFormat.DecimalDegrees)
    {
        return format switch
        {
            CoordinateFormat.DecimalDegrees => $"{latitude:F6}°",
            CoordinateFormat.DegreesMinutes => FormatDegreesMinutes(latitude, true),
            CoordinateFormat.DegreesMinutesSeconds => FormatDegreesMinutesSeconds(latitude, true),
            _ => $"{latitude:F6}°"
        };
    }

    public static string FormatLongitude(double longitude, CoordinateFormat format = CoordinateFormat.DecimalDegrees)
    {
        return format switch
        {
            CoordinateFormat.DecimalDegrees => $"{longitude:F6}°",
            CoordinateFormat.DegreesMinutes => FormatDegreesMinutes(longitude, false),
            CoordinateFormat.DegreesMinutesSeconds => FormatDegreesMinutesSeconds(longitude, false),
            _ => $"{longitude:F6}°"
        };
    }

    private static string FormatDecimalDegrees(GpsPosition position)
    {
        return $"{position.Latitude:F6}°, {position.Longitude:F6}°";
    }

    private static string FormatDegreesMinutes(GpsPosition position)
    {
        var latStr = FormatDegreesMinutes(position.Latitude, true);
        var lonStr = FormatDegreesMinutes(position.Longitude, false);
        return $"{latStr}, {lonStr}";
    }

    private static string FormatDegreesMinutes(double coordinate, bool isLatitude)
    {
        var isNegative = coordinate < 0;
        var absCoordinate = Math.Abs(coordinate);
        
        var degrees = (int)absCoordinate;
        var minutes = (absCoordinate - degrees) * 60;
        
        var hemisphere = GetHemisphere(isNegative, isLatitude);
        return $"{degrees}° {minutes:F3}' {hemisphere}";
    }

    private static string FormatDegreesMinutesSeconds(GpsPosition position)
    {
        var latStr = FormatDegreesMinutesSeconds(position.Latitude, true);
        var lonStr = FormatDegreesMinutesSeconds(position.Longitude, false);
        return $"{latStr}, {lonStr}";
    }

    private static string FormatDegreesMinutesSeconds(double coordinate, bool isLatitude)
    {
        var isNegative = coordinate < 0;
        var absCoordinate = Math.Abs(coordinate);
        
        var degrees = (int)absCoordinate;
        var totalMinutes = (absCoordinate - degrees) * 60;
        var minutes = (int)totalMinutes;
        var seconds = (totalMinutes - minutes) * 60;
        
        var hemisphere = GetHemisphere(isNegative, isLatitude);
        return $"{degrees}° {minutes}' {seconds:F1}\" {hemisphere}";
    }

    private static string GetHemisphere(bool isNegative, bool isLatitude)
    {
        if (isLatitude)
            return isNegative ? "S" : "N";
        else
            return isNegative ? "W" : "E";
    }

    public static (double latitude, double longitude) ParseCoordinates(string coordinateString)
    {
        // Simple parser for decimal degree format
        try
        {
            var parts = coordinateString.Replace("°", "").Split(',');
            if (parts.Length == 2)
            {
                var latitude = double.Parse(parts[0].Trim());
                var longitude = double.Parse(parts[1].Trim());
                return (latitude, longitude);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing coordinates: {ex.Message}");
        }
        
        return (0, 0);
    }

    public static bool IsValidLatitude(double latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    public static bool IsValidLongitude(double longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }

    public static bool IsValidCoordinate(double latitude, double longitude)
    {
        return IsValidLatitude(latitude) && IsValidLongitude(longitude);
    }
}

public enum CoordinateFormat
{
    DecimalDegrees,
    DegreesMinutes,
    DegreesMinutesSeconds
}