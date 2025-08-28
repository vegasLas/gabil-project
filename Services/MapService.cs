using PoligonMaui.Models;
using PoligonMaui.Services.Interfaces;
using PoligonMaui.Helpers;

namespace PoligonMaui.Services;

public class MapService : IMapService
{
    private readonly List<Target> _mapTargets = new List<Target>();
    private MapRoute? _currentRoute;
    private Target? _currentNearestTarget;
    
    public bool AutoFollowEnabled { get; set; } = true;
    public bool RotateWithMovement { get; set; } = false;

    public event EventHandler<Target>? TargetReached;
    public event EventHandler<double>? DistanceToTargetChanged;
    public event EventHandler<Target>? NearestTargetChanged;

    public async Task<bool> LoadOfflineMapAsync(string mbTilesPath)
    {
        try
        {
            if (!File.Exists(mbTilesPath))
            {
                System.Diagnostics.Debug.WriteLine($"MBTiles file not found: {mbTilesPath}");
                return false;
            }

            // This would be implemented with Mapsui and BruTile
            // For now, just validate the file exists
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading offline map: {ex.Message}");
            return false;
        }
    }

    public async Task AddTargetAsync(Target target)
    {
        _mapTargets.Add(target);
        await Task.CompletedTask;
    }

    public async Task RemoveTargetAsync(Target target)
    {
        _mapTargets.Remove(target);
        await Task.CompletedTask;
    }

    public async Task UpdateTargetAsync(Target target)
    {
        var existingTarget = _mapTargets.FirstOrDefault(t => t.Id == target.Id);
        if (existingTarget != null)
        {
            var index = _mapTargets.IndexOf(existingTarget);
            _mapTargets[index] = target;
        }
        await Task.CompletedTask;
    }

    public async Task<Target?> GetNearestTargetAsync(GpsPosition currentPosition)
    {
        if (!_mapTargets.Any() || currentPosition == null)
            return null;

        Target? nearestTarget = null;
        double shortestDistance = double.MaxValue;

        foreach (var target in _mapTargets.Where(t => !t.IsReached))
        {
            var targetPosition = new GpsPosition(target.Latitude, target.Longitude);
            var distance = DistanceCalculator.CalculateDistanceMeters(currentPosition, targetPosition);
            
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = target;
            }
        }

        // Check if nearest target changed
        if (nearestTarget != _currentNearestTarget)
        {
            _currentNearestTarget = nearestTarget;
            if (nearestTarget != null)
            {
                NearestTargetChanged?.Invoke(this, nearestTarget);
            }
        }

        // Check if we've reached the nearest target (within 300 meters)
        if (nearestTarget != null && shortestDistance <= 300)
        {
            nearestTarget.IsReached = true;
            TargetReached?.Invoke(this, nearestTarget);
            
            // Update the target in the list
            var existingTarget = _mapTargets.FirstOrDefault(t => t.Id == nearestTarget.Id);
            if (existingTarget != null)
            {
                existingTarget.IsReached = true;
            }
        }

        DistanceToTargetChanged?.Invoke(this, shortestDistance);
        await Task.CompletedTask;
        return nearestTarget;
    }

    public async Task<double> CalculateDistanceAsync(GpsPosition from, GpsPosition to)
    {
        await Task.CompletedTask;
        return DistanceCalculator.CalculateDistanceMeters(from, to);
    }

    public async Task<MapRoute> CalculateRouteAsync(GpsPosition from, GpsPosition to)
    {
        var route = new MapRoute(from, to)
        {
            IsActive = true
        };

        // Calculate total distance
        route.TotalDistance = await CalculateDistanceAsync(from, to);
        route.RemainingDistance = route.TotalDistance;

        // For simple implementation, create a straight line route
        // In a real implementation, this could use routing algorithms
        route.RoutePoints.Clear();
        route.RoutePoints.Add(from);
        route.RoutePoints.Add(to);

        _currentRoute = route;
        return route;
    }

    public async Task ClearRouteAsync()
    {
        _currentRoute?.ClearRoute();
        _currentRoute = null;
        await Task.CompletedTask;
    }

    public async Task SetMapRotationAsync(double rotation)
    {
        // This would be implemented with the actual map control
        await Task.CompletedTask;
    }

    public async Task SetMapCenterAsync(double latitude, double longitude)
    {
        // This would be implemented with the actual map control
        await Task.CompletedTask;
    }

    public async Task SetZoomLevelAsync(double zoomLevel)
    {
        // This would be implemented with the actual map control
        await Task.CompletedTask;
    }
}