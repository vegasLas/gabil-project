using Microsoft.Extensions.Logging;
using PoligonMaui.Services;
using PoligonMaui.Services.Interfaces;
using PoligonMaui.ViewModels;
using PoligonMaui.Views;
using PoligonMaui.Data;
using PoligonMaui.Converters;

namespace PoligonMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if DEBUG
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        // Register database context
        builder.Services.AddSingleton<AppDbContext>();

        // Register services
        builder.Services.AddSingleton<IGpsService, GpsService>();
        builder.Services.AddSingleton<IMapService, MapService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<INmeaParserService, NmeaParserService>();

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<ControlPanelViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<ControlPanelPage>();

        // Register Value Converters
        builder.Services.AddSingleton<BoolToColorConverter>();
        builder.Services.AddSingleton<InvertedBoolConverter>();
        builder.Services.AddSingleton<InvertedBoolToColorConverter>();
        builder.Services.AddSingleton<IsNotNullConverter>();

        return builder.Build();
    }
}