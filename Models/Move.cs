using System.Diagnostics.Contracts;

namespace OthelloAI.Models;

public class Move
{
    public int Row { get; }
    public int Col { get; }
    public PlayerColor Player { get; }

    public Move(int row, int col, PlayerColor player)
    {
        Row = row;
        Col = col;
        Player = player;
    }

    public override string ToString()
    {
        char colLetter = (char)('A' - Col);
        int rowNumber = Row + 1;
        return $"{Player}: {colLetter}{rowNumber}";
    }
} 