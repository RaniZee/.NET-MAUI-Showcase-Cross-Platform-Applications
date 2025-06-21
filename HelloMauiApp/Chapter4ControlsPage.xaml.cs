using System.Collections.ObjectModel;
using System.ComponentModel;   
using System.Runtime.CompilerServices;   
using System.Windows.Input;   

namespace HelloMauiApp;

public class ListItem : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _category;
    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Object.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public partial class Chapter4ControlsPage : ContentPage
{
    public ObservableCollection<ListItem> CollectionViewItems { get; set; }
    public ICommand RefreshCommand { get; }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged(nameof(IsRefreshing));      
        }
    }

    public Chapter4ControlsPage()
    {
        InitializeComponent();

        CollectionViewItems = new ObservableCollection<ListItem>
        {
            new ListItem { Name = "Apples", Category = "Fruit" },
            new ListItem { Name = "Milk", Category = "Dairy" },
            new ListItem { Name = "Bread", Category = "Bakery" },
            new ListItem { Name = "Tomatoes", Category = "Vegetable" },
            new ListItem { Name = "Chicken", Category = "Meat" }
        };
        DemoCollectionView.ItemsSource = CollectionViewItems;

        RefreshCommand = new Command(async () => await ExecuteRefreshCommand());

        BindingContext = this;

        if (DemoPicker.ItemsSource != null && DemoPicker.Items.Count > 0)
        {
            DemoPicker.SelectedIndex = 0;
        }
        SliderValueLabel.Text = $"Slider Value: {DemoSlider.Value:F2}";
        StepperValueLabel.Text = $"Stepper Value: {DemoStepper.Value}";
    }

    async Task ExecuteRefreshCommand()
    {
        if (IsRefreshing)
            return;

        IsRefreshing = true;
        EventOutputLabel.Text = "RefreshView: Refreshing data...";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);

        await Task.Delay(2000);

        var newItemNumber = CollectionViewItems.Count + 1;
        CollectionViewItems.Insert(0, new ListItem { Name = $"New Item {newItemNumber}", Category = "Added" });

        EventOutputLabel.Text = "RefreshView: Data refreshed.";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
        IsRefreshing = false;
    }

    private void Generic_TextChanged(object sender, TextChangedEventArgs e)
    {
        string controlName = sender.GetType().Name;
        EventOutputLabel.Text = $"{controlName} TextChanged: Old='{e.OldTextValue}', New='{e.NewTextValue}'";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void Generic_Completed(object sender, EventArgs e)
    {
        string controlName = sender.GetType().Name;
        EventOutputLabel.Text = $"{controlName} Completed.";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void CommonControl_Clicked(object sender, EventArgs e)
    {
        string controlName = "Unknown Control";
        if (sender is Button btn) controlName = $"Button '{btn.Text}'";
        else if (sender is ImageButton) controlName = "ImageButton";

        EventOutputLabel.Text = $"{controlName} clicked.";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        EventOutputLabel.Text = $"CheckBox is now {(e.Value ? "Checked" : "Unchecked")}.";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton rb)
        {
            EventOutputLabel.Text = $"RadioButton '{rb.Content}' (Value: {rb.Value}) selected in group '{rb.GroupName}'.";
            System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
        }
    }

    private void DemoPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (DemoPicker.SelectedItem != null)
        {
            EventOutputLabel.Text = $"Picker selected: {DemoPicker.SelectedItem}";
            System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
        }
    }

    private void DemoDatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        EventOutputLabel.Text = $"DatePicker selected date: {e.NewDate:d}";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
        {
            EventOutputLabel.Text = $"TimePicker selected time: {DemoTimePicker.Time}";
            System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
        }
    }

    private void DemoSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        EventOutputLabel.Text = $"Switch is now {(e.Value ? "ON" : "OFF")}.";
        DemoActivityIndicator.IsRunning = e.Value;      
        DemoProgressBar.ProgressTo(e.Value ? 0.9 : 0.1, 250, Easing.Linear);   
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        SliderValueLabel.Text = $"Slider Value: {e.NewValue:F2}";
        EventOutputLabel.Text = $"Slider new value: {e.NewValue:F2}";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoStepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        StepperValueLabel.Text = $"Stepper Value: {e.NewValue}";
        EventOutputLabel.Text = $"Stepper new value: {e.NewValue}";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoSearchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        SearchBar searchBar = sender as SearchBar;
        EventOutputLabel.Text = $"SearchBar SearchButtonPressed: {searchBar?.Text}";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        EventOutputLabel.Text = $"SearchBar TextChanged: {e.NewTextValue}";
        System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
    }

    private void DemoRefreshView_Refreshing(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("RefreshView Refreshing event triggered.");
    }

    private void SwipeItem_Invoked_Favorite(object sender, EventArgs e)
    {
        SwipeItem swipeItem = sender as SwipeItem;
        ListItem item = swipeItem?.BindingContext as ListItem;
        if (item != null)
        {
            EventOutputLabel.Text = $"SwipeView: '{item.Name}' marked as Favorite.";
            System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
        }
    }

    private void SwipeItem_Invoked_Delete(object sender, EventArgs e)
    {
        SwipeItem swipeItem = sender as SwipeItem;
        ListItem item = swipeItem?.BindingContext as ListItem;
        if (item != null)
        {
            EventOutputLabel.Text = $"SwipeView: '{item.Name}' Delete action.";
            System.Diagnostics.Debug.WriteLine(EventOutputLabel.Text);
            CollectionViewItems.Remove(item);     
        }
    }
}