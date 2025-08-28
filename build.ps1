# PoligonMaui Build Script for Dell 7230 Tablet
# PowerShell script for automated building and packaging

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Windows", "Android", "Both")]
    [string]$Platform = "Both"
)

Write-Host "Starting PoligonMaui build process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow

# Function to check if command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Verify prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Cyan

if (!(Test-Command "dotnet")) {
    Write-Error ".NET SDK is required but not found. Please install .NET 8.0 SDK."
    exit 1
}

$dotnetVersion = dotnet --version
Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Green

# Create output directory
$outputDir = Join-Path $PSScriptRoot "bin\$Configuration"
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Package restore failed"
    exit 1
}

# Build for Windows
if ($Platform -eq "Windows" -or $Platform -eq "Both") {
    Write-Host "Building for Windows..." -ForegroundColor Cyan
    
    $windowsOutput = Join-Path $outputDir "Windows"
    
    # Build Windows version
    dotnet build -f net8.0-windows10.0.19041.0 -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Windows build failed"
        exit 1
    }
    
    # Publish Windows version
    dotnet publish -f net8.0-windows10.0.19041.0 -c $Configuration -o $windowsOutput
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Windows publish failed"
        exit 1
    }
    
    Write-Host "Windows build completed: $windowsOutput" -ForegroundColor Green
}

# Build for Android
if ($Platform -eq "Android" -or $Platform -eq "Both") {
    Write-Host "Building for Android..." -ForegroundColor Cyan
    
    $androidOutput = Join-Path $outputDir "Android"
    
    # Build Android version
    dotnet build -f net8.0-android -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Android build failed"
        exit 1
    }
    
    # Publish Android APK
    dotnet publish -f net8.0-android -c $Configuration -o $androidOutput
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Android publish failed"
        exit 1
    }
    
    Write-Host "Android build completed: $androidOutput" -ForegroundColor Green
}

# Package for Dell 7230 deployment
Write-Host "Creating deployment package..." -ForegroundColor Cyan

$packageDir = Join-Path $PSScriptRoot "package"
if (Test-Path $packageDir) {
    Remove-Item $packageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# Copy build outputs
if ($Platform -eq "Windows" -or $Platform -eq "Both") {
    $windowsPackage = Join-Path $packageDir "Dell7230-Windows"
    New-Item -ItemType Directory -Path $windowsPackage -Force | Out-Null
    Copy-Item -Path (Join-Path $outputDir "Windows\*") -Destination $windowsPackage -Recurse
}

if ($Platform -eq "Android" -or $Platform -eq "Both") {
    $androidPackage = Join-Path $packageDir "Dell7230-Android"
    New-Item -ItemType Directory -Path $androidPackage -Force | Out-Null
    Copy-Item -Path (Join-Path $outputDir "Android\*") -Destination $androidPackage -Recurse
}

# Copy documentation
Copy-Item -Path (Join-Path $PSScriptRoot "README.md") -Destination $packageDir
Copy-Item -Path (Join-Path $PSScriptRoot "DEPLOYMENT.md") -Destination $packageDir

# Create installation instructions
$installInstructions = @"
# PoligonMaui Installation for Dell 7230 Tablet

## Windows Installation
1. Copy the contents of Dell7230-Windows folder to the tablet
2. Run the executable file: PoligonMaui.exe
3. Grant necessary permissions for GPS and file access
4. Place your offline.mbtiles file in the application data directory

## Android Installation  
1. Enable "Unknown Sources" in Android Settings > Security
2. Copy the APK file from Dell7230-Android folder to the tablet
3. Install by tapping the APK file
4. Grant location and storage permissions when prompted
5. Place your offline.mbtiles file in the app's documents directory

## GPS Setup
1. Connect GPS device to serial port (Windows) or enable location services (Android)
2. Configure COM port in the Control Panel (default: COM3, 4800 baud)
3. Test with simulation mode before using live GPS

## Map Setup
1. Obtain MBTiles file for your operational area
2. Rename to "offline.mbtiles" 
3. Place in application directory as shown in DEPLOYMENT.md

See README.md and DEPLOYMENT.md for detailed instructions.

Built on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")
Version: $Configuration build
"@

$installInstructions | Out-File -FilePath (Join-Path $packageDir "INSTALLATION.txt") -Encoding UTF8

Write-Host "Build process completed successfully!" -ForegroundColor Green
Write-Host "Package location: $packageDir" -ForegroundColor Yellow

# Display build summary
Write-Host "`n=== Build Summary ===" -ForegroundColor Cyan
if ($Platform -eq "Windows" -or $Platform -eq "Both") {
    $windowsSize = (Get-ChildItem -Path (Join-Path $outputDir "Windows") -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "Windows build size: $([math]::Round($windowsSize, 2)) MB" -ForegroundColor White
}
if ($Platform -eq "Android" -or $Platform -eq "Both") {
    $androidSize = (Get-ChildItem -Path (Join-Path $outputDir "Android") -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB  
    Write-Host "Android build size: $([math]::Round($androidSize, 2)) MB" -ForegroundColor White
}

Write-Host "`nReady for deployment to Dell 7230 tablet!" -ForegroundColor Green
Write-Host "Don't forget to add your MBTiles file before deployment." -ForegroundColor Yellow