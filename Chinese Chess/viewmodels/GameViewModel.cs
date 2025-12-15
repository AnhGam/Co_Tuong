using Chinese_Chess.Helpers;
using Chinese_Chess.Models;
using Chinese_Chess.Models.Chinese_Chess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Chinese_Chess.Services;

namespace Chinese_Chess.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Piece> Pieces { get; set; }
        public BoardState BoardLogic { get; set; }

        private Stack<Move> _redoStack = new Stack<Move>();

        public int LoadedTime { get; set; } = 0;
        public Action<int> OnGameLoaded;

        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();
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
            _botService.OnLog = (msg) =>
            {
                // Chỉ in ra màn hình Output của Visual Studio (View -> Output)
                System.Diagnostics.Debug.WriteLine($"[SERVICE LOG]: {msg}");
            };
            StartNewGame();
            AddToChat("Trò chơi bắt đầu!", MessageType.System);
        }

        public void StartNewGame()
        {
            BoardLogic.Reset();
            Pieces.Clear();
            CapturedRedPieces.Clear();
            CapturedBlackPieces.Clear();
            ValidMoves.Clear();
            _redoStack.Clear();
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
            if (BoardLogic.CurrentTurn == PieceColor.Black) return;
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
                ExecuteMove(move, clickedPiece); 
                TriggerBotTurn();
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
            AudioHelper.PlaySFX("Play.mp3");

            _redoStack.Clear();
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
                AddToChat($"KẾT THÚC: {winner} CHIẾN THẮNG!", MessageType.System);
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

        public void Undo()
        {
            var move = BoardLogic.UndoLastMove();
            if (move == null) return; 

            if (move.CapturedPiece != null)
            {
                if (move.CapturedPiece.Color == PieceColor.Black)
                    CapturedRedPieces.Remove(move.CapturedPiece);
                else
                    CapturedBlackPieces.Remove(move.CapturedPiece);
            }

            _redoStack.Push(move);

            _selectedPiece = null;
            ValidMoves.Clear();
            CheckGameState(); 
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            var move = _redoStack.Pop();

            var target = BoardLogic.GetPieceAt(move.ToX, move.ToY);

            if (target != null)
            {
                target.IsAlive = false;
                if (target.Color == PieceColor.Black) CapturedRedPieces.Add(target);
                else CapturedBlackPieces.Add(target);
            }

            BoardLogic.MovePiece(move.MovedPiece, move.ToX, move.ToY);

            CheckGameState();
        }

        public void AddToChat(string text, MessageType type, string sender = "System")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add(new ChatMessage
                {
                    Time = DateTime.Now,
                    Sender = sender,
                    Text = text,
                    Type = type
                });
            });
        }


        private const string AutoSaveFile = "autosave.json";
        public void SaveGame(int currentTimeSeconds, string filePath = null)
        {


            var saveData = new GameSaveData
            {
                GameTimeSeconds = currentTimeSeconds,
                CurrentTurn = BoardLogic.CurrentTurn.ToString(),
                ChatHistory = new List<ChatMessage>(ChatMessages)
            };


            foreach (var p in BoardLogic.Pieces)
            {
                saveData.AllPieces.Add(new PieceRecord
                {
                    Id = p.Id,
                    Type = p.Type,
                    Color = p.Color,
                    X = p.X,
                    Y = p.Y,
                    IsAlive = p.IsAlive,
                    ImagePath = p.ImagePath
                });
            }


            foreach (var m in BoardLogic.Moves)
            {
                saveData.MoveHistory.Add(ConvertToRecord(m));
            }


            foreach (var m in _redoStack)
            {
                saveData.RedoStack.Add(ConvertToRecord(m));
            }

            string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });

            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllText(filePath, jsonString);
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Chinese Chess Save (*.json)|*.json";
                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, jsonString);
                    MessageBox.Show("Đã lưu game thành công!", "THÔNG BÁO");
                }
            }
        }

        public bool LoadGame(string filePath = null)
        {
            string content = "";

            if (!string.IsNullOrEmpty(filePath))
            {
                if (!File.Exists(filePath)) return false;
                content = File.ReadAllText(filePath);
            }
            else
            {
                // Load thủ công
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Chinese Chess Save (*.json)|*.json";
                if (dialog.ShowDialog() == true)
                {
                    content = File.ReadAllText(dialog.FileName);
                }
                else return false; 
            }

            try
            {
                var saveData = JsonSerializer.Deserialize<GameSaveData>(content);
                RestoreGameFromData(saveData); 
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Hàm hỗ trợ: Chuyển Move object thành MoveRecord (để lưu)
        private MoveRecord ConvertToRecord(Move m)
        {
            return new MoveRecord
            {
                PieceId = m.MovedPiece.Id,
                CapturedPieceId = m.CapturedPiece?.Id, // Có thể null
                FromX = m.FromX,
                FromY = m.FromY,
                ToX = m.ToX,
                ToY = m.ToY
            };
        }

        // Hàm hỗ trợ: Khôi phục game từ dữ liệu Save
        private void RestoreGameFromData(GameSaveData data)
        {
            Pieces.Clear();
            CapturedRedPieces.Clear();
            CapturedBlackPieces.Clear();
            BoardLogic.Reset();
            _redoStack.Clear();
            ChatMessages.Clear();

            // 2. Khôi phục Chat
            foreach (var chat in data.ChatHistory) ChatMessages.Add(chat);

            // 3. Khôi phục Quân cờ
            var restoredPieces = new List<Piece>();
            foreach (var rec in data.AllPieces)
            {
                var p = new Piece
                {
                    Id = rec.Id, 
                    Type = rec.Type,
                    Color = rec.Color,
                    X = rec.X,
                    Y = rec.Y,
                    IsAlive = rec.IsAlive,
                    ImagePath = rec.ImagePath
                };

                restoredPieces.Add(p);
                Pieces.Add(p); 

                
                if (!p.IsAlive)
                {
                    if (p.Color == PieceColor.Black) CapturedRedPieces.Add(p);
                    else CapturedBlackPieces.Add(p);
                }
            }
            BoardLogic.Pieces = restoredPieces; 

            // 4. Khôi phục Lượt đi
            BoardLogic.CurrentTurn = (PieceColor)Enum.Parse(typeof(PieceColor), data.CurrentTurn);

            // 5. Khôi phục Lịch sử Moves
            
            foreach (var rec in data.MoveHistory)
            {
                var movedPiece = restoredPieces.FirstOrDefault(p => p.Id == rec.PieceId);
                var capturedPiece = restoredPieces.FirstOrDefault(p => p.Id == rec.CapturedPieceId);

                if (movedPiece != null)
                {
                    var move = new Move(movedPiece, rec.FromX, rec.FromY, rec.ToX, rec.ToY, capturedPiece);
                    BoardLogic.Moves.Add(move);
                }
            }

            // 6. Khôi phục Redo Stack 
            var redoList = new List<Move>();
            foreach (var rec in data.RedoStack)
            {
                var movedPiece = restoredPieces.FirstOrDefault(p => p.Id == rec.PieceId);
                var capturedPiece = restoredPieces.FirstOrDefault(p => p.Id == rec.CapturedPieceId);
                if (movedPiece != null)
                {
                    redoList.Add(new Move(movedPiece, rec.FromX, rec.FromY, rec.ToX, rec.ToY, capturedPiece));
                }
            }
            // Đẩy vào stack 
            for (int i = redoList.Count - 1; i >= 0; i--)
            {
                _redoStack.Push(redoList[i]);
            }
            LoadedTime = data.GameTimeSeconds;
            OnGameLoaded?.Invoke(LoadedTime);

            CheckGameState();
        }

        private EngineService _botService = new EngineService();
        public int Difficulty { get; set; } = 1;

        private async void TriggerBotTurn()
        {
            if (GameStatus.Contains("THẮNG")) return;

            // Delay 1 giây fake
            int delayTime = 500; 
            if(Difficulty == 3) delayTime = 1000;
            await Task.Delay(delayTime);

            // điều chỉnh độ khó (depth = số nước nhìn trước)
            int depth = 2;
            switch (Difficulty)
            {
                case 1: depth = 2; break;
                case 2: depth = 5; break;
                case 3: depth = 9; break;
            }
            try
            {
                string fen = FenHelper.GetFen(BoardLogic);
                var botMove = await _botService.GetBestMoveAsync(fen, depth);

                if (botMove != null)
                {
                    var pieceToMove = BoardLogic.GetPieceAt(botMove.FromX, botMove.FromY);
                    var targetPiece = BoardLogic.GetPieceAt(botMove.ToX, botMove.ToY);

                    if (pieceToMove != null)
                    {
                        var move = new Move(pieceToMove, botMove.FromX, botMove.FromY, botMove.ToX, botMove.ToY);
                        ExecuteMove(move, targetPiece);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bot gặp lỗi: " + ex.Message);
            }
        }

        public void StopGame()
        {
            _botService.StopEngine();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}