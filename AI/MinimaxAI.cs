using System;
using System.Collections.Generic;
using System.Linq;

namespace OthelloAI.Models;

public class MinimaxAI
{
    private struct TTEntry
    {
        public double Score;
        public int Depth;
    }

    private readonly Dictionary<long, TTEntry> _transpositionTable = new();

    private const double WIN_SCORE = 100000.0;
    private const double LOSE_SCORE = -100000.0;

    public Move? GetBestMove(Board board, int maxDepth, PlayerColor aiColor)
    {
        // Очищаем кэш только перед самым началом нового хода
        _transpositionTable.Clear(); 

        Move? globalBestMove = null;
        PlayerColor opponentColor = (aiColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        
        var validMoves = board.GetValidMoves(aiColor);
        if (validMoves.Count == 0) return null;

        // === ИТЕРАТИВНОЕ УГЛУБЛЕНИЕ ===
        // Идем от 1 до заданной глубины
        for (int currentDepth = 1; currentDepth <= maxDepth; currentDepth++)
        {
            double bestValue = LOSE_SCORE;
            Move? currentBestMove = null;

            // УМНАЯ СОРТИРОВКА: Сначала проверяем ЛУЧШИЙ ход с предыдущей итерации, 
            // затем остальные ходы по позиционным весам. Это дает идеальное Alpha-Beta отсечение.
            var sortedMoves = validMoves
                .OrderByDescending(m => (globalBestMove != null && m.Row == globalBestMove.Row && m.Col == globalBestMove.Col) ? 1000000 : Evaluator.Weights[m.Row, m.Col])
                .ToList();

            double alpha = LOSE_SCORE;
            double beta = WIN_SCORE;

            foreach (var move in sortedMoves)
            {
                var flipped = board.MakeMoveWithRecord(move.Row, move.Col, aiColor);

                double moveValue = Minimax(board, currentDepth - 1, alpha, beta, false, aiColor, opponentColor);

                board.UndoMove(move.Row, move.Col, aiColor, flipped);

                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    currentBestMove = move;
                }

                alpha = Math.Max(alpha, bestValue);
            }

            // Запоминаем лучший ход текущей глубины, чтобы начать с него на следующей
            globalBestMove = currentBestMove;

            // Оптимизация: Если мы нашли 100% выигрыш (например, мат в 3 хода), 
            // нет смысла считать глубже.
            if (bestValue >= WIN_SCORE) break; 
        }

        return globalBestMove;
    }

    private double Minimax(Board board, int depth, double alpha, double beta, bool isMaximizing, PlayerColor aiColor, PlayerColor currentPlayer)
    {
        long hash = board.GetHash();
        if (_transpositionTable.TryGetValue(hash, out var entry) && entry.Depth >= depth)
        {
            return entry.Score; 
        }

        PlayerColor opponentColor = (aiColor == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;
        PlayerColor nextPlayer = (currentPlayer == PlayerColor.Black) ? PlayerColor.White : PlayerColor.Black;

        bool currentHasMoves = board.HasValidMoves(currentPlayer);
        bool nextHasMoves = board.HasValidMoves(nextPlayer);

        if (!currentHasMoves && !nextHasMoves)
        {
            int aiScore = board.GetScore(aiColor);
            int oppScore = board.GetScore(opponentColor);
            
            double finalScore = 0;
            if (aiScore > oppScore) finalScore = WIN_SCORE + aiScore; 
            else if (oppScore > aiScore) finalScore = LOSE_SCORE - oppScore; 
            
            _transpositionTable[hash] = new TTEntry { Score = finalScore, Depth = depth };
            return finalScore; 
        }

        if (depth == 0)
        {
            double evalScore = Evaluator.Evaluate(board, aiColor);
            _transpositionTable[hash] = new TTEntry { Score = evalScore, Depth = 0 };
            return evalScore;
        }

        if (!currentHasMoves)
        {
            double skipScore = Minimax(board, depth - 1, alpha, beta, !isMaximizing, aiColor, nextPlayer);
            _transpositionTable[hash] = new TTEntry { Score = skipScore, Depth = depth };
            return skipScore;
        }

        var validMoves = board.GetValidMoves(currentPlayer)
                              .OrderByDescending(m => Evaluator.Weights[m.Row, m.Col])
                              .ToList();

        double res;
        if (isMaximizing)
        {
            double maxEval = LOSE_SCORE;
            foreach (var move in validMoves)
            {
                var flipped = board.MakeMoveWithRecord(move.Row, move.Col, currentPlayer);
                double eval = Minimax(board, depth - 1, alpha, beta, false, aiColor, nextPlayer);
                board.UndoMove(move.Row, move.Col, currentPlayer, flipped);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break; 
            }
            res = maxEval;
        }
        else 
        {
            double minEval = WIN_SCORE;
            foreach (var move in validMoves)
            {
                var flipped = board.MakeMoveWithRecord(move.Row, move.Col, currentPlayer);
                double eval = Minimax(board, depth - 1, alpha, beta, true, aiColor, nextPlayer);
                board.UndoMove(move.Row, move.Col, currentPlayer, flipped);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break; 
            }
            res = minEval;
        }

        _transpositionTable[hash] = new TTEntry { Score = res, Depth = depth };
        return res;
    }
}