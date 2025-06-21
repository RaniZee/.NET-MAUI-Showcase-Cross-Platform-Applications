namespace HelloMauiApp;

public partial class NavigationHomePage : ContentPage
{
    public NavigationHomePage()
    {
        InitializeComponent();
    }

    private async void GoToDetailPage_Clicked(object sender, EventArgs e)
    {
        string dataToSend = "Hello from Home Page!";
        await Navigation.PushAsync(new NavigationDetailPage(dataToSend));
    }

    private async void GoToDetailPageShell_Clicked(object sender, EventArgs e)
    {
        string dataToSend = "Navigated via Shell URI!";
        await Shell.Current.GoToAsync($"{nameof(NavigationDetailPage)}?message={Uri.EscapeDataString(dataToSend)}");
    }


    private async void OpenModalPage_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new NavigationModalPage());
    }

    public void UpdateResult(string result)
    {
        ResultLabel.Text = $"Result from Detail Page: {result}";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MessagingCenter.Subscribe<NavigationDetailPage, string>(this, "DataFromDetail", (sender, arg) =>
        {
            ResultLabel.Text = $"Data via MessagingCenter: {arg}";
            MessagingCenter.Unsubscribe<NavigationDetailPage, string>(this, "DataFromDetail");    
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<NavigationDetailPage, string>(this, "DataFromDetail");
    }
}