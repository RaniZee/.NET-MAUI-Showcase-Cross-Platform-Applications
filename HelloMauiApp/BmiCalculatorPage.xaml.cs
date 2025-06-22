using HelloMauiApp.ViewModels;
using System.ComponentModel;

namespace HelloMauiApp;

public partial class BmiCalculatorPage : ContentPage
{
    private readonly BmiCalculatorViewModel _viewModel;

    public BmiCalculatorPage(BmiCalculatorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BmiCalculatorViewModel.IsResultVisible))
        {
            if (sender is BmiCalculatorViewModel vm && vm.IsResultVisible)
            {
                AnimateResultFrame();
            }
        }
    }

    private async void AnimateResultFrame()
    {
        ResultFrame.Opacity = 0;
        ResultFrame.Scale = 0.8;

        await ResultFrame.FadeTo(1, 400, Easing.SinOut);
        await ResultFrame.ScaleTo(1, 400, Easing.SpringOut);
    }
}