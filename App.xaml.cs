using PoligonMaui.Views;

namespace PoligonMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}