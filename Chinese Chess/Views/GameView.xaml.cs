using Chinese_Chess.Helpers;
using Chinese_Chess.Models;
using Chinese_Chess.Models.Chinese_Chess.Models;
using Chinese_Chess.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Chinese_Chess.Views
{
    public partial class GameView : UserControl
    {
        DispatcherTimer gameTimer;
        int timeInSeconds = 0;
        bool isPaused = false;
        bool isMuted = false;
        bool _isGameFinished = false;
        public GameView()
        {
            InitializeComponent();

            // KẾT NỐI VIEWMODEL 
            var viewModel = new GameViewModel();
            this.DataContext = viewModel;
            viewModel.OnGameEnded += (winner) =>
            {
                _isGameFinished = true;
                ClearAutoSave();
                string message = (winner == "ĐỎ") ? "XUẤT SẮC! Bạn đã chiến thắng Bot!" : "HẾT CỜ! Bạn đã thua cuộc.";
                bool reviewGame = MessageBox.Show(
                    message + "\nBạn muốn làm gì tiếp theo?",
                    "KẾT THÚC",
                    "Xem lại ván đấu", 
                    "Về Menu Chính");  

                if (reviewGame){}
                else
                {
                    Window mainWindow = Window.GetWindow(this);
                    if (mainWindow != null) mainWindow.Content = new MainMenuView();
                }
            };
            viewModel.OnGameLoaded += (savedTime) =>
            {
                timeInSeconds = savedTime;
                TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);
                if (GameTimerLabel != null)
                    GameTimerLabel.Content = time.ToString(@"mm\:ss");
            };
            viewModel.ChatMessages.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    Dispatcher.InvokeAsync(() => ChatHistoryScroll.ScrollToBottom());
                }
            };
            AppSettings.OnSettingsChanged += OnAppSettingsChanged;
            UpdateGameAppearance();
            SetupTimer();

            AudioHelper.PlayBGM(AppSettings.CurrentMusicTrack);
            this.Loaded += GameView_Loaded;
            this.Unloaded += GameView_Unloaded;
        }

        // Khi bắt đầu game -> theo dõi nút thoát
        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            AppSettings.OnSettingsChanged += OnAppSettingsChanged;
            OnAppSettingsChanged();
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing += OnWindowClosing;
            }
        }

        // Khi thoát ra -> Ngừng theo dõi 
        private void GameView_Unloaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing -= OnWindowClosing;
            }
            if (this.DataContext is GameViewModel vm)
            {
                vm.StopGame();
            }
            AppSettings.OnSettingsChanged -= OnAppSettingsChanged;
        }

        private void OnAppSettingsChanged()
        {
            UpdateGameAppearance();

            if (this.DataContext is GameViewModel vm)
            {
                vm.RefreshPieceImages();
            }
        }
        private void UpdateGameAppearance()
        {
            try
            {
                var brush = new System.Windows.Media.ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(AppSettings.CurrentBoardBackground, UriKind.RelativeOrAbsolute));
                brush.Stretch = System.Windows.Media.Stretch.UniformToFill;
                this.Background = brush;
            }
            catch { }
        }
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isGameFinished && this.DataContext is GameViewModel vm)
            {
                vm.SaveGame(timeInSeconds, "autosave.json");
            }
        }


        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isPaused) return;
            var canvas = sender as Canvas;
            if (canvas == null) return;
            var point = e.GetPosition(canvas);
            var (col, row) = CoordinateHelper.GetExactCoordinate(point.X, point.Y);

            if (col != -1 && row != -1)
            {
                if (this.DataContext is GameViewModel vm)
                {
                    vm.OnTileClicked(col, row);
                }
            }
        }


        void SetupTimer()
        {
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += Timer_Tick;
            gameTimer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            // Đồng hồ thực
            if (RealTimeClock != null)
                RealTimeClock.Text = DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss");

            // Đồng hồ game
            if (!isPaused)
            {
                timeInSeconds++;
                TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);
                if (GameTimerLabel != null)
                    GameTimerLabel.Content = time.ToString(@"mm\:ss");
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            AudioHelper.PauseBGM(isPaused);
            if (isPaused)
            {
                gameTimer.Stop();
                try
                {
                    Pause_icon.Source = new BitmapImage(new Uri("/Assets/Button/play.png", UriKind.Relative));
                }
                catch { }
            }
            else
            {
                gameTimer.Start();
                try
                {
                    Pause_icon.Source = new BitmapImage(new Uri("/Assets/Button/pause1.png", UriKind.Relative));
                }
                catch { }
            }
        }

        private void New_Game_Click(object sender, RoutedEventArgs e)
        {
            timeInSeconds = 0;
            isPaused = false;
            if (GameTimerLabel != null) GameTimerLabel.Content = "00:00";

            // Gọi ViewModel để Reset bàn cờ
            (this.DataContext as GameViewModel)?.StartNewGame();
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            isMuted = !isMuted;
            AudioHelper.ToggleMute(isMuted);
            string iconName = isMuted ? "sound_off.png" : "sound_on.png";
            try
            {
                var uri = new Uri($"pack://application:,,,/Chinese Chess;component/Assets/Button/{iconName}");
                SoundIcon.Source = new BitmapImage(uri);
            }
            catch { }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {

            bool wasPaused = isPaused; 
            isPaused = true;
            gameTimer.Stop();
            // (éo biết bị gì những cần đưa vào title và message ngược nhau mới đúng)
            bool result = MessageBox.Show(
                "Mọi tiến độ trong trò chơi sẽ được lưu vào lần chơi kế tiếp.",
                "Thoát ra màn hình chính?",
                "Đồng ý",
                "Hủy");

            if (result) 
            {
                if (this.DataContext is GameViewModel vm)
                {
                    vm.SaveGame(timeInSeconds, "autosave.json");
                }
                QuitToMainMenu();
            }
            else 
            {
                if (!wasPaused)
                {
                    isPaused = false;
                    gameTimer.Start();
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = Window.GetWindow(this);
            SettingsView settingsScreen = new SettingsView();
            settingsScreen.OnCloseRequest += () =>
            {
                if (mainWindow != null)
                {
                    mainWindow.Content = this;
                    OnAppSettingsChanged();
                }
            };

            if (mainWindow != null)
            {
                mainWindow.Content = settingsScreen;
            }
        }
        private void QuitToMainMenu()
        {

            AudioHelper.PauseBGM(true); 

            Window mainWindow = Window.GetWindow(this);
            if (mainWindow != null) mainWindow.Content = new MainMenuView();
        }
        private void Backward_Click(object sender, RoutedEventArgs e) { (this.DataContext as GameViewModel)?.Undo(); }
        private void Forward_Click(object sender, RoutedEventArgs e) { (this.DataContext as GameViewModel)?.Redo(); }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }
        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void SendMessage()
        {
            string msg = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            if (this.DataContext is GameViewModel vm)
            {
                vm.AddToChat(msg, MessageType.Player, "Me");
            }

            ChatInputBox.Text = "";
            ChatInputBox.Focus();
        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            bool confirmSurrender = MessageBox.Show(
                "Bạn có chắc chắn muốn đầu hàng?",
                "Xác nhận",
                "Đúng vậy",
                "Đánh tiếp");

            if (!confirmSurrender) return;

            var vm = this.DataContext as GameViewModel;
            vm?.Surrender();
            _isGameFinished = true;
            ClearAutoSave();
            isPaused = true;
            gameTimer.Stop();

            bool reviewGame = MessageBox.Show(
                "Bạn đã đầu hàng! Bạn muốn làm gì?",
                "KẾT THÚC",
                "Xem lại ván đấu",
                "Về Menu Chính");

            if (reviewGame){}
            else
            {
                QuitToMainMenu();
            }
        }
        private void ClearAutoSave()
        {
            if (System.IO.File.Exists("autosave.json"))
            {
                System.IO.File.Delete("autosave.json");
            }
        }
    }
}