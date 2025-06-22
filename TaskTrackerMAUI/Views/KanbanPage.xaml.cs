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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("[DEBUG] KanbanPage: OnAppearing called.");
            if (_viewModel.LoadTasksCommand.CanExecute(null))
            {
                _viewModel.LoadTasksCommand.Execute(null);
            }
        }

        private void DragGestureRecognizer_OnDragStarting(object sender, DragStartingEventArgs e)
        {
            TaskItem task = null;
            if (sender is VisualElement visualElementFromSender) task = visualElementFromSender.BindingContext as TaskItem;
            if (task == null && sender is DragGestureRecognizer recognizer && recognizer.Parent is BindableObject parentElement) task = parentElement.BindingContext as TaskItem;

            if (task != null)
            {
                _viewModel.DraggedTask = task;
                Debug.WriteLine($"[DEBUG] DragStarting: SUCCESS! Task '{task.Title}' (ID: {task.Id}) set as DraggedTask.");
            }
            else
            {
                _viewModel.DraggedTask = null;
                Debug.WriteLine("[DEBUG] DragStarting: FAILED to get TaskItem from BindingContext.");
                if (sender is Element el) Debug.WriteLine($"[DEBUG] DragStarting: Sender type is {sender.GetType().FullName}. Its BC type is {el.BindingContext?.GetType().FullName ?? "null"}.");
                else Debug.WriteLine($"[DEBUG] DragStarting: Sender type is {sender.GetType().FullName}, not an Element.");
            }
        }

        private void DropGestureRecognizer_OnDragOver(object sender, DragEventArgs e)
        {
            if (_viewModel.DraggedTask != null)
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                if (sender is Border border) border.BackgroundColor = Colors.LightSlateGray;
            }
            else e.AcceptedOperation = DataPackageOperation.None;
        }

        private void DropGestureRecognizer_OnDragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border) border.BackgroundColor = Colors.Transparent;
        }

        private async void NewTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] NewTasks_Drop: Triggered.");
            await HandleDropAsync(TaskStatus.New, sender as Border);
        }

        private async void InProgressTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] InProgressTasks_Drop: Triggered.");
            await HandleDropAsync(TaskStatus.InProgress, sender as Border);
        }

        private async void OnReviewTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] OnReviewTasks_Drop: Triggered.");
            await HandleDropAsync(TaskStatus.OnReview, sender as Border);
        }

        private async void CompletedTasks_Drop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("[DEBUG] CompletedTasks_Drop: Triggered.");
            await HandleDropAsync(TaskStatus.Completed, sender as Border);
        }

        private async Task HandleDropAsync(TaskStatus targetStatus, Border dropZoneBorder)
        {
            if (_viewModel.DraggedTask != null)
            {
                Debug.WriteLine($"[DEBUG] HandleDropAsync: Processing drop for task '{_viewModel.DraggedTask.Title}' to {targetStatus}");
                await _viewModel.MoveTaskAndSave(_viewModel.DraggedTask, targetStatus);
            }
            else Debug.WriteLine("[DEBUG] HandleDropAsync: DraggedTask is null. No action taken.");

            if (dropZoneBorder != null) dropZoneBorder.BackgroundColor = Colors.Transparent;
        }
    }
}