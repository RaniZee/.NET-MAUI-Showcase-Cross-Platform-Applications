using Microsoft.Maui.Controls;

namespace HelloMauiApp
{
    public class CodePage : ContentPage
    {
        public CodePage()
        {
            Label welcomeLabel = new Label
            {
                Text = "Hello from C# code!",
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            Button codeButton = new Button
            {
                Text = "Click me (C#)",
                HorizontalOptions = LayoutOptions.Center
            };

            int codeCount = 0;
            codeButton.Clicked += (sender, args) =>
            {
                codeCount++;
                codeButton.Text = $"Clicked {codeCount} times (C#)";
            };

            VerticalStackLayout stackLayout = new VerticalStackLayout
            {
                Spacing = 15,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    welcomeLabel,
                    codeButton
                }
            };

            this.Content = stackLayout;
        }
    }
}