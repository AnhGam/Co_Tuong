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
            int selectedDifficulty = 1; // Mặc định là Easy

            if (OptionEasy.IsChecked == true) selectedDifficulty = 1;
            else if (OptionMedium.IsChecked == true) selectedDifficulty = 2;
            else if (OptionHard.IsChecked == true) selectedDifficulty = 3;

            Window mainWindow = Window.GetWindow(this);
            GameView gameScreen = new GameView();


            if (gameScreen.DataContext is GameViewModel vm)
            {
                vm.Difficulty = selectedDifficulty;
            }

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