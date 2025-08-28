using PoligonMaui.Models;
using PoligonMaui.Services.Interfaces;

namespace PoligonMaui.Services;

public class NmeaParserService : INmeaParserService
{
    public GpsPosition? ParseGprmc(string nmeaString)
    {
        if (string.IsNullOrEmpty(nmeaString) || !nmeaString.StartsWith("$GPRMC"))
            return null;

        if (!ValidateChecksum(nmeaString))
            return null;

        try
        {
            var parts = nmeaString.Split(',');
            if (parts.Length < 12)
                return null;

            // Check if data is valid
            if (parts[2] != "A") // A = valid, V = invalid
                return null;

            var position = new GpsPosition();

            // Parse latitude
            if (!string.IsNullOrEmpty(parts[3]) && !string.IsNullOrEmpty(parts[4]))
            {
                position.Latitude = ConvertDegreeMinutesToDecimal(parts[3], parts[4]);
            }

            // Parse longitude
            if (!string.IsNullOrEmpty(parts[5]) && !string.IsNullOrEmpty(parts[6]))
            {
                position.Longitude = ConvertDegreeMinutesToDecimal(parts[5], parts[6]);
            }

            // Parse speed (knots to km/h)
            if (!string.IsNullOrEmpty(parts[7]))
            {
                if (double.TryParse(parts[7], out double speedKnots))
                {
                    position.Speed = speedKnots * 1.852; // Convert knots to km/h
                }
            }

            // Parse course
            if (!string.IsNullOrEmpty(parts[8]))
            {
                if (double.TryParse(parts[8], out double course))
                {
                    position.Course = course;
                }
            }

            // Parse date and time
            if (!string.IsNullOrEmpty(parts[1]) && !string.IsNullOrEmpty(parts[9]))
            {
                position.Timestamp = ParseDateTime(parts[1], parts[9]);
            }

            position.IsValid = true;
            return position;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing NMEA: {ex.Message}");
            return null;
        }
    }

    public bool ValidateChecksum(string nmeaString)
    {
        if (string.IsNullOrEmpty(nmeaString))
            return false;

        var checksumIndex = nmeaString.LastIndexOf('*');
        if (checksumIndex == -1)
            return false;

        var sentence = nmeaString.Substring(1, checksumIndex - 1); // Remove $ and *checksum
        var providedChecksum = nmeaString.Substring(checksumIndex + 1);

        if (providedChecksum.Length != 2)
            return false;

        byte calculatedChecksum = 0;
        foreach (char c in sentence)
        {
            calculatedChecksum ^= (byte)c;
        }

        return calculatedChecksum.ToString("X2") == providedChecksum.ToUpper();
    }

    public double ConvertDegreeMinutesToDecimal(string degreeMinutes, string hemisphere)
    {
        if (string.IsNullOrEmpty(degreeMinutes) || degreeMinutes.Length < 4)
            return 0;

        try
        {
            // For latitude: DDMM.MMMM (DD = degrees, MM.MMMM = minutes)
            // For longitude: DDDMM.MMMM (DDD = degrees, MM.MMMM = minutes)
            
            int degreesLength = degreeMinutes.Length == 9 ? 3 : 2; // Longitude has 3 digits for degrees
            
            var degreesStr = degreeMinutes.Substring(0, degreesLength);
            var minutesStr = degreeMinutes.Substring(degreesLength);

            if (!int.TryParse(degreesStr, out int degrees))
                return 0;

            if (!double.TryParse(minutesStr, out double minutes))
                return 0;

            double decimal_degrees = degrees + (minutes / 60.0);

            // Apply hemisphere
            if (hemisphere == "S" || hemisphere == "W")
                decimal_degrees = -decimal_degrees;

            return decimal_degrees;
        }
        catch
        {
            return 0;
        }
    }

    private DateTime ParseDateTime(string timeStr, string dateStr)
    {
        try
        {
            // Time format: HHMMSS.SSS
            // Date format: DDMMYY
            
            if (timeStr.Length < 6 || dateStr.Length != 6)
                return DateTime.UtcNow;

            var hours = int.Parse(timeStr.Substring(0, 2));
            var minutes = int.Parse(timeStr.Substring(2, 2));
            var seconds = int.Parse(timeStr.Substring(4, 2));

            var day = int.Parse(dateStr.Substring(0, 2));
            var month = int.Parse(dateStr.Substring(2, 2));
            var year = 2000 + int.Parse(dateStr.Substring(4, 2)); // Assuming 20xx

            return new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc);
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }
}