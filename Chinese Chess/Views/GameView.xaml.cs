using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Chinese_Chess.Views
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        DispatcherTimer gameTimer;
        int timeInSeconds = 0;
        bool isPaused = false;
        bool isMuted = false;
        public GameView()
        {
            InitializeComponent();
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
            MessageBox.Show("Tính năng Cài đặt đang phát triển!");
        }


        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đi lùi (Undo) chưa làm.");
        }


        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đi tiếp (Redo) chưa làm.");
        }


        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
