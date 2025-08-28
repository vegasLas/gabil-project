namespace PoligonMaui.Constants;

public static class AppConstants
{
    // GPS and Navigation
    public const double ProximityThresholdMeters = 300.0;
    public const double DefaultSimulationSpeedKmh = 20.0;
    public const int DefaultSimulationIntervalMs = 1000;
    
    // Map Settings
    public const double DefaultZoomLevel = 15.0;
    public const double MinZoomLevel = 1.0;
    public const double MaxZoomLevel = 20.0;
    
    // Default Map Center (Prague, Czech Republic)
    public const double DefaultCenterLatitude = 50.0755;
    public const double DefaultCenterLongitude = 14.4378;
    
    // Target Colors
    public const string DefaultTargetColor = "#FF0000"; // Red
    public const string ReachedTargetColor = "#00FF00"; // Green
    public const string ActiveTargetColor = "#FFFF00"; // Yellow
    
    // Status Colors
    public const string GpsStatusColor = "#00FF00"; // Green
    public const string SimulationStatusColor = "#FFFF00"; // Yellow  
    public const string ErrorStatusColor = "#FF0000"; // Red
    public const string DisconnectedStatusColor = "#808080"; // Gray
    
    // Animation Durations
    public const uint MarkerJumpDuration = 800;
    public const uint RouteArrowDuration = 2000;
    public const uint StatusIndicatorDuration = 1000;
    public const uint ColorChangeDuration = 500;
    
    // Database
    public const string DatabaseFileName = "PoligonMaui.db3";
    
    // File Paths
    public const string OfflineMapFileName = "offline.mbtiles";
    
    // COM Port Settings
    public const int DefaultBaudRate = 4800;
    public const int SerialReadTimeout = 1000;
    public const int SerialWriteTimeout = 1000;
    
    // UI Layout
    public const double ControlPanelHeight = 120;
    public const double ButtonHeight = 40;
    public const double ButtonWidth = 120;
    public const double StatusIndicatorSize = 20;
    
    // Route Calculation
    public const double EarthRadiusKm = 6371.0;
    public const double EarthRadiusMeters = 6371000.0;
    
    // Distance Formatting
    public const int CoordinateDecimalPlaces = 6;
    public const int DistanceDecimalPlaces = 1;
    public const int SpeedDecimalPlaces = 1;
    
    // Target Group Limits
    public const int MaxTargetsPerGroup = 50;
    public const int MaxTargetGroups = 20;
}