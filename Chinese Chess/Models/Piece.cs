using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Models
{
    public enum PieceType
    {
        General,   // Tướng
        Advisor,   // Sĩ
        Elephant,  // Tượng
        Horse,     // Mã
        Rook,      // Xe
        Cannon,    // Pháo
        Soldier    // Tốt
    }

    public enum PieceColor
    {
        Red,
        Black
    }

    public class Piece
    {
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }

        // Tọa độ logic trên bàn (0-8, 0-9)
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsAlive { get; set; } = true;

        // Dễ binding ảnh sau này
        public string ImagePath { get; set; } = string.Empty;

        public Piece Clone()
        {
            return (Piece)this.MemberwiseClone();
        }
    }
}
