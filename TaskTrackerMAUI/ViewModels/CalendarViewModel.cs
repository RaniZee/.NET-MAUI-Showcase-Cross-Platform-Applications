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
    public class CalendarViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private List<TaskItem> _allTasks;

        public ObservableCollection<TaskItem> TasksForSelectedDate { get; } = new ObservableCollection<TaskItem>();

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    FilterTasksForSelectedDate();
                }
            }
        }

        public ICommand LoadTasksCommand { get; }
        public ICommand NavigateToTaskDetailCommand { get; }

        private string _pageTitle;
        public string Title
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public CalendarViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Title = "Календарь";    
            _allTasks = new List<TaskItem>();
            _selectedDate = DateTime.Today;

            LoadTasksCommand = new Command(async () => await ExecuteLoadTasksCommand());
            NavigateToTaskDetailCommand = new Command<TaskItem>(async (task) =>
            {
                if (task != null) await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId={task.Id}");
            });
        }

        private async Task ExecuteLoadTasksCommand()
        {
            try
            {
                _allTasks = await _dataService.GetAllTasksAsync();
                FilterTasksForSelectedDate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] CalendarViewModel: Failed to load tasks. {ex.Message}");
            }
        }

        private void FilterTasksForSelectedDate()
        {
            TasksForSelectedDate.Clear();
            if (_allTasks == null) return;

            var tasksOnDate = _allTasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == SelectedDate.Date)
                .OrderBy(t => t.DueDate.Value);

            foreach (var task in tasksOnDate)
            {
                TasksForSelectedDate.Add(task);
            }
            Debug.WriteLine($"[DEBUG] CalendarViewModel: Found {TasksForSelectedDate.Count} tasks for {SelectedDate.ToShortDateString()}");
        }
    }
}