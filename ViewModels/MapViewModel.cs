using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PoligonMaui.Models;
using PoligonMaui.Services.Interfaces;
using PoligonMaui.Constants;

namespace PoligonMaui.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    private readonly IGpsService _gpsService;
    private readonly IMapService _mapService;
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Target> targets = new();

    [ObservableProperty]
    private ObservableCollection<TargetGroup> targetGroups = new();

    [ObservableProperty]
    private GpsPosition? currentPosition;

    [ObservableProperty]
    private Target? nearestTarget;

    [ObservableProperty]
    private double distanceToTarget = 0;

    [ObservableProperty]
    private MapRoute? currentRoute;

    [ObservableProperty]
    private bool autoFollowEnabled = true;

    [ObservableProperty]
    private bool rotateWithMovement = false;

    [ObservableProperty]
    private double mapZoom = AppConstants.DefaultZoomLevel;

    [ObservableProperty]
    private double mapCenterLatitude = AppConstants.DefaultCenterLatitude;

    [ObservableProperty]
    private double mapCenterLongitude = AppConstants.DefaultCenterLongitude;

    [ObservableProperty]
    private string mapStatusMessage = "Map ready";

    [ObservableProperty]
    private bool isMapLoaded = false;

    public MapViewModel(IGpsService gpsService, IMapService mapService, IDatabaseService databaseService)
    {
        _gpsService = gpsService;
        _mapService = mapService;
        _databaseService = databaseService;

        Title = "Map View";

        // Subscribe to GPS events
        _gpsService.PositionChanged += OnPositionChanged;

        // Subscribe to map events  
        _mapService.TargetReached += OnTargetReached;
        _mapService.DistanceToTargetChanged += OnDistanceChanged;
        _mapService.NearestTargetChanged += OnNearestTargetChanged;

        // Set map service properties
        _mapService.AutoFollowEnabled = AutoFollowEnabled;
        _mapService.RotateWithMovement = RotateWithMovement;
    }

    public override async Task InitializeAsync()
    {
        IsBusy = true;
        
        try
        {
            await LoadMapAsync();
            await LoadTargetsAsync();
            MapStatusMessage = "Map and targets loaded successfully";
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Map initialization error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadMapAsync()
    {
        try
        {
            var mapPath = Path.Combine(FileSystem.AppDataDirectory, AppConstants.OfflineMapFileName);
            IsMapLoaded = await _mapService.LoadOfflineMapAsync(mapPath);
            
            if (!IsMapLoaded)
            {
                MapStatusMessage = "Offline map not found. Using default view.";
            }
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Map loading error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadTargetsAsync()
    {
        try
        {
            var allTargets = await _databaseService.GetAllTargetsAsync();
            var allGroups = await _databaseService.GetAllTargetGroupsAsync();

            Targets.Clear();
            TargetGroups.Clear();

            foreach (var target in allTargets)
            {
                Targets.Add(target);
                await _mapService.AddTargetAsync(target);
            }

            foreach (var group in allGroups)
            {
                TargetGroups.Add(group);
            }
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Target loading error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddTargetAsync(object parameter)
    {
        try
        {
            if (parameter is (double lat, double lon))
            {
                var target = new Target
                {
                    Latitude = lat,
                    Longitude = lon,
                    Color = AppConstants.DefaultTargetColor,
                    Name = $"Target {Targets.Count + 1}",
                    CreatedAt = DateTime.UtcNow
                };

                await _databaseService.SaveTargetAsync(target);
                await _mapService.AddTargetAsync(target);
                Targets.Add(target);

                MapStatusMessage = $"Target added at {lat:F6}, {lon:F6}";
            }
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Add target error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RemoveTargetAsync(Target target)
    {
        try
        {
            await _databaseService.DeleteTargetAsync(target);
            await _mapService.RemoveTargetAsync(target);
            Targets.Remove(target);

            MapStatusMessage = $"Target {target.Name} removed";
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Remove target error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateTargetGroupAsync()
    {
        try
        {
            if (CurrentPosition == null) return;

            var group = new TargetGroup
            {
                Name = $"Group {TargetGroups.Count + 1}",
                CenterLatitude = CurrentPosition.Latitude,
                CenterLongitude = CurrentPosition.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveTargetGroupAsync(group);
            TargetGroups.Add(group);

            MapStatusMessage = $"Target group created at current position";
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Create group error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleAutoFollowAsync()
    {
        AutoFollowEnabled = !AutoFollowEnabled;
        _mapService.AutoFollowEnabled = AutoFollowEnabled;
        
        MapStatusMessage = AutoFollowEnabled ? "Auto-follow enabled" : "Auto-follow disabled";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ToggleRotationModeAsync()
    {
        RotateWithMovement = !RotateWithMovement;
        _mapService.RotateWithMovement = RotateWithMovement;
        
        MapStatusMessage = RotateWithMovement ? "Map rotates with movement" : "Map fixed to north";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ZoomInAsync()
    {
        if (MapZoom < AppConstants.MaxZoomLevel)
        {
            MapZoom += 1;
            await _mapService.SetZoomLevelAsync(MapZoom);
        }
    }

    [RelayCommand]
    private async Task ZoomOutAsync()
    {
        if (MapZoom > AppConstants.MinZoomLevel)
        {
            MapZoom -= 1;
            await _mapService.SetZoomLevelAsync(MapZoom);
        }
    }

    [RelayCommand]
    private async Task CenterOnCurrentPositionAsync()
    {
        if (CurrentPosition != null)
        {
            MapCenterLatitude = CurrentPosition.Latitude;
            MapCenterLongitude = CurrentPosition.Longitude;
            await _mapService.SetMapCenterAsync(MapCenterLatitude, MapCenterLongitude);
        }
    }

    [RelayCommand]
    private async Task ResetAllTargetsAsync()
    {
        try
        {
            await _databaseService.ResetAllTargetsAsync();
            
            // Update local collection
            foreach (var target in Targets)
            {
                target.IsReached = false;
                await _mapService.UpdateTargetAsync(target);
            }

            MapStatusMessage = "All targets reset to 'not reached' state";
        }
        catch (Exception ex)
        {
            MapStatusMessage = $"Reset targets error: {ex.Message}";
        }
    }

    private void OnPositionChanged(object? sender, GpsPosition position)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            CurrentPosition = position;
            
            if (AutoFollowEnabled)
            {
                MapCenterLatitude = position.Latitude;
                MapCenterLongitude = position.Longitude;
                await _mapService.SetMapCenterAsync(position.Latitude, position.Longitude);
            }

            if (RotateWithMovement && position.Course > 0)
            {
                await _mapService.SetMapRotationAsync(position.Course);
            }

            // Update nearest target and calculate route
            var nearest = await _mapService.GetNearestTargetAsync(position);
            if (nearest != null && nearest != NearestTarget)
            {
                NearestTarget = nearest;
                
                // Calculate and update route to nearest target
                var targetPosition = new GpsPosition(nearest.Latitude, nearest.Longitude);
                CurrentRoute = await _mapService.CalculateRouteAsync(position, targetPosition);
            }
        });
    }

    private void OnTargetReached(object? sender, Target target)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MapStatusMessage = $"Target '{target.Name}' reached!";
            
            // Update target in collection
            var existingTarget = Targets.FirstOrDefault(t => t.Id == target.Id);
            if (existingTarget != null)
            {
                existingTarget.IsReached = true;
            }
        });
    }

    private void OnDistanceChanged(object? sender, double distance)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DistanceToTarget = distance;
        });
    }

    private void OnNearestTargetChanged(object? sender, Target target)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            NearestTarget = target;
            
            // Calculate route to new nearest target
            if (CurrentPosition != null && target != null)
            {
                var targetPosition = new GpsPosition(target.Latitude, target.Longitude);
                CurrentRoute = await _mapService.CalculateRouteAsync(CurrentPosition, targetPosition);
            }
            else
            {
                CurrentRoute = null;
            }
        });
    }
}