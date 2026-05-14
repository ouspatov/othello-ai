using System;
using System.Collections.Generic;

namespace OthelloAI.Models;

public class Evaluator
{
    // Матрица весов: углы бесценны (+120), клетки рядом с углами опасны (-20, -40), края хороши.
    public static readonly int[,] Weights = new int[8, 8]
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

    public static double Evaluate(Board board, PlayerColor aiColor)
    {
        PlayerColor opponentColor = (aiColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;

        double positionalScore = 0;

        // 1. ПОЗИЦИОННАЯ ОЦЕНКА (контроль важных клеток)
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
                        positionalScore += weight;
                    }
                    else
                    {
                        positionalScore -= weight;
                    }
                }
            }
        }

        // 2. ОЦЕНКА МОБИЛЬНОСТИ (ключевой фактор в Отелло)
        int aiMobility = board.GetValidMoves(aiColor).Count;
        int oppMobility = board.GetValidMoves(opponentColor).Count;
        
        double mobilityScore = 0;
        if (aiMobility + oppMobility != 0)
        {
            // Формула дает от -100 до 100 очков в зависимости от того, у кого больше ходов
            mobilityScore = 100.0 * (aiMobility - oppMobility) / (aiMobility + oppMobility);
        }

        // 3. ФИНАЛЬНАЯ КОМБИНАЦИЯ
        // Умножаем мобильность на коэффициент, чтобы ИИ понимал, что лишить врага ходов важнее, 
        // чем просто захватить рядовую клетку.
        return positionalScore + (mobilityScore * 2.0); 
    }
}