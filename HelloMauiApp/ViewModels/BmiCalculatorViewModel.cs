using HelloMauiApp.Models;
using HelloMauiApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HelloMauiApp.ViewModels;

public class BmiCalculatorViewModel : INotifyPropertyChanged
{
    private readonly BmiRepository _bmiRepository;

    private string _heightText;
    public string HeightText
    {
        get => _heightText;
        set => SetProperty(ref _heightText, value);
    }

    private string _weightText;
    public string WeightText
    {
        get => _weightText;
        set => SetProperty(ref _weightText, value);
    }

    private string _selectedGender;
    public string SelectedGender
    {
        get => _selectedGender;
        set => SetProperty(ref _selectedGender, value);
    }

    private double _bmiResult;
    public double BmiResult
    {
        get => _bmiResult;
        set => SetProperty(ref _bmiResult, value);
    }

    private string _bmiDescription;
    public string BmiDescription
    {
        get => _bmiDescription;
        set => SetProperty(ref _bmiDescription, value);
    }

    private Color _bmiResultColor;
    public Color BmiResultColor
    {
        get => _bmiResultColor;
        set => SetProperty(ref _bmiResultColor, value);
    }

    private bool _isResultVisible;
    public bool IsResultVisible
    {
        get => _isResultVisible;
        set => SetProperty(ref _isResultVisible, value);
    }

    public ICommand CalculateBmiCommand { get; }
    public ICommand SelectGenderCommand { get; }
    public ICommand GoToHistoryCommand { get; }

    public BmiCalculatorViewModel(BmiRepository bmiRepository)
    {
        _bmiRepository = bmiRepository;
        CalculateBmiCommand = new Command(ExecuteCalculateBmiCommand, CanExecuteCalculateBmiCommand);
        SelectGenderCommand = new Command<string>(ExecuteSelectGenderCommand);
        GoToHistoryCommand = new Command(async () => await ExecuteGoToHistoryCommand());
    }

    private bool CanExecuteCalculateBmiCommand()
    {
        return !string.IsNullOrWhiteSpace(HeightText)
            && !string.IsNullOrWhiteSpace(WeightText)
            && !string.IsNullOrWhiteSpace(SelectedGender);
    }

    private void OnCanExecuteChanged()
    {
        (CalculateBmiCommand as Command)?.ChangeCanExecute();
    }

    private void ExecuteSelectGenderCommand(string gender)
    {
        SelectedGender = gender;
    }

    private async void ExecuteCalculateBmiCommand()
    {
        bool isHeightValid = double.TryParse(HeightText, out double height) && height >= 50 && height <= 250;
        bool isWeightValid = double.TryParse(WeightText, out double weight) && weight >= 20 && weight <= 300;

        if (isHeightValid && isWeightValid)
        {
            height /= 100;
            BmiResult = Math.Round(weight / (height * height), 1);
            UpdateBmiClassification();
            IsResultVisible = true;

            await _bmiRepository.SaveResultAsync(new BmiResultRecord
            {
                Bmi = this.BmiResult,
                Classification = this.BmiDescription,
                Date = DateTime.Now
            });
        }
        else
        {
            string errorMessage = "Пожалуйста, введите корректные данные:\n- Рост: от 50 до 250 см\n- Вес: от 20 до 300 кг";
            await Application.Current.MainPage.DisplayAlert("Ошибка валидации", errorMessage, "OK");
        }
    }

    private void UpdateBmiClassification()
    {
        string baseDescription;
        if (BmiResult < 18.5)
        {
            baseDescription = "Ниже нормального веса";
            BmiResultColor = Color.FromArgb("#3498db");
        }
        else if (BmiResult < 25)
        {
            baseDescription = "Нормальный вес";
            BmiResultColor = Color.FromArgb("#2ecc71");
        }
        else if (BmiResult < 30)
        {
            baseDescription = "Избыточный вес";
            BmiResultColor = Color.FromArgb("#f1c40f");
        }
        else
        {
            baseDescription = "Ожирение";
            BmiResultColor = Color.FromArgb("#e74c3c");
        }

        string genderText = (SelectedGender == "Male") ? "(мужчина)" : "(женщина)";
        BmiDescription = $"{baseDescription} {genderText}";
    }

    private async Task ExecuteGoToHistoryCommand()
    {
        await Shell.Current.GoToAsync(nameof(HistoryPage));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Object.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(propertyName);
        OnCanExecuteChanged();         
        return true;
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}