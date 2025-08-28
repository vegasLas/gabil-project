using PoligonMaui.Models;

namespace PoligonMaui.Services.Interfaces;

public interface IGpsService
{
    event EventHandler<GpsPosition>? PositionChanged;
    event EventHandler<string>? StatusChanged;
    
    Task<bool> StartComPortAsync(string portName);
    Task StopComPortAsync();
    Task<bool> StartSimulationAsync();
    Task StopSimulationAsync();
    
    GpsPosition? CurrentPosition { get; }
    bool IsComPortActive { get; }
    bool IsSimulationActive { get; }
    string CurrentStatus { get; }
    
    Task SetSimulationStartPositionAsync(double latitude, double longitude);
    Task SetSimulationSpeedAsync(double speedKmh);
    Task SetSimulationIntervalAsync(int intervalMs);
}