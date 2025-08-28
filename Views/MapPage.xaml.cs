using PoligonMaui.ViewModels;

namespace PoligonMaui.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
        await _viewModel.OnAppearingAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.OnDisappearingAsync();
    }

    private async void OnMapTapped(object sender, (double Latitude, double Longitude) coordinates)
    {
        try
        {
            // Add target at tapped coordinates
            await _viewModel.AddTargetCommand.ExecuteAsync(coordinates);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling map tap: {ex.Message}");
        }
    }
}