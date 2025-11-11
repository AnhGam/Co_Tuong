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
            if (move.ToX < 0 || move.ToX > 8 || move.ToY < 0 || move.ToY > 9)
                return false;
            if (move.ToX == move.FromX && move.ToY == move.FromY)
                return false;
            var targetPiece = boardState.GetPieceAt(move.ToX, move.ToY);
            if (targetPiece != null && targetPiece.Color == move.MovedPiece.Color)
                return false;
            if (IfGoCheck(boardState, move))
                return false;

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

        // Kiểm tra xem tướng có bị chiếu không sau nước đi bất kì
        public static bool IfGoCheck(BoardState boardState, Move move)
        {
            var tempBoard = boardState.Clone();

            var movingPiece = tempBoard.GetPieceAt(move.FromX, move.FromY);
            var capturedPiece = tempBoard.GetPieceAt(move.ToX, move.ToY);

            if (capturedPiece != null)
                capturedPiece.IsAlive = false;

            movingPiece.X = move.ToX;
            movingPiece.Y = move.ToY;

            return IsInCheck(tempBoard, move.MovedPiece.Color);
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
            var general = boardState.GetPieceAt(move.FromX, move.FromY);
            var enemyGeneral = boardState.Pieces.FirstOrDefault(p => p.Type == PieceType.General && p.Color != general.Color && p.IsAlive);
            if (move.ToX<3 || move.ToX>5 || move.ToY <7 || move.ToY>9)
                return false;
            if (Math.Abs(move.ToX - move.FromX) + Math.Abs(move.ToY - move.FromY) != 1)
                return false;
            if (enemyGeneral != null && move.ToX == enemyGeneral.X)
            {
                Move GeneralMove = new Move(general, move.FromX, move.FromY, enemyGeneral.X, enemyGeneral.Y);
                if (!IsPathClear(boardState, GeneralMove))
                    return false;
            }
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
        //Advisor sai
        private static bool IsValidAdvisorMove(BoardState boardState, Move move)
        {
            // Implement Advisor move validation logic
            if (move.ToX < 3 || move.ToX > 5 || move.ToY < 7 || move.ToY > 9)
                return false;
            if (Math.Abs(move.ToX - move.FromX) != 1 || Math.Abs(move.ToY - move.FromY) != 1)
                return false;
            return true;
        }
        private static bool IsValidElephantMove(BoardState boardState, Move move)
        {
            // Implement Elephant move validation logic
            if (move.ToX < 0 || move.ToX > 8 || move.ToY < 5 || move.ToY > 9)
                return false;
            if (Math.Abs(move.ToX - move.FromX) != 2 || Math.Abs(move.ToY - move.FromY) != 2)
                return false;
            if(IfGoCheck(boardState, move))
                return false;
            return true;
        }
        private static bool IsValidHorseMove(BoardState boardState, Move move)
        {
            // Implement Horse move validation logic
            var dx = move.ToX - move.FromX;
            var dy = move.ToY - move.FromY;
            if (!((Math.Abs(dx) == 1 && Math.Abs(dy) == 2) || (Math.Abs(dx) == 2 && Math.Abs(dy) == 1)))
                return false;
            var direct = Math.Abs(dx)==2 ? Math.Sign(dx) : Math.Sign(dy);
            return true;
        }
        private static bool IsValidRookMove(BoardState boardState, Move move)
        {
            // Implement Rook move validation logic
            if ((move.ToX != move.FromX && move.ToY != move.FromY) || (move.ToX == move.FromX && move.ToY == move.FromY))
                    return false;
            if (!IsPathClear(boardState, move))
                return false;
            return true;
        }
        private static bool IsValidCannonMove(BoardState boardState, Move move)
        {
            // Implement Cannon move validation logic
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
            // Implement Soldier move validation logic
            if (move.FromY >=5 )
                if(move.ToX != move.FromX || move.ToY-move.FromY != -1)
                    return false;
            if(move.FromY <=4)
            {
                if(move.ToY-move.FromY != 1 && Math.Abs(move.ToX - move.FromX) + Math.Abs(move.ToY - move.FromY) != 1)
                    return false;
            }
            return true;
        }

    };
}
