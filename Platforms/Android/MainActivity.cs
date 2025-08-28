using Android;
using Android.App;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace PoligonMaui.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int RequestLocationPermission = 1000;
    private const int RequestStoragePermission = 1001;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Request necessary permissions
        RequestRequiredPermissions();
    }

    private void RequestRequiredPermissions()
    {
        var permissions = new[]
        {
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.ReadExternalStorage
        };

        var permissionsToRequest = new List<string>();

        foreach (var permission in permissions)
        {
            if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
            {
                permissionsToRequest.Add(permission);
            }
        }

        if (permissionsToRequest.Count > 0)
        {
            ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), RequestLocationPermission);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        switch (requestCode)
        {
            case RequestLocationPermission:
                HandleLocationPermissionResult(permissions, grantResults);
                break;
            case RequestStoragePermission:
                HandleStoragePermissionResult(permissions, grantResults);
                break;
        }
    }

    private void HandleLocationPermissionResult(string[] permissions, Permission[] grantResults)
    {
        for (int i = 0; i < permissions.Length; i++)
        {
            var permission = permissions[i];
            var result = grantResults[i];

            if (permission == Manifest.Permission.AccessFineLocation ||
                permission == Manifest.Permission.AccessCoarseLocation)
            {
                if (result == Permission.Granted)
                {
                    System.Diagnostics.Debug.WriteLine($"Location permission granted: {permission}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Location permission denied: {permission}");
                    // Could show a dialog explaining why the permission is needed
                }
            }
        }
    }

    private void HandleStoragePermissionResult(string[] permissions, Permission[] grantResults)
    {
        for (int i = 0; i < permissions.Length; i++)
        {
            var permission = permissions[i];
            var result = grantResults[i];

            if (result == Permission.Granted)
            {
                System.Diagnostics.Debug.WriteLine($"Storage permission granted: {permission}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Storage permission denied: {permission}");
            }
        }
    }
}