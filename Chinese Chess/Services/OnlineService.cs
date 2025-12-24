using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Firebase.Database.Streaming;
using Firebase.Database.Offline;

public class GameState
{
    public string RedPlayer { get; set; }
    public string BlackPlayer { get; set; }
    public string LastMove { get; set; }
    public string LastChat { get; set; }
}

namespace Chinese_Chess.Services
{
    public class OnlineService
    {
        private readonly FirebaseClient _firebase;
        private const string DbUrl = "https://chinesechess-6ee4e-default-rtdb.asia-southeast1.firebasedatabase.app/";

        public string CurrentGameId { get; private set; }
        public string MySide { get; private set; }

        public event Action<string> OnGameStarted;
        public event Action<string> OnMoveReceived;
        public event Action<string> OnChatReceived;

        private IDisposable _matchListener;
        private IDisposable _moveListener;
        private IDisposable _chatListener;

        private string _myMatchmakingId;
        private bool _isMatchFound = false;

        private string _lastProcessedMove = "";
        private string _lastProcessedChat = "";

        public OnlineService()
        {
            _firebase = new FirebaseClient(DbUrl);
        }

        public async Task FindMatch(string playerName)
        {
            _isMatchFound = false;
            Debug.WriteLine($"[OnlineService] Bắt đầu tìm trận: {playerName}");

            try
            {
                var queue = await _firebase.Child("matchmaking").OnceAsync<MatchRequest>();
                var opponent = queue.FirstOrDefault();

                if (opponent != null)
                {
                    // --- JOINER (BLACK) ---
                    Debug.WriteLine($"[OnlineService] Tìm thấy đối thủ: {opponent.Object.Name}");
                    MySide = "BLACK";
                    string gameId = Guid.NewGuid().ToString().Substring(0, 8);

                    await _firebase.Child("games").Child(gameId).PatchAsync(new
                    {
                        RedPlayer = opponent.Object.Name,
                        BlackPlayer = playerName,
                        LastMove = "INIT",
                        LastChat = "INIT"
                    });

                    await _firebase.Child("matchmaking").Child(opponent.Key).PatchAsync(new { GameId = gameId });

                    CurrentGameId = gameId;
                    // ✅ KỚI ĐỘNG LISTENERS NGAY LẬP TỨC
                    //StartListeningToGame(gameId);
                    StartPollingGame(gameId);
                    // Delay một chút để đảm bảo listeners đã bắt đầu
                    await Task.Delay(500);
                    
                    OnGameStarted?.Invoke(gameId);
                }
                else
                {
                    // --- WAITER (RED) ---
                    Debug.WriteLine("[OnlineService] Tạo phòng chờ mới...");
                    MySide = "RED";
                    _myMatchmakingId = Guid.NewGuid().ToString();

                    await _firebase.Child("matchmaking").Child(_myMatchmakingId).PutAsync(new MatchRequest
                    {
                        Name = playerName,
                        GameId = ""
                    });

                    _matchListener = _firebase.Child("matchmaking")
                        .AsObservable<MatchRequest>()
                        .Subscribe(async d =>
                        {
                            try
                            {
                                if (_isMatchFound) return;
                                if (d.Key == _myMatchmakingId && d.Object != null && !string.IsNullOrEmpty(d.Object.GameId))
                                {
                                    _isMatchFound = true;
                                    CurrentGameId = d.Object.GameId;
                                    Debug.WriteLine($"[OnlineService] Đối thủ đã vào! GameID: {CurrentGameId}");

                                    DeleteMyMatchRequestSafe();
                                    // ✅ KỚI ĐỘNG LISTENERS NGAY LẬP TỨC
                                    //StartListeningToGame(CurrentGameId);
                                    StartPollingGame(CurrentGameId);
                                    await Task.Delay(500);

                                    OnGameStarted?.Invoke(CurrentGameId);
                                }
                            }
                            catch (Exception ex) { Debug.WriteLine($"[MatchListener Error] {ex.Message}"); }
                        });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lỗi FindMatch] {ex.Message}");
                throw;
            }
        }

        private async void DeleteMyMatchRequestSafe()
        {
            try { if (!string.IsNullOrEmpty(_myMatchmakingId)) await _firebase.Child("matchmaking").Child(_myMatchmakingId).DeleteAsync(); } catch { }
        }

        private void MoveListener(FirebaseEvent<string> d)
        {
            try
            {
                string moveData = d.Object;
                Debug.WriteLine($"[MOVE RAW] Nhận: {moveData}");

                if (string.IsNullOrEmpty(moveData))
                {
                    Debug.WriteLine("[MOVE] moveData is null/empty");
                    return;
                }

                if (moveData == "INIT")
                {
                    Debug.WriteLine("[MOVE] moveData is INIT");
                    return;
                }

                if (moveData == _lastProcessedMove)
                {
                    Debug.WriteLine($"[MOVE] Đã xử lý: {moveData}");
                    return;
                }

                var parts = moveData.Split('|');
                if (parts.Length < 2)
                {
                    Debug.WriteLine($"[MOVE ERROR] Format sai: {moveData}");
                    return;
                }

                string senderSide = parts[0];
                string data = parts[1];

                if (senderSide == MySide)
                {
                    Debug.WriteLine($"[MOVE] Bỏ qua move của chính mình: {senderSide}");
                    return;
                }

                _lastProcessedMove = moveData;
                Debug.WriteLine($"[NHẬN MOVE] {senderSide} đã đi: {data}");
                OnMoveReceived?.Invoke(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MOVE ERROR] {ex.Message}");
            }
        }

