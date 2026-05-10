using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading; // Обязательно для таймера
using OthelloAI.Models;

namespace OthelloAI.ViewModels;

public class GameViewModel : ViewModelBase
{
    private Board _board;
    private PlayerColor _currentPlayer;
    
    // === ПЕРЕМЕННЫЕ ИИ ===
    private MinimaxAI _ai;
    private PlayerColor _aiColor; 
    private bool _isAiThinking = false; 
    private int _aiDepth; 
    private PlayerColor _humanColor;

    // === ТАЙМЕР ===
    private DispatcherTimer? _timer;
    private int _secondsLeft;

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

    public ICommand CellClickedCommand { get; }

    // === ОБНОВЛЕННЫЙ КОНСТРУКТОР ===
    // Теперь он принимает время, сложность и цвет игрока из стартового меню
    public GameViewModel(int minutes, int depth, PlayerColor humanColor)
    {
        _board = new Board(); 
        _ai = new MinimaxAI();
        _aiDepth = depth;
        _humanColor = humanColor;
        
        // ИИ всегда играет противоположным цветом
        _aiColor = (_humanColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        
        _currentPlayer = PlayerColor.Black; // В Отелло черные всегда ходят первыми
        
        BoardCells = new ObservableCollection<CellViewModel>();
        CellClickedCommand = new RelayCommand<CellViewModel>(OnCellClicked);

        // Настраиваем и запускаем таймер
        _secondsLeft = minutes * 60;
        TimeRemaining = TimeSpan.FromSeconds(_secondsLeft).ToString(@"mm\:ss");
        StartTimer();

        InitializeBoard();
        UpdateUI();

        // Если игрок выбрал белые фишки, ИИ (черные) должен сходить первым
        if (_humanColor == PlayerColor.White)
        {
            _ = LetAIPlay();
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
                _timer.Stop(); // Время вышло
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

    private async void OnCellClicked(CellViewModel cell)
    {
        // Блокируем клики, если ИИ думает или сейчас ход ИИ
        if (_isAiThinking || _currentPlayer == _aiColor) return;

        if (_board.isValidMove(cell.Row, cell.Column, _currentPlayer))
        {
            // Ход игрока
            _board.MakeMove(cell.Row, cell.Column, _currentPlayer);
            PlayerMoves.Add($"{cell.Row}, {cell.Column}");
            
            SwitchPlayer();
            UpdateUI();

            // Передаем ход ИИ
            if (_currentPlayer == _aiColor)
            {
                await LetAIPlay(); 
            }
        }
    }

    private async Task LetAIPlay()
    {
        _isAiThinking = true;
        await Task.Delay(500); // Небольшая пауза для естественности

        // Запускаем ИИ в фоне, чтобы окно не зависло
        Move? aiMove = await Task.Run(() => _ai.GetBestMove(_board, _aiDepth, _aiColor));

        if (aiMove != null)
        {
            _board.MakeMove(aiMove.Row, aiMove.Col, _aiColor);
            AIMoves.Add($"{aiMove.Row}, {aiMove.Col}");
        }

        SwitchPlayer();
        UpdateUI();
        
        _isAiThinking = false;
    }

    private void SwitchPlayer()
    {
        _currentPlayer = (_currentPlayer == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        IsDarkTurn = (_currentPlayer == PlayerColor.Black);
        IsLightTurn = (_currentPlayer == PlayerColor.White);
    }

    private void UpdateUI()
    {
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
                }
                else
                {
                    cellVm.HasPiece = true;
                    cellVm.IsDark = (state == CellState.Black);
                    cellVm.IsLight = (state == CellState.White);
                }
            }
        }

        BlackScore = _board.GetScore(PlayerColor.Black);
        WhiteScore = _board.GetScore(PlayerColor.White);
    }
}

// Вспомогательный класс для кнопок
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    public RelayCommand(Action<T> execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            _execute(t);
        }
    }
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}