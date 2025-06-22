using System;
using System.Collections.Generic;
using System.Windows.Input;
using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.Services;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;

namespace TaskTrackerMAUI.ViewModels
{
    [QueryProperty(nameof(TaskId), "taskId")]
    public class TaskDetailViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly KanbanViewModel _kanbanViewModel;

        private TaskItem _task;
        public TaskItem Task
        {
            get => _task;
            set
            {
                if (SetProperty(ref _task, value))
                {
                    if (_task?.DueDate.HasValue ?? false)
                    {
                        SelectedDate = _task.DueDate.Value.Date;
                        SelectedTime = _task.DueDate.Value.TimeOfDay;
                        IsDateSelected = true;
                    }
                    else
                    {
                        SelectedDate = DateTime.Today;           
                        SelectedTime = TimeSpan.Zero;       
                        IsDateSelected = false;       
                    }
                     (DeleteTaskCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private string _taskIdString;
        public string TaskId
        {
            get => _taskIdString;
            set
            {
                _taskIdString = value;
                LoadTaskAsync(value);
            }
        }

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    IsDateSelected = true;      
                    UpdateTaskDueDate();
                }
            }
        }

        private TimeSpan _selectedTime;
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (SetProperty(ref _selectedTime, value))
                {
                    UpdateTaskDueDate();
                }
            }
        }

        private bool _isDateSelected;
        public bool IsDateSelected
        {
            get => _isDateSelected;
            set => SetProperty(ref _isDateSelected, value);
        }


        public List<Priority> PriorityOptions { get; }
        public List<TaskStatus> StatusOptions { get; }

        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ClearDueDateCommand { get; }      

        public TaskDetailViewModel(IDataService dataService, KanbanViewModel kanbanViewModel)
        {
            _dataService = dataService;
            _kanbanViewModel = kanbanViewModel;

            Title = "Загрузка...";   

            PriorityOptions = Enum.GetValues(typeof(Priority)).Cast<Priority>().ToList();
            StatusOptions = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>().ToList();

            SaveTaskCommand = new Command(async () => await OnSaveTaskAsync(), () => !IsBusy);  
            DeleteTaskCommand = new Command(async () => await OnDeleteTaskAsync(), CanExecuteDelete);
            ClearDueDateCommand = new Command(OnClearDueDate, () => Task?.DueDate.HasValue ?? false);

            _selectedDate = DateTime.Today;        
            _selectedTime = TimeSpan.FromHours(DateTime.Now.Hour);   
            IsDateSelected = false;         
        }

        private void UpdateTaskDueDate()
        {
            if (Task != null && IsDateSelected)       
            {
                Task.DueDate = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day,
                                            SelectedTime.Hours, SelectedTime.Minutes, SelectedTime.Seconds);
                Debug.WriteLine($"[DEBUG] Task.DueDate updated to: {Task.DueDate}");
                (ClearDueDateCommand as Command)?.ChangeCanExecute();
            }
            else if (Task != null && !IsDateSelected)       
            {
                Task.DueDate = null;
                (ClearDueDateCommand as Command)?.ChangeCanExecute();
            }
        }

        private void OnClearDueDate()
        {
            if (Task != null)
            {
                Task.DueDate = null;
                IsDateSelected = false;   
                SelectedDate = DateTime.Today;
                SelectedTime = TimeSpan.FromHours(DateTime.Now.Hour);
                Debug.WriteLine("[DEBUG] DueDate cleared.");
            }
        }


        private async Task LoadTaskAsync(string taskIdValue)
        {
            IsBusy = true;
            Debug.WriteLine($"[DEBUG] TaskDetailViewModel: LoadTaskAsync called with taskIdValue: {taskIdValue}");
            try
            {
                TaskItem loadedTask = null;      
                if (string.IsNullOrEmpty(taskIdValue) || taskIdValue.Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    loadedTask = new TaskItem();
                    Title = "Новая задача";
                }
                else
                {
                    if (int.TryParse(taskIdValue, out int id))
                    {
                        loadedTask = await _dataService.GetTaskByIdAsync(id);
                        if (loadedTask != null)
                        {
                            Title = "Редактирование задачи";
                        }
                        else
                        {
                            Debug.WriteLine($"[ERROR] TaskDetailViewModel: Task with ID {id} not found in DB.");
                            await Shell.Current.DisplayAlert("Ошибка", "Задача не найдена.", "OK");
                            loadedTask = new TaskItem(); Title = "Новая задача";
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[ERROR] TaskDetailViewModel: Invalid TaskId format: {taskIdValue}");
                        await Shell.Current.DisplayAlert("Ошибка", "Неверный ID задачи.", "OK");
                        loadedTask = new TaskItem(); Title = "Новая задача";
                    }
                }
                Task = loadedTask;          
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to load task. {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить задачу.", "OK");
                Task = new TaskItem(); Title = "Новая задача";     
            }
            finally { IsBusy = false; }
        }

        private async Task OnSaveTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Task.Title))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Название задачи не может быть пустым.", "OK");
                return;
            }
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (IsDateSelected) UpdateTaskDueDate(); else Task.DueDate = null;

                await _dataService.SaveTaskAsync(Task);
                Debug.WriteLine($"[DEBUG] TaskDetailViewModel: Task '{Task.Title}' (ID: {Task.Id}) saved to DB with DueDate: {Task.DueDate}.");

                if (_kanbanViewModel.LoadTasksCommand.CanExecute(null))
                {
                    _kanbanViewModel.LoadTasksCommand.Execute(null);
                }
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to save task. {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось сохранить задачу.", "OK");
            }
            finally { IsBusy = false; }
        }

        private bool CanExecuteDelete()
        {
            return Task != null && Task.Id != 0 && !IsBusy;
        }

        private async Task OnDeleteTaskAsync()
        {
            if (!CanExecuteDelete()) return;
            bool confirm = await Shell.Current.DisplayAlert("Удаление задачи", $"Вы уверены, что хотите удалить задачу \"{Task.Title}\"?", "Удалить", "Отмена");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                await _dataService.DeleteTaskAsync(Task);
                Debug.WriteLine($"[DEBUG] TaskDetailViewModel: Task '{Task.Title}' (ID: {Task.Id}) deleted from DB.");
                if (_kanbanViewModel.LoadTasksCommand.CanExecute(null))
                {
                    _kanbanViewModel.LoadTasksCommand.Execute(null);
                }
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to delete task. {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить задачу.", "OK");
            }
            finally { IsBusy = false; }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (SaveTaskCommand as Command)?.ChangeCanExecute();
                    (DeleteTaskCommand as Command)?.ChangeCanExecute();
                    (ClearDueDateCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private string _pageTitle;
        public string Title
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }
    }
}