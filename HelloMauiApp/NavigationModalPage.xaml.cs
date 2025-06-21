namespace HelloMauiApp;

public partial class NavigationModalPage : ContentPage
{
    public NavigationModalPage()
    {
        InitializeComponent();
    }

    private async void CloseModal_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}