using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using OthelloAI.Models;

namespace OthelloAI.ViewModels;

public class GameViewModel : ViewModelBase
{
    private Board _board = null!;
    private PlayerColor _currentPlayer;
    
    private MinimaxAI _ai = null!;
    private PlayerColor _aiColor; 
    private bool _isAiThinking = false; 
    private int _aiDepth; 
    private PlayerColor _humanColor;

    private DispatcherTimer? _timer;
    private int _secondsLeft;
    private int _startingMinutes;

    public ObservableCollection<CellViewModel> BoardCells { get; set; }
    public ObservableCollection<string> PlayerMoves { get; set; } = new();
    public ObservableCollection<string> AIMoves { get; set; } = new();

    private string _timeRemaining = "00:00";
    public string TimeRemaining 
    { 
        get => _timeRemaining; 
        set { _timeRemaining = value; OnPropertyChanged(); } 
    }

    private bool _isDarkTurn = true;
    public bool IsDarkTurn 
    { 
        get => _isDarkTurn; 
        set { _isDarkTurn = value; OnPropertyChanged(); } 
    }

    private bool _isLightTurn = false;
    public bool IsLightTurn 
    { 
        get => _isLightTurn; 
        set { _isLightTurn = value; OnPropertyChanged(); } 
    }

    private int _blackScore;
    public int BlackScore 
    { 
        get => _blackScore; 
        set { _blackScore = value; OnPropertyChanged(); } 
    }

    private int _whiteScore;
    public int WhiteScore 
    { 
        get => _whiteScore; 
        set { _whiteScore = value; OnPropertyChanged(); } 
    }

    private bool _isGameOver;
    public bool IsGameOver 
    { 
        get => _isGameOver; 
        set { _isGameOver = value; OnPropertyChanged(); } 
    }

    private string _gameOverMessage = "";
    public string GameOverMessage 
    { 
        get => _gameOverMessage; 
        set { _gameOverMessage = value; OnPropertyChanged(); } 
    }

    public ICommand CellClickedCommand { get; }
    public ICommand RestartCommand { get; } // Новая команда для кнопки PLAY AGAIN
    public ICommand QuitCommand { get; } // НОВАЯ КОМАНДА ВЫХОДА

