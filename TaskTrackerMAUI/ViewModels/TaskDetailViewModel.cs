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
            set => SetProperty(ref _task, value);
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

        public List<Priority> PriorityOptions { get; }
        public List<TaskStatus> StatusOptions { get; }

        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public TaskDetailViewModel(IDataService dataService, KanbanViewModel kanbanViewModel)
        {
            _dataService = dataService;
            _kanbanViewModel = kanbanViewModel;

            Task = new TaskItem();    
            Title = "Новая задача";    

            PriorityOptions = Enum.GetValues(typeof(Priority)).Cast<Priority>().ToList();
            StatusOptions = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>().ToList();

            SaveTaskCommand = new Command(async () => await OnSaveTaskAsync());
            DeleteTaskCommand = new Command(async () => await OnDeleteTaskAsync(), CanExecuteDelete);    

        }

        private async Task LoadTaskAsync(string taskIdValue)
        {
            IsBusy = true;
            Debug.WriteLine($"[DEBUG] TaskDetailViewModel: LoadTaskAsync called with taskIdValue: {taskIdValue}");
            try
            {
                if (string.IsNullOrEmpty(taskIdValue) || taskIdValue.Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    Task = new TaskItem();   
                    Title = "Новая задача";
                    (DeleteTaskCommand as Command)?.ChangeCanExecute();     
                }
                else
                {
                    if (int.TryParse(taskIdValue, out int id))
                    {
                        var foundTask = await _dataService.GetTaskByIdAsync(id);
                        if (foundTask != null)
                        {
                            Task = foundTask;
                            Title = "Редактирование задачи";
                        }
                        else
                        {
                            Debug.WriteLine($"[ERROR] TaskDetailViewModel: Task with ID {id} not found in DB.");
                            await Shell.Current.DisplayAlert("Ошибка", "Задача не найдена.", "OK");
                            Task = new TaskItem();     
                            Title = "Новая задача";
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[ERROR] TaskDetailViewModel: Invalid TaskId format: {taskIdValue}");
                        await Shell.Current.DisplayAlert("Ошибка", "Неверный ID задачи.", "OK");
                        Task = new TaskItem(); Title = "Новая задача";
                    }
                     (DeleteTaskCommand as Command)?.ChangeCanExecute();     
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] TaskDetailViewModel: Failed to load task. {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить задачу.", "OK");
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
                await _dataService.SaveTaskAsync(Task);         
                Debug.WriteLine($"[DEBUG] TaskDetailViewModel: Task '{Task.Title}' (ID: {Task.Id}) saved to DB.");

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
            return Task != null && Task.Id != 0;        
        }

        private async Task OnDeleteTaskAsync()
        {
            if (!CanExecuteDelete()) return;

            bool confirm = await Shell.Current.DisplayAlert("Удаление задачи", $"Вы уверены, что хотите удалить задачу \"{Task.Title}\"?", "Удалить", "Отмена");
            if (!confirm) return;

            if (IsBusy) return;
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
                SetProperty(ref _isBusy, value);
                (SaveTaskCommand as Command)?.ChangeCanExecute();
                (DeleteTaskCommand as Command)?.ChangeCanExecute();
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