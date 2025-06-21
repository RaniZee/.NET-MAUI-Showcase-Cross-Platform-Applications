namespace HelloMauiApp;

public partial class DialogsDemoPage : ContentPage
{
    public DialogsDemoPage()
    {
        InitializeComponent();
    }

    private async void ShowSimpleAlert_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Simple Alert", "This is a simple alert message.", "OK");
        ResultLabel.Text = "Simple Alert was shown.";
    }

    private async void ShowConfirmationAlert_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Confirmation", "Do you want to proceed?", "Yes", "No");
        ResultLabel.Text = $"Confirmation result: {answer}";
    }

    private async void ShowActionSheet_Clicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Choose an option", "Cancel", "Delete", "Option 1", "Option 2", "Option 3");

        if (string.IsNullOrEmpty(action))
        {
            ResultLabel.Text = "Action Sheet was closed without a choice.";
        }
        else
        {
            ResultLabel.Text = $"Action Sheet result: {action}";
        }
    }

    private async void ShowPrompt_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Question", "What is your name?", "OK", "Cancel", "Your name...", 20, Keyboard.Text);

        if (string.IsNullOrEmpty(result))
        {
            ResultLabel.Text = "Prompt was cancelled or empty.";
        }
        else
        {
            ResultLabel.Text = $"Prompt result: {result}";
        }
    }
}