using System;
using System.Collections.Generic;

namespace OthelloAI.Models;

public class MinimaxAI
{
    // Главный метод, который вызывает UI, чтобы узнать ход компьютера
    public Move? GetBestMove(Board board, int depth, PlayerColor aiColor)
    {
        Move? bestMove = null;
        int maxEval = int.MinValue;
        
        // Получаем все возможные ходы для ИИ
        List<Move> validMoves = board.GetValidMoves(aiColor);
        if (validMoves.Count == 0) return null;

        // Перебираем каждый ход
        foreach (Move move in validMoves)
        {
            Board clonedBoard = board.Clone();
            clonedBoard.MakeMove(move.Row, move.Col, aiColor);

            // Запускаем рекурсию Minimax (со следующего хода - ход противника, поэтому false)
            int eval = Minimax(clonedBoard, depth - 1, int.MinValue, int.MaxValue, false, aiColor);

            if (eval > maxEval)
            {
                maxEval = eval;
                bestMove = move;
            }
        }

        return bestMove;
    }

    // Сам рекурсивный алгоритм с Альфа-Бета отсечением
    private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer, PlayerColor aiColor)
    {
        PlayerColor opponentColor = (aiColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        PlayerColor currentPlayer = isMaximizingPlayer ? aiColor : opponentColor;

        List<Move> validMoves = board.GetValidMoves(currentPlayer);

        // Базовый случай: достигли дна или нет ходов
        if (depth == 0 || validMoves.Count == 0)
        {
            return Evaluator.Evaluate(board, aiColor);
        }

        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Move move in validMoves)
            {
                Board clonedBoard = board.Clone();
                clonedBoard.MakeMove(move.Row, move.Col, currentPlayer);
                
                int eval = Minimax(clonedBoard, depth - 1, alpha, beta, false, aiColor);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                
                if (beta <= alpha) break; // Бета-отсечение
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Move move in validMoves)
            {
                Board clonedBoard = board.Clone();
                clonedBoard.MakeMove(move.Row, move.Col, currentPlayer);
                
                int eval = Minimax(clonedBoard, depth - 1, alpha, beta, true, aiColor);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                
                if (beta <= alpha) break; // Альфа-отсечение
            }
            return minEval;
        }
    }
}