using Microsoft.Extensions.Logging;

namespace HelloMauiApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public class DynamicXamlPage : ContentPage
    {
        public DynamicXamlPage()
        {
            string pageXaml =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
                <ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
                             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
                    <VerticalStackLayout Spacing=""10"" VerticalOptions=""Center"">
                        <Label x:Name=""dynamicLabel"" Text=""Initial Text from Dynamic XAML"" FontSize=""18"" HorizontalOptions=""Center""/>
                        <Button x:Name=""dynamicButton"" Text=""Change Label (Dynamic)"" HorizontalOptions=""Center""/>
                    </VerticalStackLayout>
                </ContentPage>";

            this.LoadFromXaml(pageXaml); // Загрузка XAML

            // Доступ к элементам, загруженным из XAML
            Label loadedLabel = this.FindByName<Label>("dynamicLabel");
            Button loadedButton = this.FindByName<Button>("dynamicButton");

            int dynamicCount = 0;
            if (loadedButton != null && loadedLabel != null)
            {
                loadedButton.Clicked += (sender, args) =>
                {
                    dynamicCount++;
                    loadedLabel.Text = $"Label changed {dynamicCount} times!";
                };
            }
        }
    }
}