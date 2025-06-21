namespace HelloMauiApp;

public partial class ResourcesDemoPage : ContentPage
{
    public ResourcesDemoPage()
    {
        InitializeComponent();

        if (!Application.Current.Resources.TryGetValue("DynamicTextColor", out _))
        {
            Application.Current.Resources["DynamicTextColor"] = Colors.Blue;
        }
    }

    private void ChangeColorToGreen_Clicked(object sender, EventArgs e)
    {
        Application.Current.Resources["DynamicTextColor"] = Colors.Green;
    }

    private void ChangeColorToRed_Clicked(object sender, EventArgs e)
    {
        Application.Current.Resources["DynamicTextColor"] = Colors.Red;
    }
}