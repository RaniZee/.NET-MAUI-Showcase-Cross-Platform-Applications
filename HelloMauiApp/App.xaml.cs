using static HelloMauiApp.MauiProgram;

namespace HelloMauiApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        //MainPage = new CodePage();
        //MainPage = new DynamicXamlPage();
    }
}