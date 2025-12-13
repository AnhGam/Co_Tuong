using Chinese_Chess.Helpers;
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

        public GameView()
        {
            InitializeComponent();

            // KẾT NỐI VIEWMODEL 
            var viewModel = new GameViewModel();
            this.DataContext = viewModel;

            SetupTimer();
            AudioHelper.PlayBGM("Special.mp3");
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
                Window mainWindow = Window.GetWindow(this);
                if (mainWindow != null)
                {
                    mainWindow.Content = new MainMenuView();
                }
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
            MessageBox.Show("Cài đặt...");
        }

        private void Backward_Click(object sender, RoutedEventArgs e) { (this.DataContext as GameViewModel)?.Undo(); }
        private void Forward_Click(object sender, RoutedEventArgs e) { (this.DataContext as GameViewModel)?.Redo(); }
    }
}