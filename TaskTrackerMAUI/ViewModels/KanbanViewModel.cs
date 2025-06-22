using System;
using System.Collections.ObjectModel;
using System.Windows.Input;   
using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.Services;   
using System.Diagnostics;   
using Microsoft.Maui.Controls;   
using System.Linq;     
using System.Threading.Tasks;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;   

namespace TaskTrackerMAUI.ViewModels
{
    public class KanbanViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;

        public ObservableCollection<TaskItem> NewTasks { get; set; }
        public ObservableCollection<TaskItem> InProgressTasks { get; set; }
        public ObservableCollection<TaskItem> OnReviewTasks { get; set; }
        public ObservableCollection<TaskItem> CompletedTasks { get; set; }

        private TaskItem _draggedTask;
        public TaskItem DraggedTask
        {
            get => _draggedTask;
            set => SetProperty(ref _draggedTask, value);
        }

        public ICommand NavigateToAddTaskCommand { get; }
        public ICommand NavigateToEditTaskCommand { get; }
        public ICommand LoadTasksCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }    

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _pageTitle;          
        public string Title       
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public KanbanViewModel(IDataService dataService)
        {
            _dataService = dataService;

            Title = "Канбан-доска";   
            NewTasks = new ObservableCollection<TaskItem>();
            InProgressTasks = new ObservableCollection<TaskItem>();
            OnReviewTasks = new ObservableCollection<TaskItem>();
            CompletedTasks = new ObservableCollection<TaskItem>();

            NavigateToAddTaskCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId=new");
            });

            NavigateToEditTaskCommand = new Command<TaskItem>(async (task) =>
            {
                if (task != null)
                {
                    await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId={task.Id}");
                }
            });

            LoadTasksCommand = new Command(async () => await ExecuteLoadTasksCommand());

            NavigateToSettingsCommand = new Command(async () =>    
            {
                await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
            });
        }

        async Task ExecuteLoadTasksCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            Debug.WriteLine("[DEBUG] KanbanViewModel: Loading tasks from database...");

            try
            {
                NewTasks.Clear();
                InProgressTasks.Clear();
                OnReviewTasks.Clear();
                CompletedTasks.Clear();

                var tasks = await _dataService.GetAllTasksAsync();
                if (tasks != null && tasks.Any())
                {
                    foreach (var task in tasks)
                    {
                        switch (task.Status)
                        {
                            case TaskStatus.New: NewTasks.Add(task); break;
                            case TaskStatus.InProgress: InProgressTasks.Add(task); break;
                            case TaskStatus.OnReview: OnReviewTasks.Add(task); break;
                            case TaskStatus.Completed: CompletedTasks.Add(task); break;
                        }
                    }
                    Debug.WriteLine($"[DEBUG] KanbanViewModel: Loaded {tasks.Count} tasks.");
                }
                else
                {
                    Debug.WriteLine("[DEBUG] KanbanViewModel: No tasks found in database or database is empty.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] KanbanViewModel: Failed to load tasks. {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task MoveTaskAndSave(TaskItem taskToMove, TaskStatus newStatus)
        {
            if (taskToMove == null)
            {
                Debug.WriteLine("[ERROR] MoveTaskAndSave: taskToMove is null.");
                DraggedTask = null;
                return;
            }

            TaskStatus originalStatus = taskToMove.Status;
            Debug.WriteLine($"[DEBUG] MoveTaskAndSave: Moving '{taskToMove.Title}' (ID: {taskToMove.Id}). Original: {originalStatus}, New: {newStatus}");

            if (originalStatus == newStatus)
            {
                Debug.WriteLine($"[DEBUG] MoveTaskAndSave: Task already in target status.");
                DraggedTask = null;
                return;
            }

            bool removedFromMemory = false;
            switch (originalStatus)
            {
                case TaskStatus.New: removedFromMemory = NewTasks.Remove(taskToMove); break;
                case TaskStatus.InProgress: removedFromMemory = InProgressTasks.Remove(taskToMove); break;
                case TaskStatus.OnReview: removedFromMemory = OnReviewTasks.Remove(taskToMove); break;
                case TaskStatus.Completed: removedFromMemory = CompletedTasks.Remove(taskToMove); break;
            }

            if (removedFromMemory)
            {
                taskToMove.Status = newStatus;
                taskToMove.ModifiedDate = DateTime.Now;
                switch (newStatus)
                {
                    case TaskStatus.New: NewTasks.Add(taskToMove); break;
                    case TaskStatus.InProgress: InProgressTasks.Add(taskToMove); break;
                    case TaskStatus.OnReview: OnReviewTasks.Add(taskToMove); break;
                    case TaskStatus.Completed: CompletedTasks.Add(taskToMove); break;
                }
                Debug.WriteLine($"[DEBUG] MoveTaskAndSave: Task moved in memory collections.");

                try
                {
                    await _dataService.SaveTaskAsync(taskToMove);
                    Debug.WriteLine($"[DEBUG] MoveTaskAndSave: Task '{taskToMove.Title}' (ID: {taskToMove.Id}) saved to DB with new status {newStatus}.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] MoveTaskAndSave: Failed to save moved task to DB. {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"[WARNING] MoveTaskAndSave: FAILED to remove task from memory collection. DB save skipped.");
            }
            DraggedTask = null;
        }
    }
}