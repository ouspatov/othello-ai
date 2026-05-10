namespace OthelloAI.Models;

public static class Evaluator
{
    private static readonly int[,] Weights = new int[8, 8]
    {
        { 120, -20,  20,   5,   5,  20, -20, 120 },
        { -20, -40,  -5,  -5,  -5,  -5, -40, -20 },
        {  20,  -5,  15,   3,   3,  15,  -5,  20 },
        {   5,  -5,   3,   3,   3,   3,  -5,   5 },
        {   5,  -5,   3,   3,   3,   3,  -5,   5 },
        {  20,  -5,  15,   3,   3,  15,  -5,  20 },
        { -20, -40,  -5,  -5,  -5,  -5, -40, -20 },
        { 120, -20,  20,   5,   5,  20, -20, 120 }
    };

    public static int Evaluate(Board board, PlayerColor aiColor)
    {
        int score = 0;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                CellState state = board.GetCellState(r, c);
                
                if (state != CellState.Empty)
                {
                    int weight = Weights[r, c];
                    
                    if ((state == CellState.Black && aiColor == PlayerColor.Black) ||
                        (state == CellState.White && aiColor == PlayerColor.White))
                    {
                        score += weight;
                    }
                    else
                    {
                        score -= weight;
                    }
                }
            }
        }
        return score;
    }
}