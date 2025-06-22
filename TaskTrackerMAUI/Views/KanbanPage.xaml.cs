using TaskTrackerMAUI.Models;
using TaskTrackerMAUI.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;

namespace TaskTrackerMAUI.Views
{
    public partial class KanbanPage : ContentPage
    {
        private readonly KanbanViewModel _viewModel;

        public KanbanPage(KanbanViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("[DEBUG] KanbanPage: OnAppearing called.");
            if (_viewModel.LoadDataCommand.CanExecute(null))
            {
                _viewModel.LoadDataCommand.Execute(null);
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
            else { _viewModel.DraggedTask = null; Debug.WriteLine("[DEBUG] DragStarting: FAILED to get TaskItem from BindingContext."); }
        }

        private void DropGestureRecognizer_OnDragOver(object sender, DragEventArgs e)
        {
            if (_viewModel.DraggedTask != null) { e.AcceptedOperation = DataPackageOperation.Copy; if (sender is Border border) border.BackgroundColor = Colors.LightSlateGray; }
            else e.AcceptedOperation = DataPackageOperation.None;
        }

        private void DropGestureRecognizer_OnDragLeave(object sender, DragEventArgs e) { if (sender is Border border) border.BackgroundColor = Colors.Transparent; }
        private async void NewTasks_Drop(object sender, DropEventArgs e) { await HandleDropAsync(TaskStatus.New, sender as Border); }
        private async void InProgressTasks_Drop(object sender, DropEventArgs e) { await HandleDropAsync(TaskStatus.InProgress, sender as Border); }
        private async void OnReviewTasks_Drop(object sender, DropEventArgs e) { await HandleDropAsync(TaskStatus.OnReview, sender as Border); }
        private async void CompletedTasks_Drop(object sender, DropEventArgs e) { await HandleDropAsync(TaskStatus.Completed, sender as Border); }

        private async Task HandleDropAsync(TaskStatus targetStatus, Border dropZoneBorder)
        {
            if (_viewModel.DraggedTask != null) await _viewModel.MoveTaskAndSave(_viewModel.DraggedTask, targetStatus);
            else Debug.WriteLine("[DEBUG] HandleDropAsync: DraggedTask is null. No action taken.");
            if (dropZoneBorder != null) dropZoneBorder.BackgroundColor = Colors.Transparent;
        }
    }
}