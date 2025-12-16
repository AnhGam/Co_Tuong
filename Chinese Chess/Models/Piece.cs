using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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



    public class Piece : INotifyPropertyChanged
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }

        private int _x;
        public int X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); } // Cập nhật vị trí
        }

        private int _y;
        public int Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); } // Cập nhật vị trí
        }

        private bool _isAlive;
        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                if (_isAlive != value)
                {
                    _isAlive = value;
                    OnPropertyChanged(); // Cập nhật sống/chết
                }
            }
        }

        public string ImagePath { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Piece Clone()
        {
            return new Piece
            {
                Id = this.Id,
                Type = this.Type,
                Color = this.Color,
                X = this.X,
                Y = this.Y,
                IsAlive = this.IsAlive,
                ImagePath = this.ImagePath
            };
        }
    }
}