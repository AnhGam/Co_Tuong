using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Models
{
    public static class MoveValidator
    {
        public static bool IsValidMove(BoardState boardState, Move move)
        {
            if (move.ToX < 0 || move.ToX > 8 || move.ToY < 0 || move.ToY > 9)
            {
                return false;
            }

            var targetPiece = boardState.GetPieceAt(move.ToX, move.ToY);
            if (targetPiece != null && targetPiece.Color == move.MovedPiece.Color)
            {
                return false;
            }

            switch (move.MovedPiece.Type)
            {
                case PieceType.General:
                    return IsValidGeneralMove(boardState, move);

                case PieceType.Advisor:
                    return IsValidAdvisorMove(boardState, move);

                case PieceType.Elephant:
                    return IsValidElephantMove(boardState, move);

                case PieceType.Horse:
                    return IsValidHorseMove(boardState, move);

                case PieceType.Rook:
                    return IsValidRookMove(boardState, move);

                case PieceType.Cannon:
                    return IsValidCannonMove(boardState, move);

                case PieceType.Soldier:
                    return IsValidSoldierMove(boardState, move);

                default:
                    return false;
            }
        }

        private static bool IsPathClear(BoardState boardState, Move move)
        {
            int deltaX = Math.Sign(move.ToX - move.FromX);
            int deltaY = Math.Sign(move.ToY - move.FromY);
            int currentX = move.FromX + deltaX;
            int currentY = move.FromY + deltaY;
            while (currentX != move.ToX || currentY != move.ToY)
            {
                if (boardState.GetPieceAt(currentX, currentY) != null)
                {
                    return false;
                }
                currentX += deltaX;
                currentY += deltaY;
            }
            return true;
        }

        // Kiểm tra xem tướng có bị chiếu không sau nước đi (cần fix)
        public static bool IsInCheck(BoardState boardState, PieceColor color,int ToX, int ToY)
        {
            var general = boardState.Pieces.FirstOrDefault(p => p.Type == PieceType.General && p.Color == color && p.IsAlive);
            if (general == null) return false;
            foreach (var piece in boardState.Pieces.Where(p => p.Color != color && p.IsAlive))
            {
                var hypotheticalMove = new Move(piece, piece.X, piece.Y, ToX, ToY);
                if (IsValidMove(boardState, hypotheticalMove))
                {
                    return true;
                }
            }
            return false;
        }
        // Kiểm tra bị chiếu không (cần fix)
        public static bool IsInCheck(BoardState boardState, PieceColor color)
        {
            var general = boardState.Pieces.FirstOrDefault(p => p.Type == PieceType.General && p.Color == color && p.IsAlive);
            if (general == null) return false;
            foreach (var piece in boardState.Pieces.Where(p => p.Color != color && p.IsAlive))
            {
                var hypotheticalMove = new Move(piece, piece.X, piece.Y, general.X, general.Y);
                if (IsValidMove(boardState, hypotheticalMove))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsValidGeneralMove(BoardState boardState, Move move)
        {
            // Implement General move validation logic
            if(move.ToX<3 || move.ToX>5 || move.ToY <7 || move.ToY>9)
                return false;

            return true;
        }

        public static List<(int x, int y)> GetSafeMoves(BoardState board, Piece piece)
        {
            var safeMoves = new List<(int, int)>();

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    var move = new Move(piece, piece.X, piece.Y, x, y);

                    if (!IsValidMove(board, move))
                        continue;

                    var tempBoard = board.Clone();
                    var tempPiece = tempBoard.GetPieceAt(piece.X, piece.Y);
                    tempBoard.MovePiece(tempPiece, x, y);

                    if (!IsInCheck(tempBoard, piece.Color))
                        safeMoves.Add((x, y));
                }
            }
            return safeMoves;
        }

        private static bool IsValidAdvisorMove(BoardState boardState, Move move)
        {
            // Implement Advisor move validation logic
            return true;
        }
        private static bool IsValidElephantMove(BoardState boardState, Move move)
        {
            // Implement Elephant move validation logic
            return true;
        }
        private static bool IsValidHorseMove(BoardState boardState, Move move)
        {
            // Implement Horse move validation logic
            return true;
        }
        private static bool IsValidRookMove(BoardState boardState, Move move)
        {
            // Implement Rook move validation logic
            return true;
        }
        private static bool IsValidCannonMove(BoardState boardState, Move move)
        {
            // Implement Cannon move validation logic
            return true;
        }
        private static bool IsValidSoldierMove(BoardState boardState, Move move)
        {
            // Implement Soldier move validation logic
            return true;
        }
    };
}
