using System;
using System.Collections.Generic;   
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

        private List<TaskItem> _allTasksFromDb;

        private TaskItem _draggedTask;
        public TaskItem DraggedTask { get => _draggedTask; set => SetProperty(ref _draggedTask, value); }

        public ICommand NavigateToAddTaskCommand { get; }
        public ICommand NavigateToEditTaskCommand { get; }
        public ICommand LoadTasksCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToNotificationsCommand { get; }
        public ICommand FilterTasksCommand { get; }      

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        private string _pageTitle;
        public string Title { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public List<Priority?> PriorityFilterOptions { get; }      
        private Priority? _selectedPriorityFilter;
        public Priority? SelectedPriorityFilter
        {
            get => _selectedPriorityFilter;
            set
            {
                if (SetProperty(ref _selectedPriorityFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public KanbanViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Title = "Канбан-доска";
            _allTasksFromDb = new List<TaskItem>();
            NewTasks = new ObservableCollection<TaskItem>();
            InProgressTasks = new ObservableCollection<TaskItem>();
            OnReviewTasks = new ObservableCollection<TaskItem>();
            CompletedTasks = new ObservableCollection<TaskItem>();

            PriorityFilterOptions = new List<Priority?> { null };  
            PriorityFilterOptions.AddRange(Enum.GetValues(typeof(Priority)).Cast<Priority>().Select(p => (Priority?)p));
            _selectedPriorityFilter = null;    

            NavigateToAddTaskCommand = new Command(async () => await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId=new"));
            NavigateToEditTaskCommand = new Command<TaskItem>(async (task) => { if (task != null) await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId={task.Id}"); });
            LoadTasksCommand = new Command(async () => await ExecuteLoadTasksCommand());
            NavigateToSettingsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.SettingsPage)));
            NavigateToNotificationsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.NotificationsPage)));
            FilterTasksCommand = new Command(ApplyFilters);        
        }

        async Task ExecuteLoadTasksCommand()
        {
            if (IsBusy) return;
            IsBusy = true;
            Debug.WriteLine("[DEBUG] KanbanViewModel: Loading ALL tasks from database...");
            try
            {
                _allTasksFromDb = await _dataService.GetAllTasksAsync() ?? new List<TaskItem>();
                Debug.WriteLine($"[DEBUG] KanbanViewModel: Loaded {_allTasksFromDb.Count} total tasks from DB.");
                ApplyFilters();       
            }
            catch (Exception ex) { Debug.WriteLine($"[ERROR] KanbanViewModel: Failed to load tasks. {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private void ApplyFilters()
        {
            if (_allTasksFromDb == null) return;
            if (IsBusy) return;      

            Debug.WriteLine($"[DEBUG] KanbanViewModel: Applying filters. Search: '{SearchText}', Priority: {SelectedPriorityFilter?.ToString() ?? "All"}");

            IsBusy = true;           

            IEnumerable<TaskItem> filteredTasks = _allTasksFromDb;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLowerInvariant();
                filteredTasks = filteredTasks.Where(t =>
                    (t.Title?.ToLowerInvariant().Contains(lowerSearchText) ?? false) ||
                    (t.Description?.ToLowerInvariant().Contains(lowerSearchText) ?? false));
            }

            if (SelectedPriorityFilter.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == SelectedPriorityFilter.Value);
            }

            NewTasks.Clear();
            InProgressTasks.Clear();
            OnReviewTasks.Clear();
            CompletedTasks.Clear();

            foreach (var task in filteredTasks.ToList())      
            {
                switch (task.Status)
                {
                    case TaskStatus.New: NewTasks.Add(task); break;
                    case TaskStatus.InProgress: InProgressTasks.Add(task); break;
                    case TaskStatus.OnReview: OnReviewTasks.Add(task); break;
                    case TaskStatus.Completed: CompletedTasks.Add(task); break;
                }
            }
            Debug.WriteLine($"[DEBUG] KanbanViewModel: Filters applied. New: {NewTasks.Count}, InProg: {InProgressTasks.Count}, Review: {OnReviewTasks.Count}, Done: {CompletedTasks.Count}");
            IsBusy = false;
        }

        public async Task MoveTaskAndSave(TaskItem taskToMove, TaskStatus newStatus)
        {
            if (taskToMove == null) { DraggedTask = null; return; }
            TaskStatus originalStatus = taskToMove.Status;
            if (originalStatus == newStatus) { DraggedTask = null; return; }

            var taskInAllList = _allTasksFromDb.FirstOrDefault(t => t.Id == taskToMove.Id);
            if (taskInAllList != null)
            {
                taskInAllList.Status = newStatus;
                taskInAllList.ModifiedDate = DateTime.Now;
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

                try
                {
                    await _dataService.SaveTaskAsync(taskToMove);
                    Debug.WriteLine($"[DEBUG] MoveTaskAndSave: Task '{taskToMove.Title}' (ID: {taskToMove.Id}) saved to DB with new status {newStatus}.");
                }
                catch (Exception ex) { Debug.WriteLine($"[ERROR] MoveTaskAndSave: Failed to save moved task to DB. {ex.Message}"); }
            }
            else { Debug.WriteLine($"[WARNING] MoveTaskAndSave: FAILED to remove task from memory collection. DB save skipped. This should not happen if DraggedTask was from one of the collections."); }

            DraggedTask = null;
        }
    }
}