using PoligonMaui.Models;

namespace PoligonMaui.Services.Interfaces;

public interface IMapService
{
    Task<bool> LoadOfflineMapAsync(string mbTilesPath);
    
    Task AddTargetAsync(Target target);
    Task RemoveTargetAsync(Target target);
    Task UpdateTargetAsync(Target target);
    
    Task<Target?> GetNearestTargetAsync(GpsPosition currentPosition);
    Task<double> CalculateDistanceAsync(GpsPosition from, GpsPosition to);
    
    Task<MapRoute> CalculateRouteAsync(GpsPosition from, GpsPosition to);
    Task ClearRouteAsync();
    
    Task SetMapRotationAsync(double rotation);
    Task SetMapCenterAsync(double latitude, double longitude);
    Task SetZoomLevelAsync(double zoomLevel);
    
    bool AutoFollowEnabled { get; set; }
    bool RotateWithMovement { get; set; }
    
    event EventHandler<Target>? TargetReached;
    event EventHandler<double>? DistanceToTargetChanged;
    event EventHandler<Target>? NearestTargetChanged;
}