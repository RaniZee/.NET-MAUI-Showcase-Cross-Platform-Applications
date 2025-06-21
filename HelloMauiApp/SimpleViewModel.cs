using System.ComponentModel;   
using System.Runtime.CompilerServices;   
using System.Windows.Input;   

namespace HelloMauiApp;

public class SimpleViewModel : INotifyPropertyChanged
{
    private string _userName;
    public string UserName
    {
        get => _userName;
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged();     
                OnPropertyChanged(nameof(Greeting));    
            }
        }
    }

    private string _entryText;
    public string EntryText
    {
        get => _entryText;
        set
        {
            if (_entryText != value)
            {
                _entryText = value;
                OnPropertyChanged();
            }
        }
    }

    public string Greeting => $"Hello, {UserName}!";      

    private double _sliderValue;
    public double SliderValue
    {
        get => _sliderValue;
        set
        {
            if (_sliderValue != value)
            {
                _sliderValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SliderValueText));
            }
        }
    }
    public string SliderValueText => $"Slider is at: {SliderValue:F2}";

    public ICommand UpdateNameCommand { get; }
    public ICommand ResetNameCommand { get; }

    public SimpleViewModel()
    {
        UserName = "MAUI User";   
        EntryText = "Type here";
        SliderValue = 0.5;

        UpdateNameCommand = new Command<string>((newName) =>
        {
            UserName = string.IsNullOrWhiteSpace(newName) ? "Default User" : newName;
        });

        ResetNameCommand = new Command(
            execute: () => {
                UserName = "MAUI User";
                EntryText = "Type here";
                SliderValue = 0.5;
            },
            canExecute: () => !string.IsNullOrEmpty(UserName) && UserName != "MAUI User"     
        );
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(UserName) && ResetNameCommand is Command cmd)
        {
            cmd.ChangeCanExecute();
        }
    }
}