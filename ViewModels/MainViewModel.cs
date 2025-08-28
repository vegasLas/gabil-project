using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoligonMaui.Services.Interfaces;

namespace PoligonMaui.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IGpsService _gpsService;
    private readonly IMapService _mapService;
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private string currentPosition = "No GPS Data";

    [ObservableProperty]
    private string currentStatus = "Disconnected";

    [ObservableProperty]
    private bool isGpsActive = false;

    [ObservableProperty]
    private bool isSimulationActive = false;

    public MainViewModel(IGpsService gpsService, IMapService mapService, IDatabaseService databaseService)
    {
        _gpsService = gpsService;
        _mapService = mapService;
        _databaseService = databaseService;
        
        Title = "Poligon MAUI";
        
        // Subscribe to GPS events
        _gpsService.PositionChanged += OnPositionChanged;
        _gpsService.StatusChanged += OnGpsStatusChanged;
    }

    public override async Task InitializeAsync()
    {
        IsBusy = true;
        
        try
        {
            await _databaseService.InitializeDatabaseAsync();
            StatusMessage = "Application initialized successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToMapAsync()
    {
        await Shell.Current.GoToAsync("//MapPage");
    }

    [RelayCommand]
    private async Task NavigateToControlPanelAsync()
    {
        await Shell.Current.GoToAsync("//ControlPanelPage");
    }

    [RelayCommand]
    private async Task StartGpsAsync()
    {
        try
        {
            // Try to start COM port GPS (you might need to specify the port)
            var success = await _gpsService.StartComPortAsync("COM3"); // Default COM port
            if (!success)
            {
                // Fallback to simulation
                await _gpsService.StartSimulationAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"GPS start error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StopGpsAsync()
    {
        try
        {
            await _gpsService.StopComPortAsync();
            await _gpsService.StopSimulationAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"GPS stop error: {ex.Message}";
        }
    }

    private void OnPositionChanged(object? sender, Models.GpsPosition position)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentPosition = $"Lat: {position.Latitude:F6}, Lon: {position.Longitude:F6}";
        });
    }

    private void OnGpsStatusChanged(object? sender, string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentStatus = status;
            IsGpsActive = _gpsService.IsComPortActive;
            IsSimulationActive = _gpsService.IsSimulationActive;
        });
    }
}