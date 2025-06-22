using System;
using System.Collections.ObjectModel;
using TaskTrackerMAUI.Models;
using System.Diagnostics;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;

namespace TaskTrackerMAUI.ViewModels
{
    public class KanbanViewModel : BaseViewModel
    {
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

        public KanbanViewModel()
        {
            NewTasks = new ObservableCollection<TaskItem>();
            InProgressTasks = new ObservableCollection<TaskItem>();
            OnReviewTasks = new ObservableCollection<TaskItem>();
            CompletedTasks = new ObservableCollection<TaskItem>();
            LoadSampleTasks();
        }

        void LoadSampleTasks()
        {
            NewTasks.Add(new TaskItem { Id = 1, Title = "Разработать главную страницу", Description = "Создать XAML и ViewModel для Канбан-доски.", Priority = Priority.High, Category = "Разработка", DueDate = DateTime.Now.AddDays(2), Status = TaskStatus.New });
            NewTasks.Add(new TaskItem { Id = 2, Title = "Настроить шрифты", Description = "Добавить шрифт Bahnschrift.", Priority = Priority.Medium, Category = "UI/UX", DueDate = DateTime.Now.AddDays(1), Status = TaskStatus.New });
            InProgressTasks.Add(new TaskItem { Id = 3, Title = "Создать модели данных", Description = "Определить классы TaskItem, Category, Priority.", Priority = Priority.High, Category = "Архитектура", Status = TaskStatus.InProgress, DueDate = DateTime.Now.AddDays(-1) });
            OnReviewTasks.Add(new TaskItem { Id = 4, Title = "Продумать цветовую схему", Description = "Выбрать основные цвета для темной и светлой темы.", Priority = Priority.Medium, Category = "UI/UX", Status = TaskStatus.OnReview, DueDate = DateTime.Now.AddDays(3) });
            CompletedTasks.Add(new TaskItem { Id = 5, Title = "Инициализировать проект", Description = "Создать новый .NET MAUI проект.", Priority = Priority.Low, Category = "Setup", Status = TaskStatus.Completed, DueDate = DateTime.Now.AddDays(-5) });
        }

        public void MoveTask(TaskItem taskToMove, TaskStatus newStatus)
        {
            if (taskToMove == null)
            {
                Debug.WriteLine("[ERROR] MoveTask: taskToMove is null. Operation cancelled.");
                DraggedTask = null;    
                return;
            }

            TaskStatus originalStatus = taskToMove.Status;       
            Debug.WriteLine($"[DEBUG] MoveTask: Moving '{taskToMove.Title}' (ID: {taskToMove.Id}). Original Status: {originalStatus}, New Status: {newStatus}");

            if (originalStatus == newStatus)
            {
                Debug.WriteLine($"[DEBUG] MoveTask: Task '{taskToMove.Title}' is already in the target status '{newStatus}'. No move needed.");
                DraggedTask = null;        
                return;
            }

            bool removed = false;
            switch (originalStatus)
            {
                case TaskStatus.New: removed = NewTasks.Remove(taskToMove); break;
                case TaskStatus.InProgress: removed = InProgressTasks.Remove(taskToMove); break;
                case TaskStatus.OnReview: removed = OnReviewTasks.Remove(taskToMove); break;
                case TaskStatus.Completed: removed = CompletedTasks.Remove(taskToMove); break;
                default: Debug.WriteLine($"[WARNING] MoveTask: Unknown original status {originalStatus} for task '{taskToMove.Title}'."); break;
            }

            if (removed)
            {
                Debug.WriteLine($"[DEBUG] MoveTask: Successfully removed '{taskToMove.Title}' from collection for status {originalStatus}.");
            }
            else
            {
                Debug.WriteLine($"[WARNING] MoveTask: FAILED to remove '{taskToMove.Title}' from collection for status {originalStatus}. It might lead to duplicates if added to another list.");
            }

            taskToMove.Status = newStatus;     
            taskToMove.ModifiedDate = DateTime.Now;

            switch (newStatus)
            {
                case TaskStatus.New: if (!NewTasks.Contains(taskToMove)) NewTasks.Add(taskToMove); break;
                case TaskStatus.InProgress: if (!InProgressTasks.Contains(taskToMove)) InProgressTasks.Add(taskToMove); break;
                case TaskStatus.OnReview: if (!OnReviewTasks.Contains(taskToMove)) OnReviewTasks.Add(taskToMove); break;
                case TaskStatus.Completed: if (!CompletedTasks.Contains(taskToMove)) CompletedTasks.Add(taskToMove); break;
            }
            Debug.WriteLine($"[DEBUG] MoveTask: Task '{taskToMove.Title}' added to collection for status {newStatus}. Collections counts: New={NewTasks.Count}, InProg={InProgressTasks.Count}, Review={OnReviewTasks.Count}, Done={CompletedTasks.Count}");

            DraggedTask = null;       
        }
    }
}