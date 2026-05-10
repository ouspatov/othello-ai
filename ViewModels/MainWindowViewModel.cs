namespace OthelloAI.ViewModels;
using OthelloAI.Models;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase currentViewModel_; // ViewModelBase meaning you can put any UserControl

    public ViewModelBase CurrentViewModel
    {
        get => currentViewModel_; // reading
        set 
        {
            // writing
            currentViewModel_ = value;
            OnPropertyChanged(); // Notify that the screen has changed
        }
    }

    // Constructor
    public MainWindowViewModel()
    {
        currentViewModel_ = new StartScreenViewModel(this); // on the lauch init with the StartScreenViewModel
    }

    public void GoToGame(int minutes, int depth, PlayerColor humanColor)
    {
        // Link to the start button, it will change the screen to the GameViewModel
        CurrentViewModel = new GameViewModel(minutes, depth, humanColor);
    }
}