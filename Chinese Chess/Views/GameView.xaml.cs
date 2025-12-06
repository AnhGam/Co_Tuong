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
        // Timer đếm giờ
        DispatcherTimer gameTimer;
        int timeInSeconds = 0;
        bool isPaused = false;
        bool isMuted = false;

        public GameView()
        {
            InitializeComponent();

            // 1. KẾT NỐI VIEWMODEL (QUAN TRỌNG)
            // Code này sẽ kích hoạt logic khởi tạo bàn cờ trong GameViewModel
            var viewModel = new GameViewModel();
            this.DataContext = viewModel;

            // 2. KHỞI ĐỘNG ĐỒNG HỒ
            SetupTimer();
        }

        // --- XỬ LÝ CLICK BÀN CỜ (Đã sửa lỗi không tìm thấy BoardCanvas) ---
        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // SỬA LỖI: Lấy Canvas từ 'sender' thay vì gọi tên trực tiếp
            var canvas = sender as Canvas;
            if (canvas == null) return;

            // 1. Lấy vị trí chuột trên Canvas
            var point = e.GetPosition(canvas);

            // 2. Tính toán tọa độ Cờ (Row/Col) dùng Helper
            // (Hàm này bạn đã có trong CoordinateHelper)
            var (col, row) = CoordinateHelper.GetExactCoordinate(point.X, point.Y);

            // 3. Nếu click hợp lệ (col != -1), gửi lệnh sang ViewModel
            if (col != -1 && row != -1)
            {
                if (this.DataContext is GameViewModel vm)
                {
                    vm.OnTileClicked(col, row);
                }
            }
        }

        // --- CÁC HÀM HỖ TRỢ (TIMER & BUTTON) ---

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
            string iconName = isMuted ? "sound_off.png" : "sound_on.png";
            try
            {
                var uri = new Uri($"pack://application:,,,/Chinese Chess;component/Assets/Button/{iconName}");
                SoundIcon.Source = new BitmapImage(uri);
            }
            catch { }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cài đặt...");
        }

        private void Backward_Click(object sender, RoutedEventArgs e) { }
        private void Forward_Click(object sender, RoutedEventArgs e) { }
    }
}