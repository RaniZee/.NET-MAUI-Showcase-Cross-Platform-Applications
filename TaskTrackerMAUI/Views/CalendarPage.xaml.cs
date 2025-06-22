using TaskTrackerMAUI.ViewModels;

namespace TaskTrackerMAUI.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;
    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadTasksCommand.CanExecute(null))
        {
            _viewModel.LoadTasksCommand.Execute(null);
        }
    }
}