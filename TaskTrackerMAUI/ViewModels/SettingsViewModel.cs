using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.Services;

namespace TaskTrackerMAUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly KanbanViewModel _kanbanViewModel;      
        private readonly IThemeService _themeService;

        public List<AppThemeOption> ThemeOptions { get; }

        private AppThemeOption _selectedThemeOption;
        public AppThemeOption SelectedThemeOption { get => _selectedThemeOption; set { if (SetProperty(ref _selectedThemeOption, value)) _themeService.SetTheme(value); } }

        public ObservableCollection<Category> Categories { get; }
        private string _newCategoryName;
        public string NewCategoryName { get => _newCategoryName; set => SetProperty(ref _newCategoryName, value); }
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand LoadDataCommand { get; }

        private string _pageTitle;
        public string Title { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public SettingsViewModel(IDataService dataService, IThemeService themeService, KanbanViewModel kanbanViewModel)
        {
            _dataService = dataService;
            _themeService = themeService;
            _kanbanViewModel = kanbanViewModel;   

            Title = "Настройки";
            ThemeOptions = Enum.GetValues(typeof(AppThemeOption)).Cast<AppThemeOption>().ToList();
            _selectedThemeOption = _themeService.CurrentAppThemeOption;

            Categories = new ObservableCollection<Category>();
            AddCategoryCommand = new Command(async () => await ExecuteAddCategoryCommand(), () => !string.IsNullOrWhiteSpace(NewCategoryName));
            DeleteCategoryCommand = new Command<Category>(async (category) => await ExecuteDeleteCategoryCommand(category));
            LoadDataCommand = new Command(async () => await ExecuteLoadCategoriesCommand());

            this.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(NewCategoryName)) (AddCategoryCommand as Command)?.ChangeCanExecute(); };
        }

        async Task ExecuteLoadCategoriesCommand()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Categories.Clear();
                var categoriesFromDb = await _dataService.GetAllCategoriesAsync();
                foreach (var cat in categoriesFromDb)
                {
                    Categories.Add(cat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to load categories: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить категории.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteAddCategoryCommand()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;

            if (Categories.Any(c => c.Name.Equals(NewCategoryName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Категория с таким именем уже существует.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var newCategory = new Category { Name = NewCategoryName.Trim() };
                await _dataService.SaveCategoryAsync(newCategory);
                Categories.Add(newCategory);    
                NewCategoryName = string.Empty;    

                if (_kanbanViewModel.LoadDataCommand.CanExecute(null)) _kanbanViewModel.LoadDataCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to add category: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось добавить категорию.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteDeleteCategoryCommand(Category category)
        {
            if (category == null) return;

            if (category.Name == "Без категории" && Categories.Count(c => c.Name == "Без категории") <= 1)
            {
                await Shell.Current.DisplayAlert("Действие запрещено", "Нельзя удалить основную категорию 'Без категории'.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Удаление категории", $"Вы уверены, что хотите удалить категорию \"{category.Name}\"? Все задачи в этой категории будут перемещены в 'Без категории'.", "Удалить", "Отмена");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                await _dataService.DeleteCategoryAsync(category);         
                Categories.Remove(category);

                if (_kanbanViewModel.LoadDataCommand.CanExecute(null)) _kanbanViewModel.LoadDataCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to delete category: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить категорию.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}