# PoligonMaui - Deployment Guide for Dell 7230 Tablet

## Target Hardware: Dell Latitude 7230 Rugged Extreme Tablet

### Hardware Specifications
- **Operating System**: Windows 11 / Android (dual boot capable)
- **Processor**: Intel Core i5/i7 (varies by model)
- **RAM**: 8GB/16GB DDR4
- **Storage**: 256GB/512GB/1TB SSD
- **Display**: 11.6" FHD (1920x1080) touchscreen
- **Connectivity**: Wi-Fi 6E, Bluetooth 5.2, Optional 4G/5G
- **Ports**: USB-A, USB-C, Serial port (RS-232)
- **GPS**: Integrated GPS receiver
- **Rugged Features**: IP65 rated, MIL-STD-810H tested

## Platform-Specific Deployment

### Windows 11 Deployment

#### Prerequisites
1. **Windows 11 Professional or Enterprise** (recommended for enterprise deployment)
2. **Visual Studio 2022** with .NET MAUI workload (for development)
3. **Windows App SDK** 1.4 or later
4. **Serial port drivers** for GPS hardware integration

#### Installation Steps
1. **Prepare the MBTiles file**:
   ```
   Copy your offline map file to: Resources\Raw\offline.mbtiles
   Ensure file size is appropriate (typically 100MB-2GB for tactical areas)
   ```

2. **Configure Serial Port**:
   ```
   - Connect GPS device to serial port (typically COM1 or COM3)
   - Verify port settings: 4800 baud, 8 data bits, no parity, 1 stop bit
   - Test NMEA GPRMC sentences are received
   ```

3. **Deploy Application**:
   ```bash
   # Build for Windows
   dotnet publish -f net8.0-windows10.0.19041.0 -c Release
   
   # Package as MSIX (for Microsoft Store or sideloading)
   dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:GenerateAppxPackageOnBuild=true
   ```

4. **Install on Tablet**:
   ```
   - Enable Developer Mode in Windows Settings
   - Install via PowerShell: Add-AppxPackage -Path "path\to\package.msix"
   - Or use Microsoft Store for Business deployment
   ```

#### Windows-Specific Features
- **Serial Port Access**: Full COM port support for GPS devices
- **File System**: Direct access to local storage for MBTiles files
- **Touch Optimization**: Optimized for 11.6" touchscreen interface
- **Power Management**: Configured for battery optimization

### Android Deployment

#### Prerequisites
1. **Android 7.0 (API level 24)** or higher
2. **GPS permissions** configured in manifest
3. **Storage permissions** for MBTiles file access
4. **Location services** enabled on device

#### Installation Steps
1. **Prepare APK**:
   ```bash
   # Build for Android
   dotnet build -f net8.0-android -c Release
   
   # Generate signed APK for production
   dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true
   ```

2. **Configure Permissions**:
   ```xml
   <!-- Already configured in AndroidManifest.xml -->
   <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
   <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
   <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
   ```

3. **Install on Tablet**:
   ```
   # Via ADB (Android Debug Bridge)
   adb install -r PoligonMaui.apk
   
   # Or sideload via file transfer and manual installation
   ```

#### Android-Specific Features
- **GPS Integration**: Uses Android Location Services
- **Simulation Mode**: Enhanced for testing without hardware GPS
- **Touch Gestures**: Native Android touch support
- **Battery Optimization**: Android-specific power management

## GPS Hardware Integration

### Supported GPS Devices
1. **Built-in GPS**: Dell 7230 integrated GPS receiver
2. **External GPS**: Serial/USB connected NMEA-compatible devices
3. **Military GPS**: SAASM-capable devices (with proper configuration)

### NMEA Configuration
- **Sentence Format**: GPRMC (Recommended Minimum Course)
- **Baud Rate**: 4800 (default), configurable in app
- **Update Rate**: 1Hz (1 second intervals)
- **Coordinate System**: WGS84 decimal degrees

### COM Port Settings
```
Default Configuration:
- Port: COM3 (configurable in Control Panel)
- Baud Rate: 4800
- Data Bits: 8
- Parity: None
- Stop Bits: 1
- Flow Control: None
```

## Offline Map Deployment

### MBTiles File Preparation
1. **Geographic Coverage**:
   ```
   - Define operational area boundaries
   - Recommended coverage: 50-100km radius from base
   - Consider zoom levels 1-16 for tactical use
   ```

