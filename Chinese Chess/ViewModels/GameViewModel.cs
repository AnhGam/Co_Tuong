using Chinese_Chess.Models;
using Chinese_Chess.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows;

namespace Chinese_Chess.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // Danh sách quân cờ (Binding ra View)
        public ObservableCollection<Piece> Pieces { get; set; }
        public BoardState BoardLogic { get; set; }

        private Piece _selectedPiece; // Quân đang chọn

        public GameViewModel()
        {
            BoardLogic = new BoardState();
            Pieces = new ObservableCollection<Piece>();
            StartNewGame();
        }

        public void StartNewGame()
        {
            BoardLogic.Reset();
            Pieces.Clear();
            InitStandardBoard();
            BoardLogic.Pieces = Pieces.ToList();
        }

        // Khởi tạo 32 quân cờ
        private void InitStandardBoard()
        {
            // 1. QUÂN ĐEN (Trên)
            AddPiece(PieceType.Rook, PieceColor.Black, 0, 0);
            AddPiece(PieceType.Horse, PieceColor.Black, 1, 0);
            AddPiece(PieceType.Elephant, PieceColor.Black, 2, 0);
            AddPiece(PieceType.Advisor, PieceColor.Black, 3, 0);
            AddPiece(PieceType.General, PieceColor.Black, 4, 0);
            AddPiece(PieceType.Advisor, PieceColor.Black, 5, 0);
            AddPiece(PieceType.Elephant, PieceColor.Black, 6, 0);
            AddPiece(PieceType.Horse, PieceColor.Black, 7, 0);
            AddPiece(PieceType.Rook, PieceColor.Black, 8, 0);
            AddPiece(PieceType.Cannon, PieceColor.Black, 1, 2);
            AddPiece(PieceType.Cannon, PieceColor.Black, 7, 2);
            for (int i = 0; i <= 8; i += 2) AddPiece(PieceType.Soldier, PieceColor.Black, i, 3);

            // 2. QUÂN ĐỎ (Dưới)
            AddPiece(PieceType.Rook, PieceColor.Red, 0, 9);
            AddPiece(PieceType.Horse, PieceColor.Red, 1, 9);
            AddPiece(PieceType.Elephant, PieceColor.Red, 2, 9);
            AddPiece(PieceType.Advisor, PieceColor.Red, 3, 9);
            AddPiece(PieceType.General, PieceColor.Red, 4, 9);
            AddPiece(PieceType.Advisor, PieceColor.Red, 5, 9);
            AddPiece(PieceType.Elephant, PieceColor.Red, 6, 9);
            AddPiece(PieceType.Horse, PieceColor.Red, 7, 9);
            AddPiece(PieceType.Rook, PieceColor.Red, 8, 9);
            AddPiece(PieceType.Cannon, PieceColor.Red, 1, 7);
            AddPiece(PieceType.Cannon, PieceColor.Red, 7, 7);
            for (int i = 0; i <= 8; i += 2) AddPiece(PieceType.Soldier, PieceColor.Red, i, 6);
        }

        private void AddPiece(PieceType type, PieceColor color, int x, int y)
        {
            string imgName = $"{color.ToString().ToLower()}_{type.ToString().ToLower()}_text.png";
            Pieces.Add(new Piece 
            { 
                Type = type, Color = color, X = x, Y = y, 
                ImagePath = $"/Assets/TextPiece/{imgName}", IsAlive = true 
            });
        }

        // Xử lý Click (Logic chọn và đi)
        public void OnTileClicked(int x, int y)
        {
            var clickedPiece = BoardLogic.GetPieceAt(x, y);

            if (_selectedPiece == null)
            {
                if (clickedPiece != null && clickedPiece.Color == BoardLogic.CurrentTurn)
                {
                    _selectedPiece = clickedPiece;
                    MessageBox.Show($"Đã chọn: {clickedPiece.Type} ({x},{y})");
                }
                return;
            }


            if (clickedPiece == _selectedPiece)
            {
                _selectedPiece = null;
                return;
            }


            if (clickedPiece != null && clickedPiece.Color == BoardLogic.CurrentTurn)
            {
                _selectedPiece = clickedPiece;
                MessageBox.Show($"Đổi sang: {clickedPiece.Type}");
                return;
            }


            var move = new Move(_selectedPiece, _selectedPiece.X, _selectedPiece.Y, x, y);
            if (MoveValidator.IsValidMove(BoardLogic, move))
            {
                if (clickedPiece != null) clickedPiece.IsAlive = false; 
                BoardLogic.MovePiece(_selectedPiece, x, y); 
                _selectedPiece = null; 
            }
            else
            {
                MessageBox.Show("Nước đi không hợp lệ!");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}