    public GameViewModel(int minutes, int depth, PlayerColor humanColor)
    {
        _startingMinutes = minutes;
        _aiDepth = depth;
        _humanColor = humanColor;
        _aiColor = (_humanColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        
        BoardCells = new ObservableCollection<CellViewModel>();
        CellClickedCommand = new RelayCommand<CellViewModel>(OnCellClicked);
        
        RestartCommand = new RelayCommand<object>(_ => StartNewGame());
        QuitCommand = new RelayCommand<object>(_ => Environment.Exit(0));

        StartNewGame();
    }

    private void StartNewGame()
    {
        _board = new Board(); 
        _ai = new MinimaxAI();
        _currentPlayer = PlayerColor.Black; 
        IsGameOver = false;
        
        PlayerMoves.Clear();
        AIMoves.Clear();

        InitializeBoard();
        UpdateUI();
        
        IsDarkTurn = true;
        IsLightTurn = false;

        _timer?.Stop();
        _secondsLeft = _startingMinutes * 60;
        TimeRemaining = TimeSpan.FromSeconds(_secondsLeft).ToString(@"mm\:ss");

        if (_timer == null) StartTimer();

        // Проверяем, кто ходит первым
        if (_humanColor == PlayerColor.White)
        {
            // Если человек за белых, то первый ход у ИИ (черные)
            // Таймер не запускаем, вызываем ход ИИ
            _timer?.Stop(); 
            _ = LetAIPlay();
        }
        else
        {
            // Если человек за черных, он ходит первым — запускаем таймер
            _timer?.Start();
        }
    }

    private void StartTimer()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) =>
        {
            if (_secondsLeft > 0)
            {
                _secondsLeft--;
                TimeRemaining = TimeSpan.FromSeconds(_secondsLeft).ToString(@"mm\:ss");
            }
            else
            {
                EndGame("timeout");
            }
        };
        _timer.Start();
    }

    private void InitializeBoard()
    {
        BoardCells.Clear();
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                BoardCells.Add(new CellViewModel(r, c));
            }
        }
    }

    private async Task LetAIPlay()
    {
        _isAiThinking = true;
        await Task.Delay(500);

        Move? aiMove = await Task.Run(() => _ai.GetBestMove(_board, _aiDepth, _aiColor));

        if (aiMove != null)
        {
            _board.MakeMove(aiMove.Row, aiMove.Col, _aiColor);
            AIMoves.Add($"{aiMove.Row + 1}, {aiMove.Col + 1}");
        }

        SwitchPlayer();
        UpdateUI();
        _isAiThinking = false;

        CheckGameOver();
    }

    private void SwitchPlayer()
    {
        _currentPlayer = (_currentPlayer == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        IsDarkTurn = (_currentPlayer == PlayerColor.Black);
        IsLightTurn = (_currentPlayer == PlayerColor.White);

        // ЛОГИКА ТАЙМЕРА:
        // Если сейчас ход человека и игра не окончена — запускаем таймер.
        // В противном случае (ход ИИ) — ставим таймер на паузу.
        if (_currentPlayer == _humanColor && !IsGameOver)
        {
            _timer?.Start();
        }
        else
        {
            _timer?.Stop();
        }
    }

    private async void OnCellClicked(CellViewModel cell)
    {
        if (IsGameOver || _isAiThinking || _currentPlayer == _aiColor) return;

        if (_board.isValidMove(cell.Row, cell.Column, _currentPlayer))
        {
            _board.MakeMove(cell.Row, cell.Column, _currentPlayer);
            PlayerMoves.Add($"{cell.Row + 1}, {cell.Column + 1}");
            
            SwitchPlayer();
            UpdateUI();

            CheckGameOver();

            if (!IsGameOver && _currentPlayer == _aiColor)
            {
                await LetAIPlay(); 
            }
        }
    }

    private void UpdateUI()
    {
        bool isHumanTurn = (_currentPlayer == _humanColor) && !IsGameOver;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var state = _board.GetCellState(r, c);
                var cellVm = BoardCells.First(x => x.Row == r && x.Column == c);

                if (state == CellState.Empty)
                {
                    cellVm.HasPiece = false;
                    cellVm.IsDark = false;
                    cellVm.IsLight = false;
                    cellVm.IsValidMove = isHumanTurn && _board.isValidMove(r, c, _currentPlayer);
                }
                else
                {
                    cellVm.HasPiece = true;
                    cellVm.IsDark = (state == CellState.Black);
                    cellVm.IsLight = (state == CellState.White);
                    cellVm.IsValidMove = false;
                }
            }
        }

        BlackScore = _board.GetScore(PlayerColor.Black);
        WhiteScore = _board.GetScore(PlayerColor.White);
    }

    private void CheckGameOver()
    {
        bool blackHasMoves = _board.HasValidMoves(PlayerColor.Black);
        bool whiteHasMoves = _board.HasValidMoves(PlayerColor.White);

        if (!blackHasMoves && !whiteHasMoves)
        {
            EndGame("out_of_moves");
            return;
        }

        bool currentPlayerHasMoves = _currentPlayer == PlayerColor.Black ? blackHasMoves : whiteHasMoves;
        
        if (!currentPlayerHasMoves)
        {
            SwitchPlayer();
            if (_currentPlayer == _aiColor && !IsGameOver)
            {
                _ = LetAIPlay();
            }
        }
    }

    private void EndGame(string reason)
    {
        _timer?.Stop();
        IsGameOver = true;

        if (reason == "timeout")
        {
            GameOverMessage = "TIME'S UP!";
        }
        else
        {
            if (BlackScore > WhiteScore) 
                GameOverMessage = _humanColor == PlayerColor.Black ? "YOU WIN!" : "AI WINS!";
            else if (WhiteScore > BlackScore) 
                GameOverMessage = _humanColor == PlayerColor.White ? "YOU WIN!" : "AI WINS!";
            else 
                GameOverMessage = "IT'S A DRAW!";
        }
    }

    public bool IsHumanDark => _humanColor == PlayerColor.Black;
    public bool IsHumanLight => _humanColor == PlayerColor.White;
    
    public bool IsAIDark => _aiColor == PlayerColor.Black;
    public bool IsAILight => _aiColor == PlayerColor.White;
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    public RelayCommand(Action<T> execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter)
    {
        if (parameter is T t)
            _execute(t);
        else if (parameter == null)
            _execute(default!);
    }
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}