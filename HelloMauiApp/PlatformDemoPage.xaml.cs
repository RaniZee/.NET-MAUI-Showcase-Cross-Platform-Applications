using HelloMauiApp.Services;

namespace HelloMauiApp;

public partial class PlatformDemoPage : ContentPage
{
    private readonly PlatformService _platformService;

    public PlatformDemoPage()
    {
        InitializeComponent();
        _platformService = new PlatformService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DisplayPlatformInfo();
    }

    private void DisplayPlatformInfo()
    {
        ConditionalCompilationLabel.Text = GetPlatformNameViaConditionalCompilation();
        PartialClassLabel.Text = _platformService.GetPlatformName();

        DeviceInfoModelLabel.Text = DeviceInfo.Current.Model;
        DeviceInfoPlatformLabel.Text = $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
        AppInfoVersionLabel.Text = AppInfo.Current.VersionString;

        var networkAccess = Connectivity.Current.NetworkAccess;
        ConnectivityLabel.Text = networkAccess.ToString();

        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConnectivityLabel.Text = e.NetworkAccess.ToString();
        });
    }

    private string GetPlatformNameViaConditionalCompilation()
    {
#if WINDOWS
        return "Windows (Conditional)";
#elif ANDROID
        return "Android (Conditional)";
#elif IOS
        return "iOS (Conditional)";
#elif MACCATALYST
        return "MacCatalyst (Conditional)";
#else
        return "Unknown Platform (Conditional)";
#endif
    }

    private void HapticFeedback_Clicked(object sender, EventArgs e)
    {
        if (HapticFeedback.Default.IsSupported)
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
    }

    private async void ShareText_Clicked(object sender, EventArgs e)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Share MAUI Info",
            Text = "Check out .NET MAUI!",
            Uri = "https://aka.ms/dotnet-maui"
        });
    }

    private async void OpenBrowser_Clicked(object sender, EventArgs e)
    {
        try
        {
            Uri uri = new Uri("https://learn.microsoft.com/dotnet/maui");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to open browser: {ex.Message}", "OK");
        }
    }
}