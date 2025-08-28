using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoligonMaui.Services.Interfaces;
using PoligonMaui.Models;
using PoligonMaui.Constants;

namespace PoligonMaui.ViewModels;

public partial class ControlPanelViewModel : BaseViewModel
{
    private readonly IGpsService _gpsService;
    private readonly IMapService _mapService;
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private string gpsStatusText = "Disconnected";

    [ObservableProperty]
    private Color gpsStatusColor = Colors.Gray;

    [ObservableProperty]
    private bool isComGpsActive = false;

    [ObservableProperty]
    private bool isSimulationActive = false;

    [ObservableProperty]
    private bool autoFollowEnabled = true;

    [ObservableProperty]
    private bool rotateWithMovement = false;

    [ObservableProperty]
    private string currentPosition = "No GPS Data";

    [ObservableProperty]
    private double distanceToNearestTarget = 0;

    [ObservableProperty]
    private string nearestTargetName = "None";

    [ObservableProperty]
    private double simulationSpeed = AppConstants.DefaultSimulationSpeedKmh;

    [ObservableProperty]
    private int simulationInterval = AppConstants.DefaultSimulationIntervalMs;

    [ObservableProperty]
    private string comPortName = "COM3";

    public ControlPanelViewModel(IGpsService gpsService, IMapService mapService, IDatabaseService databaseService)
    {
        _gpsService = gpsService;
        _mapService = mapService;
        _databaseService = databaseService;

        Title = "Control Panel";

        // Subscribe to events
        _gpsService.PositionChanged += OnPositionChanged;
        _gpsService.StatusChanged += OnGpsStatusChanged;
        _mapService.DistanceToTargetChanged += OnDistanceToTargetChanged;
        _mapService.TargetReached += OnTargetReached;

        // Initialize status
        UpdateGpsStatus(_gpsService.CurrentStatus);
    }

    public override async Task InitializeAsync()
    {
        IsBusy = true;
        
        try
        {
            // Update initial state
            IsComGpsActive = _gpsService.IsComPortActive;
            IsSimulationActive = _gpsService.IsSimulationActive;
            AutoFollowEnabled = _mapService.AutoFollowEnabled;
            RotateWithMovement = _mapService.RotateWithMovement;
            
            StatusMessage = "Control panel ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Control panel initialization error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartComGpsAsync()
    {
        IsBusy = true;
        try
        {
            var success = await _gpsService.StartComPortAsync(ComPortName);
            if (!success)
            {
                StatusMessage = $"Failed to connect to COM port {ComPortName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"COM GPS error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopComGpsAsync()
    {
        IsBusy = true;
        try
        {
            await _gpsService.StopComPortAsync();
            StatusMessage = "COM GPS stopped";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Stop COM GPS error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartSimulationAsync()
    {
        IsBusy = true;
        try
        {
            await _gpsService.SetSimulationSpeedAsync(SimulationSpeed);
            await _gpsService.SetSimulationIntervalAsync(SimulationInterval);
            
            var success = await _gpsService.StartSimulationAsync();
            if (!success)
            {
                StatusMessage = "Failed to start GPS simulation";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Simulation error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopSimulationAsync()
    {
        IsBusy = true;
        try
        {
            await _gpsService.StopSimulationAsync();
            StatusMessage = "GPS simulation stopped";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Stop simulation error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleAutoFollowAsync()
    {
        AutoFollowEnabled = !AutoFollowEnabled;
        _mapService.AutoFollowEnabled = AutoFollowEnabled;
        
        StatusMessage = AutoFollowEnabled ? "Auto-follow GPS enabled" : "Manual control enabled";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ToggleMapRotationAsync()
    {
        RotateWithMovement = !RotateWithMovement;
        _mapService.RotateWithMovement = RotateWithMovement;
        
        StatusMessage = RotateWithMovement ? "Map rotates with movement" : "Fixed north orientation";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ResetAllTargetsAsync()
    {
        IsBusy = true;
        try
        {
            await _databaseService.ResetAllTargetsAsync();
            StatusMessage = "All targets reset to 'not reached' state";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Reset targets error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateSimulationSpeedAsync()
    {
        try
        {
            if (_gpsService.IsSimulationActive)
            {
                await _gpsService.SetSimulationSpeedAsync(SimulationSpeed);
                StatusMessage = $"Simulation speed updated to {SimulationSpeed:F1} km/h";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Update speed error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateSimulationIntervalAsync()
    {
        try
        {
            if (_gpsService.IsSimulationActive)
            {
                await _gpsService.SetSimulationIntervalAsync(SimulationInterval);
                StatusMessage = $"Simulation interval updated to {SimulationInterval}ms";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Update interval error: {ex.Message}";
        }
    }

    private void OnPositionChanged(object? sender, GpsPosition position)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentPosition = $"Lat: {position.Latitude:F6}, Lon: {position.Longitude:F6}\n" +
                            $"Speed: {position.Speed:F1} km/h, Course: {position.Course:F0}°";
        });
    }

    private void OnGpsStatusChanged(object? sender, string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateGpsStatus(status);
            IsComGpsActive = _gpsService.IsComPortActive;
            IsSimulationActive = _gpsService.IsSimulationActive;
        });
    }

    private void OnDistanceToTargetChanged(object? sender, double distance)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DistanceToNearestTarget = distance;
        });
    }

    private void OnTargetReached(object? sender, Target target)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"🎯 Target '{target.Name}' reached!";
        });
    }

    private void UpdateGpsStatus(string status)
    {
        GpsStatusText = status;
        
        GpsStatusColor = status.ToLower() switch
        {
            var s when s.Contains("com gps") && s.Contains("connected") => Color.FromArgb(AppConstants.GpsStatusColor),
            var s when s.Contains("simulation") => Color.FromArgb(AppConstants.SimulationStatusColor),
            var s when s.Contains("error") => Color.FromArgb(AppConstants.ErrorStatusColor),
            _ => Color.FromArgb(AppConstants.DisconnectedStatusColor)
        };
    }
}