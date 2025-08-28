using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using BruTile.MbTiles;
using BruTile;
using PoligonMaui.Models;
using System.Collections.ObjectModel;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace PoligonMaui.Controls;

public class CustomMapControl : MapControl
{
    private TileLayer? _baseMapLayer;
    private MemoryLayer? _targetLayer;
    private MemoryLayer? _routeLayer;
    private MemoryLayer? _positionLayer;
    private readonly ObservableCollection<Target> _targets = new();
    private GpsPosition? _currentPosition;
    private MapRoute? _currentRoute;

    public static readonly BindableProperty TargetsProperty = BindableProperty.Create(
        nameof(Targets), typeof(ObservableCollection<Target>), typeof(CustomMapControl), 
        propertyChanged: OnTargetsChanged);

    public static readonly BindableProperty CurrentPositionProperty = BindableProperty.Create(
        nameof(CurrentPosition), typeof(GpsPosition), typeof(CustomMapControl), 
        propertyChanged: OnCurrentPositionChanged);

    public static readonly BindableProperty CurrentRouteProperty = BindableProperty.Create(
        nameof(CurrentRoute), typeof(MapRoute), typeof(CustomMapControl), 
        propertyChanged: OnCurrentRouteChanged);

    public static readonly BindableProperty AutoFollowProperty = BindableProperty.Create(
        nameof(AutoFollow), typeof(bool), typeof(CustomMapControl), true);

    public static readonly BindableProperty RotateWithMovementProperty = BindableProperty.Create(
        nameof(RotateWithMovement), typeof(bool), typeof(CustomMapControl), false);

    public ObservableCollection<Target> Targets
    {
        get => (ObservableCollection<Target>)GetValue(TargetsProperty);
        set => SetValue(TargetsProperty, value);
    }

    public GpsPosition? CurrentPosition
    {
        get => (GpsPosition?)GetValue(CurrentPositionProperty);
        set => SetValue(CurrentPositionProperty, value);
    }

    public MapRoute? CurrentRoute
    {
        get => (MapRoute?)GetValue(CurrentRouteProperty);
        set => SetValue(CurrentRouteProperty, value);
    }

    public bool AutoFollow
    {
        get => (bool)GetValue(AutoFollowProperty);
        set => SetValue(AutoFollowProperty, value);
    }

    public bool RotateWithMovement
    {
        get => (bool)GetValue(RotateWithMovementProperty);
        set => SetValue(RotateWithMovementProperty, value);
    }

    public event EventHandler<(double Latitude, double Longitude)>? MapTapped;

    public CustomMapControl()
    {
        InitializeMap();
        
        // Subscribe to map events
        this.MapClicked += OnMapClicked;
    }

    private void InitializeMap()
    {
        try
        {
            Map = new Mapsui.Map();
            
            // Create layers
            CreateBaseMapLayer();
            CreateTargetLayer();
            CreateRouteLayer();
            CreatePositionLayer();

            // Set default view
            SetDefaultView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing map: {ex.Message}");
        }
    }

    private void CreateBaseMapLayer()
    {
        try
        {
            // Try to load offline MBTiles first
            var mbTilesPath = Path.Combine(FileSystem.AppDataDirectory, "offline.mbtiles");
            
            if (File.Exists(mbTilesPath))
            {
                var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(mbTilesPath, false));
                _baseMapLayer = new TileLayer(mbTilesTileSource)
                {
                    Name = "OfflineMap"
                };
            }
            else
            {
                // Fallback to OpenStreetMap online (for development/testing)
                var httpTileSource = BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.OpenStreetMap);
                _baseMapLayer = new TileLayer(httpTileSource)
                {
                    Name = "OpenStreetMap"
                };
                
                System.Diagnostics.Debug.WriteLine("MBTiles file not found, using online OpenStreetMap");
            }

