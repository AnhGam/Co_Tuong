using Chinese_Chess.Models;
using Chinese_Chess.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace Chinese_Chess.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Piece> Pieces { get; set; }
        public BoardState BoardLogic { get; set; }

        public ObservableCollection<HighlightSpot> ValidMoves { get; set; } = new ObservableCollection<HighlightSpot>();
        public ObservableCollection<Piece> CapturedRedPieces { get; set; } = new ObservableCollection<Piece>();
        public ObservableCollection<Piece> CapturedBlackPieces { get; set; } = new ObservableCollection<Piece>();

        private Piece _selectedPiece;

        private string _gameStatus;
        public string GameStatus
        {
            get => _gameStatus;
            set { _gameStatus = value; OnPropertyChanged(); }
        }

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
            CapturedRedPieces.Clear();
            CapturedBlackPieces.Clear();
            ValidMoves.Clear();
            GameStatus = "Đỏ đi trước";

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
                    SelectPiece(clickedPiece);
                    ShowValidMoves(_selectedPiece);
                }
                return;
            }
            if (clickedPiece == _selectedPiece)
            {
                ClearSelection();
                return;
            }
            if (clickedPiece != null && clickedPiece.Color == BoardLogic.CurrentTurn)
            {
                SelectPiece(clickedPiece);
                ShowValidMoves(_selectedPiece);
                return;
            }
            var move = new Move(_selectedPiece, _selectedPiece.X, _selectedPiece.Y, x, y);

            if (MoveValidator.IsValidMove(BoardLogic, move))
            {
                if (clickedPiece != null)
                {
                    clickedPiece.IsAlive = false; 
                    if (clickedPiece.Color == PieceColor.Black)
                        CapturedRedPieces.Add(clickedPiece);   
                    else
                        CapturedBlackPieces.Add(clickedPiece);
                }
                BoardLogic.MovePiece(_selectedPiece, x, y);
                ClearSelection();
                CheckGameState();
            }
            else
            {
                ClearSelection();
            }
        }
        private void ShowValidMoves(Piece p)
        {
            ValidMoves.Clear();
            // Lấy tất cả nước đi an toàn của quân này
            var moves = MoveValidator.GetSafeMoves(BoardLogic, p);

            foreach (var m in moves)
            {
                ValidMoves.Add(new HighlightSpot { X = m.x, Y = m.y });
            }
        }
        private void SelectPiece(Piece p)
        {
            if (_selectedPiece != null) _selectedPiece.IsSelected = false;
            _selectedPiece = p;
            _selectedPiece.IsSelected = true;
        }

        private void ClearSelection()
        {
            if (_selectedPiece != null) _selectedPiece.IsSelected = false;
            _selectedPiece = null;
            ValidMoves.Clear();
        }

        private void ExecuteMove(Move move, Piece target)
        {

            if (target != null)
            {
                target.IsAlive = false; 

                if (target.Color == PieceColor.Black) CapturedRedPieces.Add(target);
                else CapturedBlackPieces.Add(target);
            }

            BoardLogic.MovePiece(move.MovedPiece, move.ToX, move.ToY);
            ClearSelection();
            CheckGameState();
        }

        private void CheckGameState()
        {
            var nextTurn = BoardLogic.CurrentTurn;

            bool canMove = MoveValidator.HasAnyValidMove(BoardLogic, nextTurn);

            if (!canMove)
            {
                string winner = (nextTurn == PieceColor.Red ? "ĐEN" : "ĐỎ");
                GameStatus = $"HẾT CỜ! {winner} THẮNG";
                MessageBox.Show(GameStatus);
                return;
            }

            bool isCheck = MoveValidator.IsInCheck(BoardLogic, nextTurn);
            if (isCheck)
            {
                GameStatus = "CHIẾU TƯỚNG!";
            }
            else
            {
                GameStatus = nextTurn == PieceColor.Red ? "Lượt Đỏ" : "Lượt Đen";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}