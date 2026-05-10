using System;
using System.Collections.Generic;

namespace OthelloAI.Models;

public class MinimaxAI
{
    public Move? GetBestMove(Board board, int depth, PlayerColor aiColor)
    {
        Move? bestMove = null;
        int maxEval = int.MinValue;
        
        List<Move> validMoves = board.GetValidMoves(aiColor);
        if (validMoves.Count == 0) return null;

        foreach (Move move in validMoves)
        {
            Board clonedBoard = board.Clone();
            clonedBoard.MakeMove(move.Row, move.Col, aiColor);

            int eval = Minimax(clonedBoard, depth - 1, int.MinValue, int.MaxValue, false, aiColor);

            if (eval > maxEval)
            {
                maxEval = eval;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer, PlayerColor aiColor)
    {
        PlayerColor opponentColor = (aiColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        PlayerColor currentPlayer = isMaximizingPlayer ? aiColor : opponentColor;

        List<Move> validMoves = board.GetValidMoves(currentPlayer);

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
                
                if (beta <= alpha) break;
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
                
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }
}