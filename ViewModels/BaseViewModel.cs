using CommunityToolkit.Mvvm.ComponentModel;

namespace PoligonMaui.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy = false;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public virtual async Task OnAppearingAsync()
    {
        await Task.CompletedTask;
    }

    public virtual async Task OnDisappearingAsync()
    {
        await Task.CompletedTask;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        // Override if needed for custom property change handling
    }
}