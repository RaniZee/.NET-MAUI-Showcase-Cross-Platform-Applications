using HelloMauiApp.Models;
using HelloMauiApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelloMauiApp.ViewModels;

public class HistoryViewModel : INotifyPropertyChanged
{
    private readonly BmiRepository _bmiRepository;
    public ObservableCollection<BmiResultRecord> History { get; } = new();

    public HistoryViewModel(BmiRepository bmiRepository)
    {
        _bmiRepository = bmiRepository;
    }

    public async Task LoadHistoryAsync()
    {
        var results = await _bmiRepository.GetResultsAsync();
        History.Clear();
        foreach (var result in results)
        {
            History.Add(result);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}