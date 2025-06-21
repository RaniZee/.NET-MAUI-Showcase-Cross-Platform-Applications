namespace HelloMauiApp;

[QueryProperty(nameof(MessageFromShell), "message")]
public partial class NavigationDetailPage : ContentPage
{
    private string _messageFromConstructor;

    private string _messageFromShell;
    public string MessageFromShell
    {
        get => _messageFromShell;
        set
        {
            _messageFromShell = Uri.UnescapeDataString(value ?? string.Empty);    
            OnPropertyChanged();         
            UpdateReceivedDataLabel();   
        }
    }

    public NavigationDetailPage()       
    {
        InitializeComponent();
        UpdateReceivedDataLabel();
    }

    public NavigationDetailPage(string message)    
    {
        InitializeComponent();
        _messageFromConstructor = message;
        UpdateReceivedDataLabel();
    }

    private void UpdateReceivedDataLabel()
    {
        if (!string.IsNullOrEmpty(_messageFromShell))
        {
            ReceivedDataLabel.Text = $"Received via Shell: {_messageFromShell}";
        }
        else if (!string.IsNullOrEmpty(_messageFromConstructor))
        {
            ReceivedDataLabel.Text = $"Received via Constructor: {_messageFromConstructor}";
        }
        else
        {
            ReceivedDataLabel.Text = "No data directly received.";
        }
    }

    private async void GoBack_Clicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1 && Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] is NavigationHomePage previousPage)
        {
            previousPage.UpdateResult(DataToReturnEntry.Text ?? "No data returned");
        }
        await Navigation.PopAsync();
    }

    private async void GoBackAndSendData_Clicked(object sender, EventArgs e)
    {
        string data = DataToReturnEntry.Text ?? "Data from MessagingCenter";
        MessagingCenter.Send(this, "DataFromDetail", data);
        await Navigation.PopAsync();
    }

    private async void GoToHomeShell_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(NavigationHomePage)}");
    }
}