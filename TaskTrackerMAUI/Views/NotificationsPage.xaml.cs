using TaskTrackerMAUI.ViewModels;

namespace TaskTrackerMAUI.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;
    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadPendingNotificationsCommand.CanExecute(null))
        {
            _viewModel.LoadPendingNotificationsCommand.Execute(null);
        }
    }
}