using System.IO.Ports;
using PoligonMaui.Models;
using PoligonMaui.Services.Interfaces;

namespace PoligonMaui.Services;

public class GpsService : IGpsService, IDisposable
{
    private readonly INmeaParserService _nmeaParser;
    private SerialPort? _serialPort;
    private Timer? _simulationTimer;
    
    private GpsPosition _simulationPosition = new GpsPosition(50.0755, 14.4378); // Prague coordinates
    private double _simulationSpeed = 20.0; // km/h
    private int _simulationInterval = 1000; // ms
    private double _simulationBearing = 0.0; // degrees
    private Random _random = new Random();

    public event EventHandler<GpsPosition>? PositionChanged;
    public event EventHandler<string>? StatusChanged;

    public GpsPosition? CurrentPosition { get; private set; }
    public bool IsComPortActive => _serialPort?.IsOpen == true;
    public bool IsSimulationActive => _simulationTimer != null;
    public string CurrentStatus { get; private set; } = "Disconnected";

    public GpsService(INmeaParserService nmeaParser)
    {
        _nmeaParser = nmeaParser;
    }

    public async Task<bool> StartComPortAsync(string portName)
    {
        try
        {
            await StopSimulationAsync();
            await StopComPortAsync();

            _serialPort = new SerialPort(portName, 4800, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };

            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();

            CurrentStatus = "COM GPS Connected";
            StatusChanged?.Invoke(this, CurrentStatus);
            return true;
        }
        catch (Exception ex)
        {
            CurrentStatus = $"COM GPS Error: {ex.Message}";
            StatusChanged?.Invoke(this, CurrentStatus);
            return false;
        }
    }

    public async Task StopComPortAsync()
    {
        if (_serialPort != null)
        {
            try
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;

                CurrentStatus = "Disconnected";
                StatusChanged?.Invoke(this, CurrentStatus);
            }
            catch (Exception ex)
            {
                CurrentStatus = $"Error stopping COM port: {ex.Message}";
                StatusChanged?.Invoke(this, CurrentStatus);
            }
        }
        await Task.CompletedTask;
    }

    public async Task<bool> StartSimulationAsync()
    {
        try
        {
            await StopComPortAsync();
            await StopSimulationAsync();

            _simulationTimer = new Timer(SimulationTimer_Tick, null, 0, _simulationInterval);
            
            CurrentStatus = "GPS Simulation Active";
            StatusChanged?.Invoke(this, CurrentStatus);
            return true;
        }
        catch (Exception ex)
        {
            CurrentStatus = $"Simulation Error: {ex.Message}";
            StatusChanged?.Invoke(this, CurrentStatus);
            return false;
        }
    }

    public async Task StopSimulationAsync()
    {
        if (_simulationTimer != null)
        {
            await _simulationTimer.DisposeAsync();
            _simulationTimer = null;

            CurrentStatus = "Disconnected";
            StatusChanged?.Invoke(this, CurrentStatus);
        }
    }

    public async Task SetSimulationStartPositionAsync(double latitude, double longitude)
    {
        _simulationPosition = new GpsPosition(latitude, longitude);
        await Task.CompletedTask;
    }

    public async Task SetSimulationSpeedAsync(double speedKmh)
    {
        _simulationSpeed = speedKmh;
        await Task.CompletedTask;
    }

    public async Task SetSimulationIntervalAsync(int intervalMs)
    {
        _simulationInterval = intervalMs;
        
        if (_simulationTimer != null)
        {
            await _simulationTimer.DisposeAsync();
            _simulationTimer = new Timer(SimulationTimer_Tick, null, 0, _simulationInterval);
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var serialPort = (SerialPort)sender;
            string data = serialPort.ReadLine();
            
            if (!string.IsNullOrEmpty(data))
            {
                var position = _nmeaParser.ParseGprmc(data);
                if (position != null)
                {
                    CurrentPosition = position;
                    PositionChanged?.Invoke(this, position);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading COM port: {ex.Message}");
            CurrentStatus = $"COM GPS Error: {ex.Message}";
            StatusChanged?.Invoke(this, CurrentStatus);
        }
    }

    private void SimulationTimer_Tick(object? state)
    {
        try
        {
            // Add some randomness to the movement
            var randomOffset = 0.0001; // About 11 meters
            var latOffset = (_random.NextDouble() - 0.5) * randomOffset;
            var lonOffset = (_random.NextDouble() - 0.5) * randomOffset;

            // Calculate movement based on speed and time interval
            var timeStep = _simulationInterval / 1000.0; // seconds
            var distanceKm = (_simulationSpeed * timeStep) / 3600.0; // km moved in this time step
            
            // Convert to degrees (rough approximation)
            var latDegrees = distanceKm / 111.0; // 1 degree lat ≈ 111 km
            var lonDegrees = distanceKm / (111.0 * Math.Cos(_simulationPosition.Latitude * Math.PI / 180));

            // Update bearing slightly for more realistic movement
            _simulationBearing += (_random.NextDouble() - 0.5) * 20; // ±10 degrees
            if (_simulationBearing < 0) _simulationBearing += 360;
            if (_simulationBearing >= 360) _simulationBearing -= 360;

            // Calculate new position
            var bearingRad = _simulationBearing * Math.PI / 180;
            _simulationPosition.Latitude += latDegrees * Math.Cos(bearingRad) + latOffset;
            _simulationPosition.Longitude += lonDegrees * Math.Sin(bearingRad) + lonOffset;
            _simulationPosition.Speed = _simulationSpeed;
            _simulationPosition.Course = _simulationBearing;
            _simulationPosition.Timestamp = DateTime.UtcNow;

            CurrentPosition = _simulationPosition;
            PositionChanged?.Invoke(this, _simulationPosition);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Simulation error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _simulationTimer?.Dispose();
        if (_serialPort != null)
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            if (_serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.Dispose();
        }
    }
}