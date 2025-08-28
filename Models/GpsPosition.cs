namespace PoligonMaui.Models;

public class GpsPosition
{
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public double Altitude { get; set; }
    
    public double Speed { get; set; }
    
    public double Course { get; set; } // Direction of movement in degrees
    
    public DateTime Timestamp { get; set; }
    
    public double Accuracy { get; set; }
    
    public bool IsValid { get; set; } = true;

    public GpsPosition()
    {
        Timestamp = DateTime.UtcNow;
    }

    public GpsPosition(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Timestamp = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"Lat: {Latitude:F6}, Lon: {Longitude:F6}, Speed: {Speed:F2}, Course: {Course:F1}°";
    }
}