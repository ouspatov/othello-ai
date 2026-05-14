using System;
using System.Collections.Generic;

namespace OthelloAI.Models;

public class Board
{
    private readonly CellState[,] grid_;
    
    // === ДОБАВЛЕНО: Переменные для Хэширования ===
    private long _currentHash;
    private static readonly long[,,] _zobristTable = InitializeZobrist();

    private static long[,,] InitializeZobrist()
    {
        var rnd = new Random(42); // Фиксированный сид
        var table = new long[8, 8, 3];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                for (int k = 0; k < 3; k++)
                    table[i, j, k] = rnd.NextInt64(); // Генерируем случайное 64-битное число
        return table;
    }

    public long GetHash() => _currentHash;

    // Безопасный метод для получения индекса состояния (0 = Пусто, 1 = Черные, 2 = Белые)
    private int GetStateIndex(CellState state)
    {
        return state switch
        {
            CellState.Empty => 0,
            CellState.Black => 1,
            CellState.White => 2,
            _ => 0
        };
    }

    // === ДОБАВЛЕНО: Умный метод изменения клетки, который обновляет хэш ===
    private void SetCellWithHash(int r, int c, CellState newState)
    {
        CellState oldState = grid_[r, c];
        if (oldState == newState) return;

        // "Вычитаем" старое состояние из хэша
        _currentHash ^= _zobristTable[r, c, GetStateIndex(oldState)];
        
        // Меняем состояние на доске
        grid_[r, c] = newState;
        
        // "Прибавляем" новое состояние в хэш
        _currentHash ^= _zobristTable[r, c, GetStateIndex(newState)];
    }
    // =============================================

    private CellState GetState(PlayerColor player)
    {
        if (player == PlayerColor.Black)
        {
            return CellState.Black;
        }
        return CellState.White;
    }

    private bool IsOnBoard(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }

    public Board()
    {
        grid_ = new CellState[8, 8];

        // Заполняем хэш так, будто доска изначально полностью пустая
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _currentHash ^= _zobristTable[i, j, GetStateIndex(CellState.Empty)];
            }
        }

        // Расставляем стартовые фишки через новый метод
        SetCellWithHash(3, 3, CellState.White);
        SetCellWithHash(3, 4, CellState.Black);
        SetCellWithHash(4, 3, CellState.Black);
        SetCellWithHash(4, 4, CellState.White);
    }
    
    public bool isValidMove(int row, int col, PlayerColor player)
    {
        (int row, int col)[] directions =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        CellState myColor = GetState(player);
        CellState oponentsColor;
        if (myColor == CellState.Black)
        {
            oponentsColor = CellState.White;
        }
        else
        {
            oponentsColor = CellState.Black;
        }

        if (grid_[row, col] != CellState.Empty)
        {
            return false;
        }
        
        foreach (var(dr, dc) in directions)
        {
            int currentRow = row + dr;
            int currentCol = col + dc;
            bool trap = false;

            while (IsOnBoard(currentRow, currentCol))
            {
                CellState current = grid_[currentRow, currentCol];
                
                if (current == oponentsColor)
                {
                    trap = true;
                    currentRow += dr;
                    currentCol += dc;
                }
                else if (current == myColor)
                {
                    if (trap)
                    {
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                else 
                {
                    break;
                }
            }
        }            
        return false;
    }

    public void MakeMove(int row, int col, PlayerColor player)
    {
        if(!isValidMove(row, col, player))
        {
            return;
        }
        
        CellState myColor = GetState(player);
        CellState oponentsColor;

        if (myColor == CellState.Black)
        {
            oponentsColor = CellState.White;
        }
        else
        {
            oponentsColor = CellState.Black;
        }
        
        SetCellWithHash(row, col, myColor); // ИЗМЕНЕНО
        
        (int row, int col)[] directions =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };
        
        foreach (var (dr, dc) in directions)
        {
            int currentRow = row + dr;
            int currentCol = col + dc;

            List<(int r, int c)> piecesToFlip = new List<(int r, int c)>();
            
            while (IsOnBoard(currentRow, currentCol))
            {
                CellState current = grid_[currentRow, currentCol];
                if (current == oponentsColor)
                {
                    piecesToFlip.Add((currentRow, currentCol));

                    currentRow += dr;
                    currentCol += dc;
                }
                else if (current == myColor)
                {
                    foreach(var enemyPiece in piecesToFlip)
                    {
                        SetCellWithHash(enemyPiece.r, enemyPiece.c, myColor); // ИЗМЕНЕНО
                    }
                    break;
                }
                else 
                {
                    break;
                }
            }
        }
    }

    public List<(int r, int c)> MakeMoveWithRecord(int row, int col, PlayerColor player)
    {
        List<(int r, int c)> allFlippedPieces = new List<(int r, int c)>();

        if (!isValidMove(row, col, player))
        {
            return allFlippedPieces;
        }

        CellState myColor = GetState(player);
        CellState oponentsColor = (myColor == CellState.Black) ? CellState.White : CellState.Black;

        SetCellWithHash(row, col, myColor); // ИЗМЕНЕНО

        (int row, int col)[] directions =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        foreach (var (dr, dc) in directions)
        {
            int currentRow = row + dr;
            int currentCol = col + dc;

            List<(int r, int c)> piecesToFlipInDirection = new List<(int r, int c)>();

            while (IsOnBoard(currentRow, currentCol))
            {
                CellState current = grid_[currentRow, currentCol];
                if (current == oponentsColor)
                {
                    piecesToFlipInDirection.Add((currentRow, currentCol));

                    currentRow += dr;
                    currentCol += dc;
                }
                else if (current == myColor)
                {
                    foreach (var enemyPiece in piecesToFlipInDirection)
                    {
                        SetCellWithHash(enemyPiece.r, enemyPiece.c, myColor); // ИЗМЕНЕНО
                        allFlippedPieces.Add(enemyPiece);
                    }
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        return allFlippedPieces;
    }

    public void UndoMove(int row, int col, PlayerColor player, List<(int r, int c)> flippedPieces)
    {
        SetCellWithHash(row, col, CellState.Empty); // ИЗМЕНЕНО

        CellState oponentsColor = (GetState(player) == CellState.Black) ? CellState.White : CellState.Black;

        foreach (var piece in flippedPieces)
        {
            SetCellWithHash(piece.r, piece.c, oponentsColor); // ИЗМЕНЕНО
        }
    }

    public List<Move> GetValidMoves(PlayerColor player)
    {
        List<Move> ValidMoves = new List<Move>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (isValidMove(i, j, player))
                {
                    ValidMoves.Add(new Move(i, j, player));
                }
            }
        }
        return ValidMoves;
    }

    public bool HasValidMoves(PlayerColor player)
    {
        if (GetValidMoves(player).Count > 0)
        {
            return true;   
        }
        return false;        
    }

    public int GetScore(PlayerColor player)
    {
        CellState target = GetState(player);
        int score = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++) {
                if (grid_[i, j] == target)
                {
                    score++;
                }
            }
        } 
        return score;
    }

    public CellState GetCellState(int row, int col)
    {
        return grid_[row, col];
    }
    
    public Board Clone()
    {
        Board newBoard = new Board();

        for(int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                newBoard.grid_[i, j] = this.grid_[i, j];
            }
        }
        
        // Копируем хэш, чтобы клон знал свой ID
        newBoard._currentHash = this._currentHash;

        return newBoard;
    }
}