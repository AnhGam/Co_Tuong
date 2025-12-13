using Chinese_Chess.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chinese_Chess.Views
{
    /// <summary>
    /// Interaction logic for MainMenuView.xaml
    /// </summary>
    public partial class MainMenuView : UserControl
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = Window.GetWindow(this);

            ChallengeView challengeScreen = new ChallengeView();

            if (mainWindow != null)
            {
                mainWindow.Content = challengeScreen;
            }
        }
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists("autosave.json"))
            {
                return;
            }
            GameView gameScreen = new GameView();

            if (gameScreen.DataContext is GameViewModel vm)
            {
                bool isLoaded = vm.LoadGame("autosave.json");

                if (isLoaded)
                {
                    Window mainWindow = Window.GetWindow(this);
                    if (mainWindow != null)
                    {
                        mainWindow.Content = gameScreen;
                    }
                }
                else
                {
                    MessageBox.Show("File save bị lỗi, không thể tiếp tục!", "LỖI");
                }
            }
        }
        private void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
