using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Models
{
    public class Move
    {
        public Piece MovedPiece { get; set; }
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }

        // Nếu ăn quân
        public Piece CapturedPiece { get; set; }

        // Thời điểm hoặc số thứ tự
        public int MoveNumber { get; set; }

        public Move(Piece piece, int fromX, int fromY, int toX, int toY, Piece captured = null, int moveNumber = 0)
        {
            MovedPiece = piece;
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
            CapturedPiece = captured;
            MoveNumber = moveNumber;
        }

        public override string ToString()
        {
            string action = CapturedPiece != null ? "ăn" : "đi";
            return $"{MoveNumber}. {MovedPiece.Color} {MovedPiece.Type} {action} ({FromX},{FromY}) → ({ToX},{ToY})";
        }
    }
}
