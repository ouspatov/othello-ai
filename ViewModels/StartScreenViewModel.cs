using System;
using System.Windows.Input;
using OthelloAI.Models;

namespace OthelloAI.ViewModels;

public class StartScreenViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;

    private bool _isDarkColorSelected = true; 
    public bool IsDarkColorSelected 
    { 
        get => _isDarkColorSelected; 
        set { _isDarkColorSelected = value; OnPropertyChanged(); } 
    }

    private int _selectedTimeIndex = 0; 
    public int SelectedTimeIndex 
    { 
        get => _selectedTimeIndex; 
        set { _selectedTimeIndex = value; OnPropertyChanged(); } 
    }

    private int _selectedDifficultyIndex = 1; 
    public int SelectedDifficultyIndex 
    { 
        get => _selectedDifficultyIndex; 
        set { _selectedDifficultyIndex = value; OnPropertyChanged(); } 
    }

    public ICommand StartGameCommand { get; }

    public StartScreenViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        StartGameCommand = new RelayCommand(StartGame);
    }

    private void StartGame()
    {
        int minutes = SelectedTimeIndex switch
        {
            0 => 10, 
            1 => 20, 
            2 => 30, 
            _ => 10
        };

        int depth = SelectedDifficultyIndex switch
        {
            0 => 2, 1 => 4, 2 => 6, 3 => 8, _ => 4
        };

        PlayerColor playerColor = IsDarkColorSelected ? PlayerColor.Black : PlayerColor.White;

        _mainViewModel.GoToGame(minutes, depth, playerColor);
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}