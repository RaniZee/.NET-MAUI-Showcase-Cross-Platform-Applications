using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;   
using TaskTrackerMAUI.Models;     
using TaskTrackerMAUI.Services;   
using System.Diagnostics;

namespace TaskTrackerMAUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel      
    {
        private readonly IThemeService _themeService;

        public List<AppThemeOption> ThemeOptions { get; }

        private AppThemeOption _selectedThemeOption;
        public AppThemeOption SelectedThemeOption
        {
            get => _selectedThemeOption;
            set
            {
                if (SetProperty(ref _selectedThemeOption, value))
                {
                    Debug.WriteLine($"[DEBUG] SettingsViewModel: SelectedThemeOption changed to {value}");
                    _themeService.SetTheme(value);
                }
            }
        }

        private string _pageTitle;                 
        public string Title      
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public SettingsViewModel(IThemeService themeService)
        {
            _themeService = themeService;
            Title = "Настройки";     

            ThemeOptions = Enum.GetValues(typeof(AppThemeOption)).Cast<AppThemeOption>().ToList();

            _selectedThemeOption = _themeService.CurrentAppThemeOption;
            Debug.WriteLine($"[DEBUG] SettingsViewModel: Initialized with theme: {_selectedThemeOption}");
        }
    }
}