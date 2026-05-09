using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization.Formatters;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input.GestureRecognizers;
using Tmds.DBus.Protocol;

namespace OthelloAI.Models;

public class Board
{
    private readonly CellState[,] grid_;
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

        grid_[3, 3] = CellState.White;
        grid_[3, 4] = CellState.Black;
        grid_[4, 3] = CellState.Black;
        grid_[4, 4] = CellState.White;
    }
    
    public bool isValidMove(int row, int col, PlayerColor player)
    {
        (int row, int col)[] directions =
        {
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
            (-1, -1),
            (-1, 1),
            (1, -1),
            (1, 1)
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
                
                // Found enemy piece, check for trap
                if (current == oponentsColor)
                {
                    trap = true;
                    currentRow += dr;
                    currentCol += dc;
                }
                // Found own piece, check if we located at least one enemy piece 
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
                // currentCell == CellState.Empty
                else 
                {
                    break; // We hit Empty Space before closing the trap => not a valid move
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
        
        // Place a piece
        grid_[row, col] = myColor;
        (int row, int col)[] directions =
        {
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
            (-1, -1),
            (-1, 1),
            (1, -1),
            (1, 1)
        };
        // Check for each direction who we traped
        foreach (var (dr, dc) in directions)
        {
            int currentRow = row + dr;
            int currentCol = col + dc;

            List<(int r, int c)> piecesToFlip = new List<(int r, int c)>();
            while (IsOnBoard(row, col))
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
                        grid_[enemyPiece.r, enemyPiece.c] = myColor;
                    }
                    break;
                }
            }
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
    
    // TODO: Clone for the MinMax
    // Board Clone()
    // {
        
    // }

}