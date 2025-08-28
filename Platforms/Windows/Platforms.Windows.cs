using Microsoft.UI.Xaml;

namespace PoligonMaui.Platforms.Windows;

public class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // Configure for tablet mode if needed
        var window = Application.Current?.MainPage?.GetParentWindow()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (window != null)
        {
            // Set minimum window size for Dell 7230 tablet
            window.SetIsResizable(true);
            window.SetIsMaximizable(true);
            window.SetIsMinimizable(true);
        }
    }
}