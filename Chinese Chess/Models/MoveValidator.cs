using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Chinese_Chess.Models
{
    public static class MoveValidator
    {
        public static bool IsValidMove(BoardState boardState, Move move)
        {
            if (move.ToX < 0 || move.ToX > 8 || move.ToY < 0 || move.ToY > 9) return false;
            if (move.ToX == move.FromX && move.ToY == move.FromY) return false;
            var targetPiece = boardState.GetPieceAt(move.ToX, move.ToY);
            if (targetPiece != null && targetPiece.Color == move.MovedPiece.Color) return false;

            bool isMoveLogicValid = false;
            switch (move.MovedPiece.Type)
            {
                case PieceType.General: isMoveLogicValid = IsValidGeneralMove(boardState, move); break;
                case PieceType.Advisor: isMoveLogicValid = IsValidAdvisorMove(boardState, move); break;
                case PieceType.Elephant: isMoveLogicValid = IsValidElephantMove(boardState, move); break;
                case PieceType.Horse: isMoveLogicValid = IsValidHorseMove(boardState, move); break;
                case PieceType.Rook: isMoveLogicValid = IsValidRookMove(boardState, move); break;
                case PieceType.Cannon: isMoveLogicValid = IsValidCannonMove(boardState, move); break;
                case PieceType.Soldier: isMoveLogicValid = IsValidSoldierMove(boardState, move); break;
            }
            if (!isMoveLogicValid) return false;
            if (IfGoCheck(boardState, move)) return false;
            return true;
        }

        private static bool IsPathClear(BoardState boardState, Move move)
        {
            if(CountPiecesInPath(boardState, move) != 0)
                return false;
            return true;
        }

        private static int CountPiecesInPath(BoardState boardState, Move move)
        {
            int count = 0;
            int deltaX = Math.Sign(move.ToX - move.FromX);
            int deltaY = Math.Sign(move.ToY - move.FromY);
            int currentX = move.FromX + deltaX;
            int currentY = move.FromY + deltaY;
            while (currentX != move.ToX || currentY != move.ToY)
            {
                if (boardState.GetPieceAt(currentX, currentY) != null)
                {
                    count++;
                }
                currentX += deltaX;
                currentY += deltaY;
            }
            return count;
        }

        public static bool IfGoCheck(BoardState boardState, Move move)
        {
            var tempBoard = boardState.Clone();
            var movingPiece = tempBoard.GetPieceAt(move.FromX, move.FromY);
            var capturedPiece = tempBoard.GetPieceAt(move.ToX, move.ToY);

            if (capturedPiece != null) capturedPiece.IsAlive = false;
            movingPiece.X = move.ToX;
            movingPiece.Y = move.ToY;

            if (IsInCheck(tempBoard, move.MovedPiece.Color)) return true;
            if (IsGeneralsFacing(tempBoard)) return true;

            return false;
        }
        // Kiểm tra bị chiếu không 
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
            if (move.MovedPiece.Color == PieceColor.Red)
            {
                if (move.ToX < 3 || move.ToX > 5 || move.ToY < 7 || move.ToY > 9) return false;
            }
            else 
            {
                if (move.ToX < 3 || move.ToX > 5 || move.ToY < 0 || move.ToY > 2) return false;
            }
            var dx = Math.Abs(move.ToX - move.FromX);
            var dy = Math.Abs(move.ToY - move.FromY);
            if (dx + dy != 1) return false;
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

        public static bool HasAnyValidMove(BoardState board, PieceColor color)
        {
            var pieces = board.Pieces.Where(p => p.Color == color && p.IsAlive);

            foreach (var piece in pieces)
            {
                if (GetSafeMoves(board, piece).Count > 0)
                    return true;
            }
            return false;
        }

        private static bool IsValidAdvisorMove(BoardState boardState, Move move)
        {
            if (move.MovedPiece.Color == PieceColor.Red)
            {
                if (move.ToX < 3 || move.ToX > 5 || move.ToY < 7 || move.ToY > 9) return false;
            }
            else 
            {
                if (move.ToX < 3 || move.ToX > 5 || move.ToY < 0 || move.ToY > 2) return false;
            }
            if (Math.Abs(move.ToX - move.FromX) != 1 || Math.Abs(move.ToY - move.FromY) != 1) return false;
            return true;
        }
        public static bool IsGeneralsFacing(BoardState board)
        {
            var redGeneral = board.Pieces.FirstOrDefault(p => p.Type == PieceType.General && p.Color == PieceColor.Red && p.IsAlive);
            var blackGeneral = board.Pieces.FirstOrDefault(p => p.Type == PieceType.General && p.Color == PieceColor.Black && p.IsAlive);

            if (redGeneral == null || blackGeneral == null) return false; // Không thể xảy ra trong game chuẩn

            if (redGeneral.X != blackGeneral.X) return false;

            int minYi = Math.Min(redGeneral.Y, blackGeneral.Y);
            int maxYi = Math.Max(redGeneral.Y, blackGeneral.Y);

            int obstacleCount = board.Pieces.Count(p => p.IsAlive && p.X == redGeneral.X && p.Y > minYi && p.Y < maxYi);
            return obstacleCount == 0;
        }
        private static bool IsValidElephantMove(BoardState boardState, Move move)
        {
            if (move.MovedPiece.Color == PieceColor.Red)
            {
                if (move.ToY < 5 || move.ToY > 9) return false;
            }
            else
            {
                if (move.ToY < 0 || move.ToY > 4) return false;
            }

            if (Math.Abs(move.ToX - move.FromX) != 2 || Math.Abs(move.ToY - move.FromY) != 2)
                return false;

            int midX = (move.FromX + move.ToX) / 2;
            int midY = (move.FromY + move.ToY) / 2;
            if (boardState.GetPieceAt(midX, midY) != null)
                return false;
            return true;
        }
        private static bool IsValidHorseMove(BoardState boardState, Move move)
        {
            var dx = move.ToX - move.FromX;
            var dy = move.ToY - move.FromY;
            if (!((Math.Abs(dx) == 1 && Math.Abs(dy) == 2) || (Math.Abs(dx) == 2 && Math.Abs(dy) == 1)))
                return false;
            var direct = Math.Abs(dx)==2 ? Math.Sign(dx) : Math.Sign(dy);
            var blockX = Math.Abs(dx) == 2 ? move.FromX + direct : move.FromX;
            var blockY = Math.Abs(dy) == 2 ? move.FromY + direct : move.FromY;
            if (boardState.GetPieceAt(blockX, blockY) != null)
                return false;
            return true;
        }
        private static bool IsValidRookMove(BoardState boardState, Move move)
        {
            if ((move.ToX != move.FromX && move.ToY != move.FromY) || (move.ToX == move.FromX && move.ToY == move.FromY))
                    return false;
            if (!IsPathClear(boardState, move))
                return false;
            return true;
        }
        private static bool IsValidCannonMove(BoardState boardState, Move move)
        {
            var enemy = boardState.GetPieceAt(move.ToX, move.ToY);
            if (move.ToX != move.FromX && move.ToY != move.FromY)
                    return false;
            if(enemy == null)
            {
                if (!IsPathClear(boardState, move))
                    return false;
            }
            else
            {
                if(CountPiecesInPath(boardState, move) != 1)
                    return false;
            }
            return true;
        }
        private static bool IsValidSoldierMove(BoardState boardState, Move move)
        {
            int dx = move.ToX - move.FromX;
            int dy = move.ToY - move.FromY;
            if (move.MovedPiece.Color == PieceColor.Red)
            {
                if (dy > 0) return false;

                if (move.FromY > 4)
                {
                    if (dx != 0 || dy != -1) return false;
                }

                else
                {
                    if (dy == -1 && dx == 0) return true; 
                    if (dy == 0 && Math.Abs(dx) == 1) return true; 
                    return false; 
                }
            }

            else
            {
                if (dy < 0) return false;
                if (move.FromY < 5)
                {
                    if (dx != 0 || dy != 1) return false;
                }
                else
                {
                    if (dy == 1 && dx == 0) return true;
                    if (dy == 0 && Math.Abs(dx) == 1) return true;
                    return false;
                }
            }
            return true;
        }

    };
}