        private void HandleIncomingMove(string moveData)
        {
            if (string.IsNullOrEmpty(moveData) || moveData == "INIT") return;
            if (moveData == _lastProcessedMove) return; // Ignore duplicates

            Debug.WriteLine($"[POLLING RECEIVE] {moveData}");

            var parts = moveData.Split('|');
            if (parts.Length < 2) return;

            string senderSide = parts[0];
            string moveStr = parts[1];

            if (senderSide == MySide) return; // Ignore own echo

            _lastProcessedMove = moveData;
            OnMoveReceived?.Invoke(moveStr);
        }

        private bool _isPolling = false;

        public void StartPollingGame(string gameId)
        {
            // 1. SAFETY CHECK: Ensure gameId is not null before starting the task
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.WriteLine("[POLLING ERROR] GameId is NULL. Cannot start.");
                return;
            }

            _isPolling = true;

            Task.Run(async () =>
            {
                Debug.WriteLine($"[POLLING] Background loop started for: {gameId}");

                while (_isPolling)
                {
                    try
                    {
                        // 2. FETCH PARENT OBJECT (More robust than fetching leaf string)
                        // This downloads "LastMove", "RedPlayer", etc. at once.
                        var gameState = await _firebase
                            .Child("games")
                            .Child(gameId)
                            .OnceSingleAsync<GameState>()
                            .ConfigureAwait(false);

                        if (gameState != null && !string.IsNullOrEmpty(gameState.LastMove))
                        {
                            Debug.WriteLine($"[POLLING DATA] LastMove: {gameState.LastMove}");

                            // 3. Process the move (Requires Dispatcher for UI)
                            HandleIncomingMove(gameState.LastMove);
                        }
                        else
                        {
                            Debug.WriteLine("[POLLING] Game state or LastMove is empty.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Print the FULL error to see if it's 404, Auth, or Format
                        Debug.WriteLine($"[POLLING EXCEPTION] {ex.GetType().Name}: {ex.Message}");
                    }

                    await Task.Delay(2000);
                }
            });
        }

        private void StartListeningToGame(string gameId)
        {
            Debug.WriteLine($"[OnlineService] Bắt đầu lắng nghe Game: {gameId}");

            // ✅ CLEAN UP listeners cũ nếu có
            _moveListener?.Dispose();
            _chatListener?.Dispose();

            // 1. LISTEN TO MOVES
            _moveListener = _firebase.Child("games").Child(gameId).Child("LastMove")
                .AsObservable<string>()
                //.DistinctUntilChanged()
                .Subscribe(
                    onNext: MoveListener,
                    onError: ex => Debug.WriteLine($"[FIREBASE ERROR] {ex.Message}"));

            // 2. LISTEN TO CHAT
            _chatListener = _firebase.Child("games").Child(gameId).Child("LastChat")
                .AsObservable<string>()
                .DistinctUntilChanged()
                .Subscribe(d =>
                {
                    try
                    {
                        string chatData = d.Object;
                        Debug.WriteLine($"[CHAT RAW] Nhận: {chatData}");

                        if (string.IsNullOrEmpty(chatData))
                        {
                            Debug.WriteLine("[CHAT] chatData is null/empty");
                            return;
                        }

                        if (chatData == "INIT")
                        {
                            Debug.WriteLine("[CHAT] chatData is INIT");
                            return;
                        }

                        if (chatData == _lastProcessedChat)
                        {
                            Debug.WriteLine($"[CHAT] Đã xử lý: {chatData}");
                            return;
                        }

                        var parts = chatData.Split(new[] { '|' }, 2);
                        if (parts.Length < 2)
                        {
                            Debug.WriteLine($"[CHAT ERROR] Format sai: {chatData}");
                            return;
                        }

                        string senderSide = parts[0];
                        string msg = parts[1];

                        if (senderSide == MySide)
                        {
                            Debug.WriteLine($"[CHAT] Bỏ qua chat của chính mình");
                            return;
                        }

                        _lastProcessedChat = chatData;
                        Debug.WriteLine($"[NHẬN CHAT] {senderSide}: {msg}");
                        OnChatReceived?.Invoke($"{senderSide}:{msg}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[CHAT ERROR] {ex.Message}");
                    }
                });

            Debug.WriteLine($"[OnlineService] Listeners đã được cài đặt cho game {gameId}");
        }

        public async Task SendMove(string moveStr)
        {
            if (!string.IsNullOrEmpty(CurrentGameId))
            {
                string payload = $"{MySide}|{moveStr}";
                Debug.WriteLine($"[GỬI MOVE] {payload}");

                try
                {
                    await _firebase
                        .Child("games")
                        .Child(CurrentGameId)
                        .PatchAsync(new { LastMove = payload });
                    Debug.WriteLine("[GỬI MOVE] OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GỬI MOVE ERROR] {ex.Message}");
                }
            }
        }

        public async Task SendChat(string msg)
        {
            if (!string.IsNullOrEmpty(CurrentGameId))
            {
                string payload = $"{MySide}|{msg}";
                Debug.WriteLine($"[GỬI CHAT] {payload}");

                try
                {
                    await _firebase.Child("games").Child(CurrentGameId).PatchAsync(new { LastChat = payload });
                    Debug.WriteLine("[GỬI CHAT] OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GỬI CHAT ERROR] {ex.Message}");
                }
            }
        }

        public void StopMatching()
        {
            Debug.WriteLine("[OnlineService] Dừng matching");
            _isPolling = false;
            _matchListener?.Dispose();
            _moveListener?.Dispose();
            _chatListener?.Dispose();
            DeleteMyMatchRequestSafe();
        }
    }

    public class MatchRequest { public string Name { get; set; } public string GameId { get; set; } }
}