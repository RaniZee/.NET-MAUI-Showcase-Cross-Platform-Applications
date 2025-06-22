using System;
using System.Collections.Generic;
using System.Windows.Input;
using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.Services;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;

namespace TaskTrackerMAUI.ViewModels
{
    [QueryProperty(nameof(TaskId), "taskId")]
    public class TaskDetailViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly KanbanViewModel _kanbanViewModel;

        private TaskItem _task;
        public TaskItem Task { get => _task; set { if (SetProperty(ref _task, value)) { if (_task != null) { if (_task.DueDate.HasValue) { SelectedDate = _task.DueDate.Value.Date; SelectedTime = _task.DueDate.Value.TimeOfDay; IsDateSelected = true; } else { SelectedDate = DateTime.Today; SelectedTime = TimeSpan.FromHours(DateTime.Now.Hour); IsDateSelected = false; } if (_task.ReminderDateTime.HasValue) { SelectedReminderDate = _task.ReminderDateTime.Value.Date; SelectedReminderTime = _task.ReminderDateTime.Value.TimeOfDay; IsReminderDateSelected = true; } else { SelectedReminderDate = DateTime.Today; SelectedReminderTime = TimeSpan.FromHours(DateTime.Now.Hour); IsReminderDateSelected = false; } (DeleteTaskCommand as Command)?.ChangeCanExecute(); (ClearDueDateCommand as Command)?.ChangeCanExecute(); (ClearReminderDateTimeCommand as Command)?.ChangeCanExecute(); } } } }
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
        public List<Priority> PriorityOptions { get; }
        public List<TaskStatus> StatusOptions { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ClearDueDateCommand { get; }
        public ICommand ClearReminderDateTimeCommand { get; }

        public TaskDetailViewModel(IDataService dataService, KanbanViewModel kanbanViewModel)
        {
            _dataService = dataService; _kanbanViewModel = kanbanViewModel; Title = "Загрузка...";
            PriorityOptions = Enum.GetValues(typeof(Priority)).Cast<Priority>().ToList(); StatusOptions = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>().ToList();
            SaveTaskCommand = new Command(async () => await OnSaveTaskAsync(), () => !IsBusy && Task != null); DeleteTaskCommand = new Command(async () => await OnDeleteTaskAsync(), CanExecuteDelete); ClearDueDateCommand = new Command(OnClearDueDate, () => (Task?.DueDate.HasValue ?? false) && !IsBusy && IsDateSelected); ClearReminderDateTimeCommand = new Command(OnClearReminderDateTime, () => (Task?.ReminderDateTime.HasValue ?? false) && !IsBusy && IsReminderDateSelected);
            _selectedDate = DateTime.Today; _selectedTime = TimeSpan.FromHours(DateTime.Now.Hour); _isDateSelected = false; _selectedReminderDate = DateTime.Today; _selectedReminderTime = TimeSpan.FromHours(DateTime.Now.Hour); _isReminderDateSelected = false;
        }

        private void UpdateTaskDueDate() { if (Task != null) { if (IsDateSelected) Task.DueDate = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, SelectedTime.Hours, SelectedTime.Minutes, SelectedTime.Seconds); else Task.DueDate = null; Debug.WriteLine($"[DEBUG] Task.DueDate updated to: {Task.DueDate}"); (ClearDueDateCommand as Command)?.ChangeCanExecute(); } }
        private void OnClearDueDate() { if (Task != null) { IsDateSelected = false; SelectedDate = DateTime.Today; SelectedTime = TimeSpan.FromHours(DateTime.Now.Hour); Debug.WriteLine("[DEBUG] DueDate cleared via OnClearDueDate."); } }
        private void UpdateTaskReminderDateTime() { if (Task != null) { if (IsReminderDateSelected) Task.ReminderDateTime = new DateTime(SelectedReminderDate.Year, SelectedReminderDate.Month, SelectedReminderDate.Day, SelectedReminderTime.Hours, SelectedReminderTime.Minutes, SelectedReminderTime.Seconds); else Task.ReminderDateTime = null; Debug.WriteLine($"[DEBUG] Task.ReminderDateTime updated to: {Task.ReminderDateTime}"); (ClearReminderDateTimeCommand as Command)?.ChangeCanExecute(); } }
        private void OnClearReminderDateTime() { if (Task != null) { IsReminderDateSelected = false; SelectedReminderDate = DateTime.Today; SelectedReminderTime = TimeSpan.FromHours(DateTime.Now.Hour); Debug.WriteLine("[DEBUG] ReminderDateTime cleared via OnClearReminderDateTime."); } }
        private async Task LoadTaskAsync(string taskIdValue) { IsBusy = true; Debug.WriteLine($"[DEBUG] TaskDetailViewModel: LoadTaskAsync called with taskIdValue: {taskIdValue}"); try { TaskItem loadedTask; if (string.IsNullOrEmpty(taskIdValue) || taskIdValue.Equals("new", StringComparison.OrdinalIgnoreCase)) { loadedTask = new TaskItem(); Title = "Новая задача"; } else { if (int.TryParse(taskIdValue, out int id)) { loadedTask = await _dataService.GetTaskByIdAsync(id); if (loadedTask != null) Title = "Редактирование задачи"; else { Debug.WriteLine($"[ERROR] TaskDetailViewModel: Task with ID {id} not found in DB."); await Shell.Current.DisplayAlert("Ошибка", "Задача не найдена.", "OK"); loadedTask = new TaskItem(); Title = "Новая задача"; } } else { Debug.WriteLine($"[ERROR] TaskDetailViewModel: Invalid TaskId format: {taskIdValue}"); await Shell.Current.DisplayAlert("Ошибка", "Неверный ID задачи.", "OK"); loadedTask = new TaskItem(); Title = "Новая задача"; } } Task = loadedTask; } catch (Exception ex) { Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to load task. {ex.Message}"); await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить задачу.", "OK"); Task = new TaskItem(); Title = "Новая задача"; } finally { IsBusy = false; } }
        private async Task OnSaveTaskAsync() { if (Task == null || string.IsNullOrWhiteSpace(Task.Title)) { await Shell.Current.DisplayAlert("Ошибка", "Название задачи не может быть пустым.", "OK"); return; } if (IsBusy) return; IsBusy = true; try { if (IsDateSelected) UpdateTaskDueDate(); else Task.DueDate = null; if (IsReminderDateSelected) UpdateTaskReminderDateTime(); else Task.ReminderDateTime = null; if (Task.Id != 0) await CancelNotificationAsync(Task.Id); int savedTaskId = await _dataService.SaveTaskAsync(Task); if (Task.Id == 0 && savedTaskId != 0) Task.Id = savedTaskId; Debug.WriteLine($"[DEBUG] TaskDetailViewModel: Task '{Task.Title}' (ID: {Task.Id}) saved. DueDate: {Task.DueDate}, Reminder: {Task.ReminderDateTime}."); if (Task.ReminderDateTime.HasValue && Task.ReminderDateTime.Value > DateTime.Now && Task.Id != 0) await ScheduleNotificationAsync(Task); if (_kanbanViewModel.LoadTasksCommand.CanExecute(null)) _kanbanViewModel.LoadTasksCommand.Execute(null); await Shell.Current.GoToAsync(".."); } catch (Exception ex) { Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to save task. {ex.Message}"); await Shell.Current.DisplayAlert("Ошибка", "Не удалось сохранить задачу.", "OK"); } finally { IsBusy = false; } }
        private bool CanExecuteDelete() => Task != null && Task.Id != 0 && !IsBusy;
        private async Task OnDeleteTaskAsync() { if (!CanExecuteDelete()) return; bool confirm = await Shell.Current.DisplayAlert("Удаление задачи", $"Вы уверены, что хотите удалить задачу \"{Task.Title}\"?", "Удалить", "Отмена"); if (!confirm) return; IsBusy = true; try { int taskIdToDelete = Task.Id; await _dataService.DeleteTaskAsync(Task); Debug.WriteLine($"[DEBUG] TaskDetailViewModel: Task '{Task.Title}' (ID: {taskIdToDelete}) deleted from DB."); await CancelNotificationAsync(taskIdToDelete); if (_kanbanViewModel.LoadTasksCommand.CanExecute(null)) _kanbanViewModel.LoadTasksCommand.Execute(null); await Shell.Current.GoToAsync(".."); } catch (Exception ex) { Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to delete task. {ex.Message}"); await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить задачу.", "OK"); } finally { IsBusy = false; } }

        private async Task ScheduleNotificationAsync(TaskItem task)
        {
            if (task == null || !task.ReminderDateTime.HasValue || task.Id == 0) return;

            bool permissionGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            Debug.WriteLine($"[DEBUG] ScheduleNotificationAsync: Permission status: {permissionGranted}");

            if (permissionGranted)
            {
                var notification = new NotificationRequest
                {
                    NotificationId = task.Id,
                    Title = $"Напоминание: {task.Title}",
                    Description = string.IsNullOrWhiteSpace(task.Description) ? "Время выполнить задачу!" : (task.Description.Length > 50 ? task.Description.Substring(0, 50) + "..." : task.Description),
                    Subtitle = $"Срок: {task.DueDate?.ToString("dd.MM.yyyy HH:mm") ?? "не указан"}",
                    Schedule = new NotificationRequestSchedule { NotifyTime = task.ReminderDateTime.Value },
                    ReturningData = task.Id.ToString()
                };
                await LocalNotificationCenter.Current.Show(notification);
                Debug.WriteLine($"[DEBUG] Notification scheduled for Task ID {task.Id} at {task.ReminderDateTime.Value}");
            }
            else Debug.WriteLine("[WARNING] Notification permission not granted. Cannot schedule notification.");
        }

        private async Task CancelNotificationAsync(int taskId) { if (taskId == 0) return; bool canceled = LocalNotificationCenter.Current.Cancel(taskId); if (canceled) Debug.WriteLine($"[DEBUG] Notification CANCELED for Task ID {taskId}"); else Debug.WriteLine($"[DEBUG] Notification for Task ID {taskId} was not found or already triggered/canceled (no-op)."); }
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { if (SetProperty(ref _isBusy, value)) { (SaveTaskCommand as Command)?.ChangeCanExecute(); (DeleteTaskCommand as Command)?.ChangeCanExecute(); (ClearDueDateCommand as Command)?.ChangeCanExecute(); (ClearReminderDateTimeCommand as Command)?.ChangeCanExecute(); } } }
        private string _pageTitle;
        public string Title { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }
    }
}