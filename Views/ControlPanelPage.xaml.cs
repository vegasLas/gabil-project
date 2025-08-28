using PoligonMaui.ViewModels;

namespace PoligonMaui.Views;

public partial class ControlPanelPage : ContentPage
{
    private readonly ControlPanelViewModel _viewModel;

    public ControlPanelPage(ControlPanelViewModel viewModel)
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
}