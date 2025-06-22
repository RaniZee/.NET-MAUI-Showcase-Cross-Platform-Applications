using Microsoft.Maui.ApplicationModel;    
using Microsoft.Maui.Controls;          
using TaskTrackerMAUI.Models;         
using System.Diagnostics;

namespace TaskTrackerMAUI.Services
{
    public interface IThemeService
    {
        AppThemeOption CurrentAppThemeOption { get; }
        void InitializeTheme();
        void SetTheme(AppThemeOption themeOption);
    }

    public class ThemeService : IThemeService
    {
        private const string ThemePreferenceKey = "AppThemePreference";
        private AppThemeOption _currentAppThemeOption;

        public AppThemeOption CurrentAppThemeOption
        {
            get => _currentAppThemeOption;
            private set
            {
                if (_currentAppThemeOption != value)
                {
                    _currentAppThemeOption = value;
                }
            }
        }


        public ThemeService()
        {
            LoadThemePreference();
        }

        public void InitializeTheme()
        {
            Debug.WriteLine($"[DEBUG] ThemeService: Initializing theme. Current loaded preference: {CurrentAppThemeOption}");
            ApplyTheme(CurrentAppThemeOption);      
        }


        private void LoadThemePreference()
        {
            var themeString = Preferences.Get(ThemePreferenceKey, AppThemeOption.System.ToString());
            if (Enum.TryParse<AppThemeOption>(themeString, out var themeOption))
            {
                CurrentAppThemeOption = themeOption;
            }
            else
            {
                CurrentAppThemeOption = AppThemeOption.System;  
            }
            Debug.WriteLine($"[DEBUG] ThemeService: Loaded theme preference: {CurrentAppThemeOption}");
        }

        public void SetTheme(AppThemeOption themeOption)
        {
            if (Application.Current == null)
            {
                Debug.WriteLine("[ERROR] ThemeService: Application.Current is null. Cannot set theme yet.");
                return;
            }

            CurrentAppThemeOption = themeOption;
            Preferences.Set(ThemePreferenceKey, themeOption.ToString());
            Debug.WriteLine($"[DEBUG] ThemeService: Saved theme preference: {themeOption}");
            ApplyTheme(themeOption);
        }

        private void ApplyTheme(AppThemeOption themeOption)
        {
            if (Application.Current == null) return;

            AppTheme osAppTheme;
            switch (themeOption)
            {
                case AppThemeOption.Light:
                    osAppTheme = AppTheme.Light;
                    break;
                case AppThemeOption.Dark:
                    osAppTheme = AppTheme.Dark;
                    break;
                case AppThemeOption.System:
                default:
                    osAppTheme = AppTheme.Unspecified;    
                    break;
            }

            Application.Current.UserAppTheme = osAppTheme;
            Debug.WriteLine($"[DEBUG] ThemeService: Applied OS AppTheme: {osAppTheme} (based on option: {themeOption})");
        }
    }
}