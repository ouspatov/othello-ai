namespace OthelloAI.ViewModels;

public class CellViewModel : ViewModelBase
{
    public int Row { get; }
    public int Column { get; }

    private bool _hasPiece;
    public bool HasPiece
    {
        get => _hasPiece;
        set { _hasPiece = value; OnPropertyChanged(); }
    }

    private bool _isDark;
    public bool IsDark
    {
        get => _isDark;
        set { _isDark = value; OnPropertyChanged(); }
    }

    private bool _isLight;
    public bool IsLight
    {
        get => _isLight;
        set { _isLight = value; OnPropertyChanged(); }
    }

    public CellViewModel(int row, int column)
    {
        Row = row;
        Column = column;
        HasPiece = false;
        IsDark = false;
        IsLight = false;
    }
    private bool _isValidMove;
    public bool IsValidMove
    {
        get => _isValidMove;
        set { _isValidMove = value; OnPropertyChanged(); }
    }
}