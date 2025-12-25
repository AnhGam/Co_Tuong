using Chinese_Chess.Helpers;
using Chinese_Chess.Models;
using Chinese_Chess.Models.Chinese_Chess.Models;
using Chinese_Chess.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics; // [DEBUG]
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Chinese_Chess.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // --- PROPERIES ---
        public ObservableCollection<Piece> Pieces { get; set; }
        public BoardState BoardLogic { get; set; }
        private Stack<Move> _redoStack = new Stack<Move>();
        public bool IsGameFinished { get; private set; } = false;
        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();
        public ObservableCollection<HighlightSpot> ValidMoves { get; set; } = new ObservableCollection<HighlightSpot>();
        public ObservableCollection<Piece> CapturedRedPieces { get; set; } = new ObservableCollection<Piece>();
        public ObservableCollection<Piece> CapturedBlackPieces { get; set; } = new ObservableCollection<Piece>();

        private Piece _selectedPiece;

        // --- ONLINE VARS ---
        private OnlineService _onlineService;
        public bool IsOnline { get; set; } = false;

        private bool _isMyTurn = true;
        public bool IsMyTurn
        {
            get => _isMyTurn;
            set { _isMyTurn = value; OnPropertyChanged(); }
        }

        private string _gameStatus;
        public string GameStatus
        {
            get => _gameStatus;
            set { _gameStatus = value; OnPropertyChanged(); }
        }

        private double _boardRotation;
        public double BoardRotation
        {
            get => _boardRotation;
            set { _boardRotation = value; OnPropertyChanged(); }
        }

        // --- BOT / PLAYER INFO ---
        private string _botName = "Bot";
        public string BotName { get => _botName; set { _botName = value; OnPropertyChanged(); } }

        private string _botAvatar = "pack://application:,,,/Chinese Chess;component/Assets/bot.png";
        public string BotAvatar { get => _botAvatar; set { _botAvatar = value; OnPropertyChanged(); } }

        public string PlayerName => AppSettings.PlayerName;
        public string PlayerAvatar => AppSettings.AvatarPath;
        public int Difficulty { get; set; } = 1;

        private EngineService _botService = new EngineService();
        public int LoadedTime { get; set; } = 0;
        public Action<int> OnGameLoaded;
        public event Action<string> OnGameEnded;

        // --- CONSTRUCTOR ---
        public GameViewModel()
        {
            BoardLogic = new BoardState();
            Pieces = new ObservableCollection<Piece>();

            AppSettings.OnSettingsChanged += () =>
            {
                OnPropertyChanged(nameof(PlayerName));
                OnPropertyChanged(nameof(PlayerAvatar));
                RefreshPieceImages();
            };

            StartNewGame();
        }

        // --- START GAME ---
        public void StartNewGame()
        {
            Debug.WriteLine("[GameViewModel] StartNewGame được gọi.");
            BoardLogic.Reset();
            Pieces.Clear();
            CapturedRedPieces.Clear();
            CapturedBlackPieces.Clear();
            ValidMoves.Clear();
            _redoStack.Clear();
            ChatMessages.Clear();

            GameStatus = "Lượt Đỏ";
            IsMyTurn = true;
            IsOnline = false;
            IsGameFinished = false;
            BoardRotation = 0;

            InitBotInfo();
            InitStandardBoard();
            BoardLogic.Pieces = Pieces.ToList();
            RefreshPieceImages();
            AddToChat("Trò chơi bắt đầu!", MessageType.System);
        }

        private void InitBotInfo()
        {
            switch (Difficulty)
            {
                case 1: BotName = "Easy Bot"; break;
                case 2: BotName = "Medium Bot"; break;
                case 3: BotName = "Hard Bot"; break;
                default: BotName = "Bot"; break;
            }
            BotAvatar = "pack://application:,,,/Chinese Chess;component/Assets/bot.png";
        }

        // --- ONLINE LOGIC ---

        public void ContinueOnlineMatch(OnlineService service)
        {
            Debug.WriteLine("[GameViewModel] Bắt đầu thiết lập trận Online...");
            _onlineService = service;
            IsOnline = true;

            // ✅ ĐĂNG KÝ EVENT HANDLERS TRƯỚC
            _onlineService.OnMoveReceived -= HandleOnlineMove;
            _onlineService.OnMoveReceived += HandleOnlineMove;

            _onlineService.OnChatReceived -= HandleOnlineChat;
            _onlineService.OnChatReceived += HandleOnlineChat;

            // ✅ Reset bàn cờ
            StartNewGame();
            IsOnline = true; // Set lại vì StartNewGame reset nó về false
            BotName = _onlineService.OpponentName;
            BotAvatar = "pack://application:,,,/Chinese Chess;component/Assets/bot.png";

            // ✅ Setup Phe
            if (_onlineService.MySide == "RED")
            {
                Debug.WriteLine("[GameViewModel] Phe: ĐỎ (Đi trước)");
                BoardRotation = 0;
                IsMyTurn = true;
                BotName = "Đối thủ (ĐEN)";
                GameStatus = "Lượt Đỏ";
                AddToChat("Bạn cầm quân ĐỎ (Đi trước).", MessageType.System);
            }
            else
            {
                Debug.WriteLine("[GameViewModel] Phe: ĐEN (Đi sau)");
                BoardRotation = 180;
                IsMyTurn = false;
                BotName = "Đối thủ (ĐỎ)";
                GameStatus = "Lượt Đỏ";
                AddToChat("Bạn cầm quân ĐEN (Đi sau).", MessageType.System);
            }

            Debug.WriteLine($"[GameViewModel] Setup xong. MySide={_onlineService.MySide}, Listeners đã sẵn sàng");
        }

        // XỬ LÝ NHẬN MOVE
        private void HandleOnlineMove(string moveStr)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // 1. SURRENDER
                    if (moveStr == "SURRENDER")
                    {
                        StopGame();
                        GameStatus = "ĐỐI THỦ ĐẦU HÀNG!";
                        OnGameEnded?.Invoke("OPPONENT_SURRENDER"); 
                        return;
                    }

                    // 2. DRAW REQUEST
                    if (moveStr == "DRAW_REQUEST")
                    {
                        bool accept = Views.MessageBox.Show($"{_onlineService.OpponentName} xin hòa. Đồng ý?", "CẦU HÒA", "Đồng ý", "Từ chối");
                        if (accept)
                        {
                            _onlineService.SendMove("DRAW_ACCEPT");
                            EndGameAsDraw();
                        }
                        else
                        {
                            _onlineService.SendMove("DRAW_REFUSE");
                        }
                        return;
                    }

                    if (moveStr == "DRAW_ACCEPT") { EndGameAsDraw();  return; }
                    if (moveStr == "DRAW_REFUSE") { Views.MessageBox.Show("Đối thủ từ chối hòa.", "THÔNG BÁO"); return; }

                    // 3. MOVE
                    var parts = moveStr.Split(',');
                    if (parts.Length < 4) return;
                    int fx = int.Parse(parts[0]), fy = int.Parse(parts[1]);
                    int tx = int.Parse(parts[2]), ty = int.Parse(parts[3]);

                    var piece = BoardLogic.GetPieceAt(fx, fy);
                    if (piece != null)
                    {
                        var target = BoardLogic.GetPieceAt(tx, ty);
                        var move = new Move(piece, fx, fy, tx, ty);
                        ExecuteMove(move, target);
                        IsMyTurn = true;
                        CheckGameState();
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"[Move Error] {ex.Message}"); }
            });
        }
        public async void RequestDraw()
        {
            if (!IsOnline || (GameStatus != null && (GameStatus.Contains("THẮNG") || GameStatus.Contains("HÒA")))) return;

            bool confirm = Views.MessageBox.Show(
                "Bạn muốn gửi yêu cầu HÒA cho đối thủ?",
                "Xác nhận",
                "Gửi yêu cầu",
                "Hủy");

            if (confirm)
            {
                await _onlineService.SendMove("DRAW_REQUEST");
                AddToChat("Đã gửi yêu cầu cầu hòa...", MessageType.System);
            }
        }

        private async void SendDrawResponse(string response)
        {
            if (IsOnline) await _onlineService.SendMove(response);
        }

        private void EndGameAsDraw()
        {
            GameStatus = "VÁN ĐẤU HÒA";
            AddToChat("Hai bên đã đồng ý HÒA.", MessageType.System);
            StopGame();
            OnGameEnded?.Invoke("DRAW");
        }

        // XỬ LÝ NHẬN CHAT
        private void HandleOnlineChat(string chatData)
        {
            Debug.WriteLine($"[GameViewModel] HandleOnlineChat được gọi với: {chatData}");
            
            var parts = chatData.Split(new[] { ':' }, 2);
            if (parts.Length < 2) 
            {
                Debug.WriteLine($"[GameViewModel CHAT ERROR] Format sai, expected 'SIDE:message', nhận: {chatData}");
                return;
            }

            string senderSide = parts[0];
            string msg = parts[1];

            Debug.WriteLine($"[GameViewModel CHAT] senderSide={senderSide}, msg={msg}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                AddToChat(msg, MessageType.Bot, senderSide);
            });
        }

        // GỬI CHAT
        public async void SendChatOnline(string message)
        {
            AddToChat(message, MessageType.Player, "Me");

            if (IsOnline && _onlineService != null)
            {
                string data = $"{AppSettings.PlayerName}:{message}";
                Debug.WriteLine($"[GameViewModel] Gửi chat: {data}");
                await _onlineService.SendChat(data);
            }
        }

        // --- GAME LOGIC ---

        public async void OnTileClicked(int x, int y)
        {
            if (IsGameFinished) return;
            if (IsOnline && !IsMyTurn)
            {
                Debug.WriteLine("[GameViewModel] Click chặn: Chưa đến lượt.");
                return;
            }

            var clickedPiece = BoardLogic.GetPieceAt(x, y);

            if (_selectedPiece == null || (clickedPiece != null && clickedPiece.Color == BoardLogic.CurrentTurn))
            {
                if (clickedPiece != null && clickedPiece.Color == BoardLogic.CurrentTurn)
                {
                    SelectPiece(clickedPiece);
                    ShowValidMoves(_selectedPiece);
                }
                return;
            }

            // Thực hiện đi quân
            var move = new Move(_selectedPiece, _selectedPiece.X, _selectedPiece.Y, x, y);

            if (MoveValidator.IsValidMove(BoardLogic, move))
            {
                ExecuteMove(move, clickedPiece);

                if (IsOnline)
                {
                    IsMyTurn = false;
                    string moveStr = $"{move.FromX},{move.FromY},{move.ToX},{move.ToY}";
                    Debug.WriteLine($"[GameViewModel] Người chơi đi. Gửi mạng: {moveStr}");
                    await _onlineService.SendMove(moveStr);
                }
                else
                {
                    TriggerBotTurn();
                }
            }
            else
            {
                ClearSelection();
            }
        }

        private void ExecuteMove(Move move, Piece target)
        {
            if (target != null)
            {
                if (target.Color == PieceColor.Black) CapturedRedPieces.Add(target);
                else CapturedBlackPieces.Add(target);
            }

            BoardLogic.MovePiece(move.MovedPiece, move.ToX, move.ToY);
            AudioHelper.PlaySFX("Play.mp3");

            _redoStack.Clear();
            ClearSelection();
            // CheckGameState gọi sau
        }

        private void CheckGameState()
        {
            var nextTurn = BoardLogic.CurrentTurn;
            GameStatus = (nextTurn == PieceColor.Red) ? "Lượt Đỏ" : "Lượt Đen";
            if (!MoveValidator.HasAnyValidMove(BoardLogic, nextTurn))
            {
                string winnerSide = (nextTurn == PieceColor.Red) ? "ĐEN" : "ĐỎ";

                string resultSignal = "";

                if (IsOnline)
                {
                    if (_onlineService.MySide == "RED") resultSignal = (winnerSide == "ĐEN") ? "LOSE" : "WIN";
                    else resultSignal = (winnerSide == "ĐỎ") ? "LOSE" : "WIN";
                }
                else
                {
                    resultSignal = (winnerSide == "ĐEN") ? "LOSE" : "WIN";
                }

                GameStatus = $"HẾT CỜ! {winnerSide} THẮNG";
                IsGameFinished = true;
                StopGame();
                OnGameEnded?.Invoke(resultSignal);
            }
            else if (MoveValidator.IsInCheck(BoardLogic, nextTurn)) GameStatus = "CHIẾU TƯỚNG!";
        }

        // --- HELPERS ---

        private async void TriggerBotTurn()
        {
            if( IsGameFinished) return; 
            if (GameStatus.Contains("THẮNG")) return;

            int delayTime = (Difficulty == 3) ? 1000 : 500;
            await Task.Delay(delayTime);

            int depth = (Difficulty == 1) ? 2 : (Difficulty == 2 ? 5 : 7);

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
                        CheckGameState();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bot lỗi: " + ex.Message);
            }
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

        private void InitStandardBoard()
        {
            // QUÂN ĐEN
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

            // QUÂN ĐỎ
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
            string suffix = AppSettings.PieceStyleSuffix;
            string imgName = $"{color.ToString().ToLower()}_{type.ToString().ToLower()}{suffix}.png";
            string folder = (suffix == "_text") ? "TextPiece" : "ImagePiece";

            Pieces.Add(new Piece
            {
                Type = type,
                Color = color,
                X = x,
                Y = y,
                ImagePath = $"pack://application:,,,/Chinese Chess;component/Assets/{folder}/{imgName}",
                IsAlive = true
            });
        }

        public void RefreshPieceImages()
        {
            string suffix = AppSettings.PieceStyleSuffix;
            string folder = (suffix == "_text") ? "TextPiece" : "ImagePiece";
            var tempList = Pieces.ToList();
            Pieces.Clear();
            foreach (var p in tempList)
            {
                string imgName = $"{p.Color.ToString().ToLower()}_{p.Type.ToString().ToLower()}{suffix}.png";
                p.ImagePath = $"pack://application:,,,/Chinese Chess;component/Assets/{folder}/{imgName}";
                Pieces.Add(p);
            }
        }

        private void ShowValidMoves(Piece p)
        {
            ValidMoves.Clear();
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

        public void StopGame()
        {
            IsGameFinished = true;
            if (!IsOnline) _botService.StopEngine();
        }

        public async void Surrender()
        {
            if( IsGameFinished) return;
            if (GameStatus != null && (GameStatus.Contains("THẮNG") || GameStatus.Contains("HÒA") || GameStatus.Contains("THUA"))) return;

            if (IsOnline)
            {
                await _onlineService.SendMove("SURRENDER");
            }

            GameStatus = "BẠN ĐÃ ĐẦU HÀNG";
            AddToChat("Bạn đã đầu hàng.", MessageType.System);
            IsGameFinished = true;
            StopGame();
            OnGameEnded?.Invoke("SELF_SURRENDER");
        }

        // --- UNDO / REDO / SAVE / LOAD ---
        public void Undo()
        {
            if (IsOnline && !IsGameFinished) return;
            int steps = (BoardLogic.Moves.Count < 2) ? 1 : 2;
            for (int i = 0; i < steps; i++)
            {
                var m = BoardLogic.UndoLastMove();
                if (m != null)
                {
                    if (m.CapturedPiece != null)
                    {
                        if (m.CapturedPiece.Color == PieceColor.Black) CapturedRedPieces.Remove(m.CapturedPiece);
                        else CapturedBlackPieces.Remove(m.CapturedPiece);
                    }
                    _redoStack.Push(m);
                }
            }
            ClearSelection();
            CheckGameState();
        }

        public void Redo()
        {
            if (IsOnline && !IsGameFinished) return;
            if (_redoStack.Count == 0) return;

            while (_redoStack.Count > 0)
            {
                var move = _redoStack.Pop();
                var target = BoardLogic.GetPieceAt(move.ToX, move.ToY);
                if (target != null)
                {
                    if (target.Color == PieceColor.Black) CapturedRedPieces.Add(target);
                    else CapturedBlackPieces.Add(target);
                }
                BoardLogic.MovePiece(move.MovedPiece, move.ToX, move.ToY);
                if (BoardLogic.CurrentTurn == PieceColor.Red) break;
            }
            CheckGameState();
            if (BoardLogic.CurrentTurn == PieceColor.Black && _redoStack.Count == 0)
            {
                TriggerBotTurn();
            }
        }

        private const string AutoSaveFile = "autosave.json";
        public void SaveGame(int currentTimeSeconds, string filePath = null)
        {
            if (IsOnline) { return; }
            var saveData = new GameSaveData
            {
                GameTimeSeconds = currentTimeSeconds,
                CurrentTurn = BoardLogic.CurrentTurn.ToString(),
                Difficulty = this.Difficulty,
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

            foreach (var m in BoardLogic.Moves) saveData.MoveHistory.Add(ConvertToRecord(m));
            foreach (var m in _redoStack) saveData.RedoStack.Add(ConvertToRecord(m));

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
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Chinese Chess Save (*.json)|*.json";
                if (dialog.ShowDialog() == true) content = File.ReadAllText(dialog.FileName);
                else return false;
            }

            try
            {
                var saveData = JsonSerializer.Deserialize<GameSaveData>(content);
                RestoreGameFromData(saveData);
                return true;
            }
            catch { return false; }
        }

        private MoveRecord ConvertToRecord(Move m)
        {
            return new MoveRecord
            {
                PieceId = m.MovedPiece.Id,
                CapturedPieceId = m.CapturedPiece?.Id,
                FromX = m.FromX,
                FromY = m.FromY,
                ToX = m.ToX,
                ToY = m.ToY
            };
        }

        private void RestoreGameFromData(GameSaveData data)
        {
            Pieces.Clear(); CapturedRedPieces.Clear(); CapturedBlackPieces.Clear();
            BoardLogic.Reset(); _redoStack.Clear(); ChatMessages.Clear();

            this.Difficulty = data.Difficulty;
            foreach (var chat in data.ChatHistory) ChatMessages.Add(chat);

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
            BoardLogic.CurrentTurn = (PieceColor)Enum.Parse(typeof(PieceColor), data.CurrentTurn);

            foreach (var rec in data.MoveHistory)
            {
                var moved = restoredPieces.FirstOrDefault(p => p.Id == rec.PieceId);
                var captured = restoredPieces.FirstOrDefault(p => p.Id == rec.CapturedPieceId);
                if (moved != null) BoardLogic.Moves.Add(new Move(moved, rec.FromX, rec.FromY, rec.ToX, rec.ToY, captured));
            }

            var redoList = new List<Move>();
            foreach (var rec in data.RedoStack)
            {
                var moved = restoredPieces.FirstOrDefault(p => p.Id == rec.PieceId);
                var captured = restoredPieces.FirstOrDefault(p => p.Id == rec.CapturedPieceId);
                if (moved != null) redoList.Add(new Move(moved, rec.FromX, rec.FromY, rec.ToX, rec.ToY, captured));
            }
            for (int i = redoList.Count - 1; i >= 0; i--) _redoStack.Push(redoList[i]);

            LoadedTime = data.GameTimeSeconds;
            OnGameLoaded?.Invoke(LoadedTime);
            CheckGameState();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}