            if (_baseMapLayer != null && Map != null)
            {
                Map.Layers.Add(_baseMapLayer);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating base map layer: {ex.Message}");
        }
    }

    private void CreateTargetLayer()
    {
        _targetLayer = new MemoryLayer
        {
            Name = "Targets",
            Style = null
        };

        if (Map != null)
        {
            Map.Layers.Add(_targetLayer);
        }
    }

    private void CreateRouteLayer()
    {
        _routeLayer = new MemoryLayer
        {
            Name = "Route",
            Style = null
        };

        if (Map != null)
        {
            Map.Layers.Add(_routeLayer);
        }
    }

    private void CreatePositionLayer()
    {
        _positionLayer = new MemoryLayer
        {
            Name = "Position",
            Style = null
        };

        if (Map != null)
        {
            Map.Layers.Add(_positionLayer);
        }
    }

    private void SetDefaultView()
    {
        if (Map != null)
        {
            // Default to Prague coordinates
            var centerPoint = SphericalMercator.FromLonLat(14.4378, 50.0755);
            Map.Home = navigator => navigator.CenterOnAndZoomTo(centerPoint, 15);
            Map.Navigator.GoHome();
        }
    }

    public async Task LoadOfflineMapAsync(string mbTilesPath)
    {
        try
        {
            if (!File.Exists(mbTilesPath))
            {
                System.Diagnostics.Debug.WriteLine($"MBTiles file not found: {mbTilesPath}");
                return;
            }

            // Remove existing base layer
            if (_baseMapLayer != null && Map != null)
            {
                Map.Layers.Remove(_baseMapLayer);
            }

            // Create new MBTiles layer
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(mbTilesPath, false));
            _baseMapLayer = new TileLayer(mbTilesTileSource)
            {
                Name = "OfflineMap"
            };

            if (Map != null)
            {
                Map.Layers.Insert(0, _baseMapLayer); // Add as base layer
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading offline map: {ex.Message}");
        }
    }

    public void UpdateTargets(IEnumerable<Target> targets)
    {
        if (_targetLayer == null) return;

        try
        {
            _targetLayer.Clear();

            foreach (var target in targets)
            {
                var feature = CreateTargetFeature(target);
                _targetLayer.Add(feature);
            }

            Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating targets: {ex.Message}");
        }
    }

    private Feature CreateTargetFeature(Target target)
    {
        var point = SphericalMercator.FromLonLat(target.Longitude, target.Latitude);
        var geometry = new Point(point.X, point.Y);
        
        var feature = new Feature
        {
            Geometry = geometry,
            ["Id"] = target.Id,
            ["Name"] = target.Name,
            ["IsReached"] = target.IsReached
        };

        // Set style based on target state
        feature.Styles.Add(CreateTargetStyle(target));
        
        return feature;
    }

    private IStyle CreateTargetStyle(Target target)
    {
        var color = target.IsReached ? Mapsui.Styles.Color.Green : 
                   target.Color == "#FFFF00" ? Mapsui.Styles.Color.Yellow : 
                   Mapsui.Styles.Color.Red;

        return new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = target.IsReached ? 0.8 : 1.0,
            Fill = new Brush(color),
            Outline = new Pen(Mapsui.Styles.Color.Black, 2),
            Width = 20,
            Height = 20
        };
    }

    public void UpdateCurrentPosition(GpsPosition? position)
    {
        if (_positionLayer == null || position == null) return;

        try
        {
            _positionLayer.Clear();

            var point = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);
            var geometry = new Point(point.X, point.Y);
            
            var feature = new Feature
            {
                Geometry = geometry,
                ["Type"] = "CurrentPosition"
            };

            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = 1.2,
                Fill = new Brush(Mapsui.Styles.Color.Blue),
                Outline = new Pen(Mapsui.Styles.Color.White, 3),
                Width = 15,
                Height = 15
            });

            _positionLayer.Add(feature);

            // Auto-follow if enabled
            if (AutoFollow && Map != null)
            {
                var centerPoint = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);
                Map.Navigator.CenterOn(centerPoint);
            }

            // Rotate map if enabled
            if (RotateWithMovement && position.Course > 0 && Map != null)
            {
                Map.Navigator.RotateTo(position.Course);
            }

            Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating current position: {ex.Message}");
        }
    }

    public void UpdateRoute(MapRoute? route)
    {
        if (_routeLayer == null) return;

        try
        {
            _routeLayer.Clear();

            if (route?.RoutePoints.Count > 1)
            {
                var coordinates = route.RoutePoints
                    .Select(p => SphericalMercator.FromLonLat(p.Longitude, p.Latitude))
                    .Select(p => new Coordinate(p.X, p.Y))
                    .ToArray();

                var lineString = new LineString(coordinates);
                var feature = new Feature
                {
                    Geometry = lineString,
                    ["Type"] = "Route"
                };

                feature.Styles.Add(new VectorStyle
                {
                    Line = new Pen(Mapsui.Styles.Color.Purple, 4),
                    Opacity = 0.8f
                });

                _routeLayer.Add(feature);
            }

            Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating route: {ex.Message}");
        }
    }

    public void CenterOnCoordinate(double latitude, double longitude, double? zoomLevel = null)
    {
        if (Map == null) return;

        try
        {
            var centerPoint = SphericalMercator.FromLonLat(longitude, latitude);
            
            if (zoomLevel.HasValue)
            {
                Map.Navigator.CenterOnAndZoomTo(centerPoint, zoomLevel.Value);
            }
            else
            {
                Map.Navigator.CenterOn(centerPoint);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error centering on coordinate: {ex.Message}");
        }
    }

    public void SetZoomLevel(double zoomLevel)
    {
        if (Map == null) return;

        try
        {
            Map.Navigator.ZoomTo(zoomLevel);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting zoom level: {ex.Message}");
        }
    }

    public void SetRotation(double rotation)
    {
        if (Map == null) return;

        try
        {
            Map.Navigator.RotateTo(rotation);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting rotation: {ex.Message}");
        }
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        try
        {
            var worldPosition = e.Point;
            var lonLat = SphericalMercator.ToLonLat(worldPosition.X, worldPosition.Y);
            
            MapTapped?.Invoke(this, (lonLat.Y, lonLat.X)); // (Latitude, Longitude)
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling map click: {ex.Message}");
        }
    }

    private static void OnTargetsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomMapControl mapControl && newValue is ObservableCollection<Target> targets)
        {
            mapControl.UpdateTargets(targets);
        }
    }

    private static void OnCurrentPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomMapControl mapControl && newValue is GpsPosition position)
        {
            mapControl.UpdateCurrentPosition(position);
        }
    }

    private static void OnCurrentRouteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomMapControl mapControl && newValue is MapRoute route)
        {
            mapControl.UpdateRoute(route);
        }
    }
}