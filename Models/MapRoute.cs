namespace PoligonMaui.Models;

public class MapRoute
{
    public List<GpsPosition> RoutePoints { get; set; } = new List<GpsPosition>();
    
    public GpsPosition StartPoint { get; set; } = new GpsPosition();
    
    public GpsPosition EndPoint { get; set; } = new GpsPosition();
    
    public double TotalDistance { get; set; } // in meters
    
    public double RemainingDistance { get; set; } // in meters
    
    public TimeSpan EstimatedTime { get; set; }
    
    public bool IsActive { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }

    public MapRoute()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public MapRoute(GpsPosition start, GpsPosition end)
    {
        StartPoint = start;
        EndPoint = end;
        CreatedAt = DateTime.UtcNow;
        RoutePoints.Add(start);
        RoutePoints.Add(end);
    }

    public void AddRoutePoint(GpsPosition point)
    {
        RoutePoints.Add(point);
    }

    public void ClearRoute()
    {
        RoutePoints.Clear();
        TotalDistance = 0;
        RemainingDistance = 0;
        IsActive = false;
    }
}