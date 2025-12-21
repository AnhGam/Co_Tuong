using Chinese_Chess.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Chinese_Chess.Views
{
    public partial class ChallengeView : UserControl
    {
        public ChallengeView()
        {
            InitializeComponent();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameViewModel viewModel = new GameViewModel();

            if (OptionEasy.IsChecked == true)
            {
                viewModel.Difficulty = 1;
            }
            else if (OptionMedium.IsChecked == true)
            {
                viewModel.Difficulty = 2;
            }
            else
            {
                viewModel.Difficulty = 3;
            }

            GameView gameScreen = new GameView();

            gameScreen.DataContext = viewModel;

            Window mainWindow = Window.GetWindow(this);
            if (mainWindow != null)
            {
                mainWindow.Content = gameScreen;
            }
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = Window.GetWindow(this);
            MainMenuView mainMenuScreen = new MainMenuView();
            if (mainWindow != null)
            {
                mainWindow.Content = mainMenuScreen;
            }
        }
    }
}