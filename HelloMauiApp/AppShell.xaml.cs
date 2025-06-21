namespace HelloMauiApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(NavigationDetailPage), typeof(NavigationDetailPage));
    }
}