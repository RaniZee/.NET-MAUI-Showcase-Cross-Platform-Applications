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
        public ICommand LoadDataCommand { get; }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        private string _pageTitle;
        public string Title { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }

        private string _searchText;
        public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) ApplyFiltersAndSort(); } }

        public ObservableCollection<Category> CategoryFilterOptions { get; }
        private Category _selectedCategoryFilter;
        public Category SelectedCategoryFilter { get => _selectedCategoryFilter; set { if (SetProperty(ref _selectedCategoryFilter, value)) ApplyFiltersAndSort(); } }

        public ObservableCollection<PriorityDisplay> PriorityFilterOptions { get; }
        private PriorityDisplay _selectedPriorityFilter;
        public PriorityDisplay SelectedPriorityFilter { get => _selectedPriorityFilter; set { if (SetProperty(ref _selectedPriorityFilter, value)) ApplyFiltersAndSort(); } }

        public List<string> SortOptions { get; }
        private string _selectedSortOption;
        public string SelectedSortOption { get => _selectedSortOption; set { if (SetProperty(ref _selectedSortOption, value)) ApplyFiltersAndSort(); } }


        public KanbanViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Title = "Канбан-доска";
            _allTasksFromDb = new List<TaskItem>();
            NewTasks = new ObservableCollection<TaskItem>();
            InProgressTasks = new ObservableCollection<TaskItem>();
            OnReviewTasks = new ObservableCollection<TaskItem>();
            CompletedTasks = new ObservableCollection<TaskItem>();

            CategoryFilterOptions = new ObservableCollection<Category>();

            PriorityFilterOptions = new ObservableCollection<PriorityDisplay>
            {
                new PriorityDisplay { DisplayName = "Все приоритеты", Value = null }
            };
            foreach (Priority p in Enum.GetValues(typeof(Priority)))
            {
                PriorityFilterOptions.Add(new PriorityDisplay { DisplayName = p.ToString(), Value = p });
            }
            _selectedPriorityFilter = PriorityFilterOptions.First();

            SortOptions = new List<string> { "По дате создания", "По сроку выполнения", "По приоритету" };
            SelectedSortOption = SortOptions.First();

            NavigateToAddTaskCommand = new Command(async () => await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId=new"));
            NavigateToEditTaskCommand = new Command<TaskItem>(async (task) => { if (task != null) await Shell.Current.GoToAsync($"{nameof(Views.TaskDetailPage)}?taskId={task.Id}"); });
            LoadDataCommand = new Command(async () => await ExecuteLoadDataCommand());
        }

        async Task ExecuteLoadDataCommand()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                CategoryFilterOptions.Clear();
                CategoryFilterOptions.Add(new Category { Id = 0, Name = "Все категории" });
                var categoriesFromDb = await _dataService.GetAllCategoriesAsync();
                foreach (var cat in categoriesFromDb) CategoryFilterOptions.Add(cat);
                SelectedCategoryFilter = CategoryFilterOptions.First();

                _allTasksFromDb = await _dataService.GetAllTasksAsync() ?? new List<TaskItem>();
                ApplyFiltersAndSort();
            }
            catch (Exception ex) { Debug.WriteLine($"[ERROR] Failed to load data. {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private void ApplyFiltersAndSort()
        {
            if (_allTasksFromDb == null) return;
            IEnumerable<TaskItem> filteredTasks = _allTasksFromDb;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLowerInvariant();
                filteredTasks = filteredTasks.Where(t => (t.Title?.ToLowerInvariant().Contains(lowerSearchText) ?? false) || (t.Description?.ToLowerInvariant().Contains(lowerSearchText) ?? false));
            }
            if (SelectedPriorityFilter != null && SelectedPriorityFilter.Value.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == SelectedPriorityFilter.Value.Value);
            }
            if (SelectedCategoryFilter != null && SelectedCategoryFilter.Id != 0)
            {
                filteredTasks = filteredTasks.Where(t => t.CategoryId == SelectedCategoryFilter.Id);
            }

            switch (SelectedSortOption)
            {
                case "По сроку выполнения": filteredTasks = filteredTasks.OrderBy(t => t.DueDate.HasValue ? 0 : 1).ThenBy(t => t.DueDate); break;
                case "По приоритету": filteredTasks = filteredTasks.OrderByDescending(t => t.Priority); break;
                case "По дате создания": default: filteredTasks = filteredTasks.OrderByDescending(t => t.CreatedDate); break;
            }

            NewTasks.Clear(); InProgressTasks.Clear(); OnReviewTasks.Clear(); CompletedTasks.Clear();
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
        }

        public async Task MoveTaskAndSave(TaskItem taskToMove, TaskStatus newStatus)
        {
            if (taskToMove == null) { DraggedTask = null; return; }
            TaskStatus originalStatus = taskToMove.Status;
            if (originalStatus == newStatus) { DraggedTask = null; return; }
            var taskInAllList = _allTasksFromDb.FirstOrDefault(t => t.Id == taskToMove.Id);
            if (taskInAllList != null) { taskInAllList.Status = newStatus; taskInAllList.ModifiedDate = DateTime.Now; }
            bool removedFromMemory = false;
            switch (originalStatus) { case TaskStatus.New: removedFromMemory = NewTasks.Remove(taskToMove); break; case TaskStatus.InProgress: removedFromMemory = InProgressTasks.Remove(taskToMove); break; case TaskStatus.OnReview: removedFromMemory = OnReviewTasks.Remove(taskToMove); break; case TaskStatus.Completed: removedFromMemory = CompletedTasks.Remove(taskToMove); break; }
            if (removedFromMemory)
            {
                taskToMove.Status = newStatus; taskToMove.ModifiedDate = DateTime.Now;
                switch (newStatus) { case TaskStatus.New: NewTasks.Add(taskToMove); break; case TaskStatus.InProgress: InProgressTasks.Add(taskToMove); break; case TaskStatus.OnReview: OnReviewTasks.Add(taskToMove); break; case TaskStatus.Completed: CompletedTasks.Add(taskToMove); break; }
                try { await _dataService.SaveTaskAsync(taskToMove); } catch (Exception ex) { Debug.WriteLine($"[ERROR] MoveTaskAndSave: Failed to save moved task. {ex.Message}"); }
            }
            DraggedTask = null;
        }
    }

    public class PriorityDisplay
    {
        public string DisplayName { get; set; }
        public Priority? Value { get; set; }
        public override string ToString() => DisplayName;
    }
}