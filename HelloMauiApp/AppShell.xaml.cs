namespace HelloMauiApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(NavigationDetailPage), typeof(NavigationDetailPage));
        Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
    }
}