2. **File Size Optimization**:
   ```
   - Zoom levels 1-10: Regional overview (small file size)
   - Zoom levels 11-14: Tactical detail (moderate size)  
   - Zoom levels 15-18: Close detail (large file size)
   
   Estimated sizes:
   - 10km x 10km area, zoom 1-14: ~50MB
   - 50km x 50km area, zoom 1-16: ~500MB
   - 100km x 100km area, zoom 1-18: ~2GB
   ```

3. **Map Sources**:
   ```
   Recommended sources:
   - OpenStreetMap (free, detailed)
   - Satellite imagery (commercial licensing required)
   - Military grid references (specialized sources)
   - Custom tactical overlays
   ```

### File Deployment
1. **Windows**: Place in `%USERPROFILE%\AppData\Local\Packages\PoligonMaui_[ID]\LocalState\offline.mbtiles`
2. **Android**: Place in `/Android/data/com.poligonmaui.app/files/offline.mbtiles`
3. **Network Deployment**: Use MDM (Mobile Device Management) for enterprise rollout

## Performance Optimization

### Hardware-Specific Optimizations
1. **Memory Management**:
   ```csharp
   // Configured in MauiProgram.cs
   - SQLite connection pooling
   - Tile cache size limits
   - Garbage collection optimization
   ```

2. **Battery Life**:
   ```
   - GPS update intervals: configurable (1-10 seconds)
   - Screen brightness: automatic based on ambient light
   - Background processing: minimized when app not in focus
   ```

3. **Storage Management**:
   ```
   - Database size monitoring
   - Automatic cleanup of old target data
   - MBTiles file validation and error recovery
   ```

## Security Considerations

### Data Protection
1. **Database Encryption**: SQLite database encrypted with device key
2. **Communication**: No external network communication required
3. **GPS Data**: Stored locally, not transmitted
4. **Maps**: Offline storage prevents data leakage

### Access Control
1. **Device Security**: Requires device PIN/biometric authentication
2. **App Permissions**: Minimal required permissions requested
3. **Data Isolation**: App data isolated from other applications

## Testing and Validation

### Pre-Deployment Testing
1. **GPS Accuracy Test**:
   ```
   - Compare with known reference points
   - Test in various environmental conditions
   - Validate NMEA sentence parsing
   ```

2. **Map Rendering Test**:
   ```
   - Verify MBTiles file loading
   - Test zoom/pan performance
   - Validate coordinate accuracy
   ```

3. **Target Functionality**:
   ```
   - Create/delete target groups
   - Test proximity detection (300m threshold)
   - Validate database persistence
   ```

### Field Testing Checklist
- [ ] GPS acquisition time < 30 seconds
- [ ] Map renders smoothly at all zoom levels
- [ ] Target placement accuracy within 5 meters
- [ ] Battery life > 8 hours continuous use
- [ ] Touchscreen responsiveness in all conditions
- [ ] Data persistence through app restart
- [ ] Simulation mode functions correctly
- [ ] All control panel buttons responsive

## Troubleshooting

### Common Issues
1. **GPS Not Working**:
   ```
   - Check COM port configuration
   - Verify GPS device connected and powered
   - Test with GPS simulation mode
   - Check Windows Device Manager for port conflicts
   ```

2. **Map Not Loading**:
   ```
   - Verify MBTiles file exists and is not corrupted
   - Check file permissions
   - Try with smaller test MBTiles file
   - Monitor app logs for SQLite errors
   ```

3. **Performance Issues**:
   ```
   - Reduce MBTiles file size/zoom levels
   - Increase GPS update interval
   - Close other running applications
   - Check available storage space
   ```

### Logging and Diagnostics
- **Application Logs**: Available in Windows Event Viewer or Android Logcat
- **GPS Diagnostics**: Built-in NMEA sentence validation
- **Database Integrity**: SQLite PRAGMA integrity_check
- **Performance Metrics**: Memory usage and rendering frame rates

## Support and Maintenance

### Regular Maintenance
1. **Map Updates**: Replace MBTiles files quarterly or as needed
2. **Database Cleanup**: Reset target data periodically
3. **App Updates**: Deploy via MDM or manual installation
4. **Hardware Maintenance**: Clean touchscreen, check GPS antenna

### Backup and Recovery
1. **Target Data Backup**: Export SQLite database regularly
2. **Configuration Backup**: Document COM port and GPS settings
3. **Map Archive**: Maintain copies of all MBTiles files used
4. **Recovery Procedures**: Document complete reinstallation process

For technical support, contact the development team or refer to the project documentation repository.