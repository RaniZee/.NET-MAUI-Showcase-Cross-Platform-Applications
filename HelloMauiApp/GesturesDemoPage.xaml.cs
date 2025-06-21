namespace HelloMauiApp;

public partial class GesturesDemoPage : ContentPage
{
    private double _initialScale = 1;
    private double _xOffset = 0;
    private double _yOffset = 0;

    public GesturesDemoPage()
    {
        InitializeComponent();
    }

    private void OnSingleTap(object sender, TappedEventArgs e)
    {
        EventOutputLabel.Text = "Single Tap on Image detected!";
    }

    private void OnDoubleTap(object sender, TappedEventArgs e)
    {
        EventOutputLabel.Text = "Double Tap on Frame detected!";
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _initialScale = PinchImage.Scale;
                EventOutputLabel.Text = "Pinch gesture started.";
                break;
            case GestureStatus.Running:
                var currentScale = _initialScale * e.Scale;
                PinchImage.Scale = Math.Clamp(currentScale, 0.5, 3.0);
                EventOutputLabel.Text = $"Pinch running. Scale: {PinchImage.Scale:F2}";
                break;
            case GestureStatus.Completed:
                EventOutputLabel.Text = "Pinch gesture completed.";
                break;
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (sender is not View view)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _xOffset = view.TranslationX;
                _yOffset = view.TranslationY;
                EventOutputLabel.Text = "Pan gesture started.";
                break;
            case GestureStatus.Running:
                view.TranslationX = _xOffset + e.TotalX;
                view.TranslationY = _yOffset + e.TotalY;
                EventOutputLabel.Text = $"Pan running. X: {view.TranslationX:F0}, Y: {view.TranslationY:F0}";
                break;
            case GestureStatus.Completed:
                EventOutputLabel.Text = "Pan gesture completed.";
                break;
        }
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        EventOutputLabel.Text = $"Swiped in direction: {e.Direction}";
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        PointerLabel.Text = "Mouse Pointer Entered!";
        PointerLabel.TextColor = Colors.Green;
        EventOutputLabel.Text = "Pointer Entered";
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        PointerLabel.Text = "Hover mouse over this area";
        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            PointerLabel.TextColor = Colors.White;
        }
        else
        {
            PointerLabel.TextColor = Colors.Black;
        }
        EventOutputLabel.Text = "Pointer Exited";
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        EventOutputLabel.Text = "Pointer Moved";
    }
}