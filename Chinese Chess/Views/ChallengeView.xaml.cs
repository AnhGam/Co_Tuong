using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Window mainWindow = Window.GetWindow(this);

            GameView gameScreen = new GameView();

            if (mainWindow != null)
            {
                mainWindow.Content = gameScreen;
            }
        }
    }
}
