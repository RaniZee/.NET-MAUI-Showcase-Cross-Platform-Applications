using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;   

namespace TaskTrackerMAUI.Views
{
    public partial class KanbanPage : ContentPage
    {
        private KanbanViewModel _viewModel;

        public KanbanPage(KanbanViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private void DragGestureRecognizer_OnDragStarting(object sender, DragStartingEventArgs e)
        {
            TaskItem task = null;

            if (sender is VisualElement visualElementFromSender)
            {
                task = visualElementFromSender.BindingContext as TaskItem;
                Debug.WriteLine($"[DEBUG] DragStarting: Attempt 1 - Sender is VisualElement. BindingContext is TaskItem? {task != null}");
            }

            if (task == null && sender is DragGestureRecognizer recognizer)
            {
                if (recognizer.Parent is BindableObject parentElement)     
                {
                    task = parentElement.BindingContext as TaskItem;
                    Debug.WriteLine($"[DEBUG] DragStarting: Attempt 2 - Sender is DragGestureRecognizer. Parent's BindingContext is TaskItem? {task != null}");
                }
            }

            if (task != null)
            {
                _viewModel.DraggedTask = task;
                Debug.WriteLine($"[DEBUG] DragStarting: SUCCESS! Task '{task.Title}' (ID: {task.Id}) set as DraggedTask.");
            }
            else
            {
                _viewModel.DraggedTask = null;   
                Debug.WriteLine("[DEBUG] DragStarting: FAILED to get TaskItem from BindingContext through multiple attempts.");
                if (sender is Element el)         
                {
                    Debug.WriteLine($"[DEBUG] DragStarting: Sender type is {sender.GetType().FullName}. Its BindingContext type is {el.BindingContext?.GetType().FullName ?? "null"}.");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] DragStarting: Sender type is {sender.GetType().FullName}, not an Element.");
                }
            }
        }

        private void DropGestureRecognizer_OnDragOver(object sender, DragEventArgs e)
        {
            if (_viewModel.DraggedTask != null)        
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                if (sender is Border border)
                {
                    border.BackgroundColor = Colors.LightSlateGray;
                }
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;       
            }
        }

        private void DropGestureRecognizer_OnDragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.BackgroundColor = Colors.Transparent;
            }
        }

        private void NewTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] NewTasks_Drop: Triggered.");
            HandleDrop(TaskStatus.New, sender as Border);
        }

        private void InProgressTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] InProgressTasks_Drop: Triggered.");
            HandleDrop(TaskStatus.InProgress, sender as Border);
        }

        private void OnReviewTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] OnReviewTasks_Drop: Triggered.");
            HandleDrop(TaskStatus.OnReview, sender as Border);
        }

        private void CompletedTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] CompletedTasks_Drop: Triggered.");
            HandleDrop(TaskStatus.Completed, sender as Border);
        }

        private void HandleDrop(TaskStatus targetStatus, Border dropZoneBorder)
        {
            if (_viewModel.DraggedTask != null)
            {
                Debug.WriteLine($"[DEBUG] HandleDrop: Processing drop for task '{_viewModel.DraggedTask.Title}' to {targetStatus}");
                _viewModel.MoveTask(_viewModel.DraggedTask, targetStatus);
            }
            else
            {
                Debug.WriteLine("[DEBUG] HandleDrop: DraggedTask is null. No action taken.");
            }

            if (dropZoneBorder != null)
            {
                dropZoneBorder.BackgroundColor = Colors.Transparent;
            }
        }
    }
}