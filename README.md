# PoligonMaui - Offline Maps & Targets Application

A .NET MAUI cross-platform application for working with offline maps and target tracking, specifically designed for Dell 7230 tablet (Windows/Android) deployment.

## Features

### ✅ Implemented Core Features

#### GPS & Positioning System
- **COM Port GPS Integration**: Supports NMEA GPRMC format GPS data via serial ports
- **GPS Simulation**: Configurable simulation with speed and update intervals 
- **Position Tracking**: Real-time user location tracking and display
- **Auto-follow**: Map automatically centers on current position
- **Map Rotation**: Option to rotate map based on movement direction

#### Target Management
- **Target Groups**: Organize targets by groups with coordinate capture
- **SQLite Database**: Persistent storage for targets and groups
- **Target States**: Track reached/not-reached status
- **Distance Calculation**: Haversine formula for accurate distance measurement
- **Proximity Detection**: 300-meter threshold for target reached detection

#### User Interface
- **Control Panel**: Complete interface for all GPS and map controls
- **Status Indicators**: Color-coded status (Green/Yellow/Red) for GPS modes
- **Modern MVVM**: Clean architecture using CommunityToolkit.Mvvm
- **Responsive Design**: Optimized for tablet usage

#### Core Services
- **Database Service**: SQLite integration with async operations  
- **GPS Service**: COM port and simulation management
- **Map Service**: Target management and route calculations
- **NMEA Parser**: Professional NMEA GPRMC sentence parsing

### 🚧 Pending Integration Features

#### Map Display & Interaction
- **Offline Maps**: MBTiles integration with BruTile (structure ready)
- **Layered Architecture**: Separate layers for map, markers, and routes
- **Map Controls**: Zoom, pan, and rotation controls
- **Visual Markers**: Animated target markers on map

#### Advanced Features  
- **Route Visualization**: Drawing routes to nearest targets
- **Marker Animation**: Jumping animation for closest targets
- **Touch Interaction**: Tap-to-add target functionality
- **Route Animation**: Moving arrows along calculated routes

## Technical Architecture

### Project Structure
```
PoligonMaui/
├── Models/                    # Data models (Target, TargetGroup, GpsPosition)
├── ViewModels/               # MVVM ViewModels with CommunityToolkit
├── Views/                    # XAML pages and code-behind
├── Services/                 # Business logic services
│   ├── Interfaces/          # Service contracts
│   ├── GpsService.cs        # GPS and simulation management
│   ├── MapService.cs        # Map and target operations
│   ├── DatabaseService.cs   # SQLite data access
│   └── NmeaParserService.cs # NMEA sentence parsing
├── Data/                     # Database context and repositories
├── Helpers/                  # Utility classes
│   ├── DistanceCalculator.cs
│   ├── CoordinateConverter.cs
│   └── AnimationHelper.cs
├── Constants/                # Application constants
├── Converters/               # XAML value converters
└── Resources/                # Styles, images, fonts
```

### Dependencies
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="Mapsui" Version="5.0.0" />
<PackageReference Include="Mapsui.UI.Maui" Version="5.0.0" />
<PackageReference Include="BruTile.MbTiles" Version="5.0.1" />
<PackageReference Include="System.IO.Ports" Version="8.0.0" />
```

## Getting Started

### Prerequisites
- Visual Studio 2022 (17.8 or later) or Visual Studio Code
- .NET 8.0 SDK
- Android SDK (for Android development)
- Windows SDK (for Windows development)

### Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd PoligonMaui
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Deploy Offline Maps**
   - Place your `.mbtiles` file in `Resources/Maps/offline.mbtiles`
   - Ensure the file follows MBTiles specification

4. **Configure COM Ports**
   - Update `AppConstants.cs` with your GPS device's COM port
   - Ensure proper permissions in platform manifests

5. **Build and Run**
   ```bash
   # For Windows
   dotnet build -f net8.0-windows10.0.19041.0
   
   # For Android  
   dotnet build -f net8.0-android
   ```

## Usage Guide

### GPS Operation Modes

#### COM Port GPS Mode (Green Indicator)
1. Connect GPS device to COM port
2. Click "COM GPS" button in Control Panel
3. Enter correct COM port name (default: COM3)
4. System will parse NMEA GPRMC sentences

#### Simulation Mode (Yellow Indicator)  
1. Click "SIMULATION" button in Control Panel
2. Adjust speed and update interval as needed
3. Simulation starts from Prague coordinates by default
4. Position updates with realistic movement patterns

### Target Management

#### Creating Target Groups
1. Navigate to desired location (GPS or simulation)
2. Click "Add Group" button on Map page
3. Group is created at current GPS coordinates
4. Add individual targets to groups as needed

#### Target States
- **Red**: Default state, not reached
- **Yellow**: Currently nearest/active target  
- **Green**: Target reached (within 300m)

### Map Controls
- **Auto Follow**: Map centers on current position
- **Manual Control**: Free map navigation
- **Turn by Movement**: Map rotates with GPS course
- **Fixed North**: Map maintains north orientation

## Dell 7230 Tablet Configuration

### Windows Configuration
```xml
<DeviceCapability Name="location" />
<DeviceCapability Name="serialcommunication" />
```

### Android Configuration  
```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

## Development Status

### ✅ Completed (Phase 1-3)
- [x] Project structure and dependencies
- [x] SQLite database with async operations
- [x] MVVM architecture with CommunityToolkit
- [x] GPS COM port integration
- [x] GPS simulation system  
- [x] Target and group management
- [x] Distance calculations and proximity detection
- [x] Control panel UI with all buttons
- [x] Status indicator system
- [x] Value converters and styling

### 🚧 In Development (Phase 4-6)
- [ ] Mapsui offline map integration
- [ ] Visual map markers and overlays
- [ ] Target placement via map interaction
- [ ] Animated markers for closest targets
- [ ] Route drawing and visualization
- [ ] Performance optimization

## API Reference

### Key Classes

#### `GpsService` 
Manages GPS data acquisition from COM ports or simulation.
```csharp
Task<bool> StartComPortAsync(string portName);
Task<bool> StartSimulationAsync();
event EventHandler<GpsPosition> PositionChanged;
```

#### `DatabaseService`
Handles SQLite operations for targets and groups.
```csharp
Task<List<Target>> GetAllTargetsAsync();
Task<int> SaveTargetAsync(Target target);
Task ResetAllTargetsAsync();
```

#### `MapService` 
Manages map operations and target calculations.
```csharp
Task<Target?> GetNearestTargetAsync(GpsPosition position);
Task<double> CalculateDistanceAsync(GpsPosition from, GpsPosition to);
Task<MapRoute> CalculateRouteAsync(GpsPosition from, GpsPosition to);
```

## Contributing

1. Follow the existing MVVM architecture patterns
2. Use async/await for all database and GPS operations  
3. Implement proper error handling and logging
4. Update unit tests for new features
5. Maintain compatibility with Dell 7230 tablet requirements

## License

This project is developed for specialized military/tactical applications. Contact the development team for licensing information.

## Support

For technical support or feature requests, please contact the development team or create an issue in the project repository.