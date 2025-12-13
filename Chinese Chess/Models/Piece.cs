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
        // Type và Color không đổi 
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private int _x;
        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(); 
                }
            }
        }

        private int _y;
        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(); 
                }
            }
        }

        private bool _isAlive = true;
        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                if (_isAlive != value)
                {
                    _isAlive = value;
                    OnPropertyChanged(); 
                }
            }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        public Piece Clone()
        {
            return (Piece)this.MemberwiseClone();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}