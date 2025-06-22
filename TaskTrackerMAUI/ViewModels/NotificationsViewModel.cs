using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.Services;
using Plugin.LocalNotification;

namespace TaskTrackerMAUI.ViewModels
{
    public class NotificationsViewModel : BaseViewModel      
    {
        private readonly IDataService _dataService;
        private readonly KanbanViewModel _kanbanViewModel;

        private ObservableCollection<TaskItem> _pendingNotifications;
        public ObservableCollection<TaskItem> PendingNotifications
        {
            get => _pendingNotifications;
            set => SetProperty(ref _pendingNotifications, value);
        }

        public ICommand LoadPendingNotificationsCommand { get; }
        public ICommand NavigateToTaskDetailCommand { get; }
        public ICommand CancelReminderCommand { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { if (SetProperty(ref _isBusy, value)) (CancelReminderCommand as Command)?.ChangeCanExecute(); }
        }

        private string _pageTitle;
        public string Title
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }
        public NotificationsViewModel(IDataService dataService, KanbanViewModel kanbanViewModel)
        {
            _dataService = dataService;
            _kanbanViewModel = kanbanViewModel;
            Title = "Запланированные напоминания";     
            PendingNotifications = new ObservableCollection<TaskItem>();

            LoadPendingNotificationsCommand = new Command(async () => await ExecuteLoadPendingNotificationsCommand());

            NavigateToTaskDetailCommand = new Command<TaskItem>(async (task) =>
            {
                if (task != null)
                {
                    await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId={task.Id}");
                }
            });

            CancelReminderCommand = new Command<TaskItem>(async (task) =>
            {
                if (task != null)
                {
                    await ExecuteCancelReminderCommand(task);
                }
            }, (task) => task != null && !IsBusy);   
        }

        async Task ExecuteLoadPendingNotificationsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            Debug.WriteLine("[DEBUG] NotificationsViewModel: Loading pending notifications...");

            try
            {
                PendingNotifications.Clear();
                var allTasks = await _dataService.GetAllTasksAsync();

                if (allTasks != null)
                {
                    var tasksWithReminders = allTasks
                        .Where(t => t.ReminderDateTime.HasValue && t.ReminderDateTime.Value > DateTime.Now)
                        .OrderBy(t => t.ReminderDateTime.Value)
                        .ToList();

                    foreach (var task in tasksWithReminders)
                    {
                        PendingNotifications.Add(task);
                    }
                    Debug.WriteLine($"[DEBUG] NotificationsViewModel: Loaded {PendingNotifications.Count} pending notifications.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] NotificationsViewModel: Failed to load pending notifications. {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить список напоминаний.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteCancelReminderCommand(TaskItem task)
        {
            if (task == null || !task.ReminderDateTime.HasValue) return;

            bool confirm = await Shell.Current.DisplayAlert("Отмена напоминания",
                $"Отменить напоминание для задачи \"{task.Title}\" в {task.ReminderDateTime:dd.MM.yyyy HH:mm}?",
                "Да, отменить", "Нет");

            if (!confirm) return;

            IsBusy = true;
            DateTime? originalReminderTime = task.ReminderDateTime;    

            try
            {
                int taskIdToCancel = task.Id;

                task.ReminderDateTime = null;
                await _dataService.SaveTaskAsync(task);

                LocalNotificationCenter.Current.Cancel(taskIdToCancel);
                Debug.WriteLine($"[DEBUG] NotificationsViewModel: Reminder for task '{task.Title}' (ID: {taskIdToCancel}) cancelled and DB updated.");

                PendingNotifications.Remove(task);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] NotificationsViewModel: Failed to cancel reminder. {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось отменить напоминание.", "OK");
                task.ReminderDateTime = originalReminderTime;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}