using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.Services;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;

namespace TaskTrackerMAUI.ViewModels
{
    [QueryProperty(nameof(TaskId), "taskId")]
    public class TaskDetailViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly KanbanViewModel _kanbanViewModel;

        private TaskItem _task;
        public TaskItem Task { get => _task; set => SetProperty(ref _task, value); }
        private string _taskIdString;
        public string TaskId { get => _taskIdString; set { _taskIdString = value; LoadTaskAsync(value); } }
        private DateTime _selectedDate;
        public DateTime SelectedDate { get => _selectedDate; set { if (SetProperty(ref _selectedDate, value)) { IsDateSelected = true; UpdateTaskDueDate(); } } }
        private TimeSpan _selectedTime;
        public TimeSpan SelectedTime { get => _selectedTime; set { if (SetProperty(ref _selectedTime, value)) UpdateTaskDueDate(); } }
        private bool _isDateSelected;
        public bool IsDateSelected { get => _isDateSelected; set { if (SetProperty(ref _isDateSelected, value)) UpdateTaskDueDate(); } }
        private DateTime _selectedReminderDate;
        public DateTime SelectedReminderDate { get => _selectedReminderDate; set { if (SetProperty(ref _selectedReminderDate, value)) { IsReminderDateSelected = true; UpdateTaskReminderDateTime(); } } }
        private TimeSpan _selectedReminderTime;
        public TimeSpan SelectedReminderTime { get => _selectedReminderTime; set { if (SetProperty(ref _selectedReminderTime, value)) UpdateTaskReminderDateTime(); } }
        private bool _isReminderDateSelected;
        public bool IsReminderDateSelected { get => _isReminderDateSelected; set { if (SetProperty(ref _isReminderDateSelected, value)) UpdateTaskReminderDateTime(); } }
        public ObservableCollection<Category> CategoryOptions { get; }
        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { if (SetProperty(ref _selectedCategory, value)) { if (Task != null && value != null) { Task.CategoryId = value.Id; Task.CategoryName = value.Name; } } } }
        public List<Priority> PriorityOptions { get; }
        public List<TaskStatus> StatusOptions { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ClearDueDateCommand { get; }
        public ICommand ClearReminderDateTimeCommand { get; }

        public TaskDetailViewModel(IDataService dataService, KanbanViewModel kanbanViewModel)
        {
            _dataService = dataService; _kanbanViewModel = kanbanViewModel; Title = "Загрузка...";
            CategoryOptions = new ObservableCollection<Category>();
            PriorityOptions = Enum.GetValues(typeof(Priority)).Cast<Priority>().ToList();
            StatusOptions = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>().ToList();
            SaveTaskCommand = new Command(async () => await OnSaveTaskAsync(), () => !IsBusy && Task != null); DeleteTaskCommand = new Command(async () => await OnDeleteTaskAsync(), CanExecuteDelete); ClearDueDateCommand = new Command(OnClearDueDate, () => (IsDateSelected) && !IsBusy); ClearReminderDateTimeCommand = new Command(OnClearReminderDateTime, () => (IsReminderDateSelected) && !IsBusy);
            _selectedDate = DateTime.Today; _selectedTime = TimeSpan.FromHours(DateTime.Now.Hour); _isDateSelected = false; _selectedReminderDate = DateTime.Today; _selectedReminderTime = TimeSpan.FromHours(DateTime.Now.Hour); _isReminderDateSelected = false;
        }

        private async Task LoadTaskAsync(string taskIdValue)
        {
            IsBusy = true; await LoadCategoriesAsync();
            try { TaskItem loadedTask; if (string.IsNullOrEmpty(taskIdValue) || taskIdValue.Equals("new", StringComparison.OrdinalIgnoreCase)) { loadedTask = new TaskItem(); Title = "Новая задача"; } else { if (int.TryParse(taskIdValue, out int id)) { loadedTask = await _dataService.GetTaskByIdAsync(id); if (loadedTask != null) Title = "Редактирование задачи"; else { loadedTask = new TaskItem(); Title = "Новая задача"; } } else { loadedTask = new TaskItem(); Title = "Новая задача"; } } SetupPropertiesFromTask(loadedTask); }
            catch (Exception ex) { Debug.WriteLine($"[ERROR] Failed to load task: {ex.Message}"); SetupPropertiesFromTask(new TaskItem()); Title = "Ошибка загрузки"; }
            finally { IsBusy = false; }
        }

        private void SetupPropertiesFromTask(TaskItem task)
        {
            Task = task; if (Task.CategoryId > 0) SelectedCategory = CategoryOptions.FirstOrDefault(c => c.Id == Task.CategoryId); else SelectedCategory = CategoryOptions.FirstOrDefault(c => c.Name == "Без категории") ?? CategoryOptions.FirstOrDefault();
            if (Task.DueDate.HasValue) { SelectedDate = Task.DueDate.Value.Date; SelectedTime = Task.DueDate.Value.TimeOfDay; IsDateSelected = true; } else { SelectedDate = DateTime.Today; SelectedTime = TimeSpan.FromHours(DateTime.Now.Hour); IsDateSelected = false; }
            if (Task.ReminderDateTime.HasValue) { SelectedReminderDate = Task.ReminderDateTime.Value.Date; SelectedReminderTime = Task.ReminderDateTime.Value.TimeOfDay; IsReminderDateSelected = true; } else { SelectedReminderDate = DateTime.Today; SelectedReminderTime = TimeSpan.FromHours(DateTime.Now.Hour); IsReminderDateSelected = false; }
            (DeleteTaskCommand as Command)?.ChangeCanExecute(); (ClearDueDateCommand as Command)?.ChangeCanExecute(); (ClearReminderDateTimeCommand as Command)?.ChangeCanExecute();
        }

        private async Task LoadCategoriesAsync() { try { CategoryOptions.Clear(); var categories = await _dataService.GetAllCategoriesAsync(); foreach (var category in categories) CategoryOptions.Add(category); } catch (Exception ex) { Debug.WriteLine($"[ERROR] Failed to load categories: {ex.Message}"); } }
        private void UpdateTaskDueDate() { if (Task != null) { Task.DueDate = IsDateSelected ? new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, SelectedTime.Hours, SelectedTime.Minutes, SelectedTime.Seconds) : null; (ClearDueDateCommand as Command)?.ChangeCanExecute(); } }
        private void OnClearDueDate() { IsDateSelected = false; }
        private void UpdateTaskReminderDateTime() { if (Task != null) { Task.ReminderDateTime = IsReminderDateSelected ? new DateTime(SelectedReminderDate.Year, SelectedReminderDate.Month, SelectedReminderDate.Day, SelectedReminderTime.Hours, SelectedReminderTime.Minutes, SelectedReminderTime.Seconds) : null; (ClearReminderDateTimeCommand as Command)?.ChangeCanExecute(); } }
        private void OnClearReminderDateTime() { IsReminderDateSelected = false; }

        private async Task OnSaveTaskAsync()
        {
            if (Task == null || string.IsNullOrWhiteSpace(Task.Title)) { await Shell.Current.DisplayAlert("Ошибка", "Название задачи не может быть пустым.", "OK"); return; }
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (Task.Id != 0) await CancelNotificationAsync(Task.Id);
                int savedTaskId = await _dataService.SaveTaskAsync(Task);
                if (Task.Id == 0 && savedTaskId != 0) Task.Id = savedTaskId;
                if (Task.ReminderDateTime.HasValue && Task.ReminderDateTime.Value > DateTime.Now && Task.Id != 0) await ScheduleNotificationAsync(Task);
                if (_kanbanViewModel.LoadDataCommand.CanExecute(null)) _kanbanViewModel.LoadDataCommand.Execute(null);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex) { Debug.WriteLine($"[ERROR] Failed to save task: {ex.Message}"); await Shell.Current.DisplayAlert("Ошибка", "Не удалось сохранить задачу.", "OK"); }
            finally { IsBusy = false; }
        }

        private bool CanExecuteDelete() => Task != null && Task.Id != 0 && !IsBusy;

        private async Task OnDeleteTaskAsync()
        {
            if (!CanExecuteDelete()) return;
            bool confirm = await Shell.Current.DisplayAlert("Удаление задачи", $"Вы уверены, что хотите удалить задачу \"{Task.Title}\"?", "Удалить", "Отмена");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                int taskIdToDelete = Task.Id; await _dataService.DeleteTaskAsync(Task); await CancelNotificationAsync(taskIdToDelete);
                if (_kanbanViewModel.LoadDataCommand.CanExecute(null)) _kanbanViewModel.LoadDataCommand.Execute(null);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex) { Debug.WriteLine($"[ERROR] Failed to delete task: {ex.Message}"); await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить задачу.", "OK"); }
            finally { IsBusy = false; }
        }

        private async Task ScheduleNotificationAsync(TaskItem task)
        {
            if (task == null || !task.ReminderDateTime.HasValue || task.Id == 0) return;
            bool permissionGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            if (permissionGranted)
            {
                var notification = new NotificationRequest { NotificationId = task.Id, Title = $"Напоминание: {task.Title}", Description = string.IsNullOrWhiteSpace(task.Description) ? "Время выполнить задачу!" : (task.Description.Length > 50 ? task.Description.Substring(0, 50) + "..." : task.Description), Subtitle = $"Срок: {task.DueDate?.ToString("dd.MM.yyyy HH:mm") ?? "не указан"}", Schedule = new NotificationRequestSchedule { NotifyTime = task.ReminderDateTime.Value }, ReturningData = task.Id.ToString() };
                await LocalNotificationCenter.Current.Show(notification);
            }
            else Debug.WriteLine("[WARNING] Notification permission not granted.");
        }

        private async Task CancelNotificationAsync(int taskId) { if (taskId == 0) return; LocalNotificationCenter.Current.Cancel(taskId); }
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { if (SetProperty(ref _isBusy, value)) { (SaveTaskCommand as Command)?.ChangeCanExecute(); (DeleteTaskCommand as Command)?.ChangeCanExecute(); (ClearDueDateCommand as Command)?.ChangeCanExecute(); (ClearReminderDateTimeCommand as Command)?.ChangeCanExecute(); } } }
        private string _pageTitle;
        public string Title { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }
    }
}