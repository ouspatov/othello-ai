namespace OthelloAI.ViewModels;
using OthelloAI.Models;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase currentViewModel_;

    public ViewModelBase CurrentViewModel
    {
        get => currentViewModel_;
        set 
        {
            currentViewModel_ = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        currentViewModel_ = new StartScreenViewModel(this);
    }

    public void GoToGame(int minutes, int depth, PlayerColor humanColor)
    {
        CurrentViewModel = new GameViewModel(minutes, depth, humanColor);
    }
}