using Chinese_Chess.Helpers;
using Chinese_Chess.Models;
using Chinese_Chess.Services;
using Chinese_Chess.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Chinese_Chess.Views
{
    public partial class MainMenuView : UserControl
    {
        private OnlineService _onlineService;

        public MainMenuView()
        {
            InitializeComponent();
        }

        private async void PlayOnlineButton_Click(object sender, RoutedEventArgs e)
        {
            MatchFindingOverlay.Visibility = Visibility.Visible;

            _onlineService = new OnlineService();
            // Đăng ký sự kiện
            _onlineService.OnGameStarted += OnGameStarted;

            try
            {
                await _onlineService.FindMatch(AppSettings.PlayerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Matching error: {ex.Message}", "ERROR");
                MatchFindingOverlay.Visibility = Visibility.Collapsed;
                if (_onlineService != null)
                {
                    _onlineService.StopMatching();
                    _onlineService = null;
                }
            }
        }

        // [SỬA LỖI TẠI ĐÂY]
        private void OnGameStarted(string gameId)
        {
            // BẮT BUỘC dùng Dispatcher.Invoke để chuyển về luồng UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    MatchFindingOverlay.Visibility = Visibility.Collapsed;

                    var gameView = new GameView();
                    var gameViewModel = gameView.DataContext as GameViewModel;

                    if (gameViewModel != null)
                    {
                        // Truyền service sang ViewModel để dùng tiếp
                        gameViewModel.ContinueOnlineMatch(_onlineService);

                        Window mainWindow = Window.GetWindow(this);
                        if (mainWindow != null)
                        {
                            mainWindow.Content = gameView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Game start error: {ex.Message}", "ERROR");
                }
            });
        }

        private void CancelSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_onlineService != null)
            {
                _onlineService.StopMatching();
                _onlineService = null;
            }
            MatchFindingOverlay.Visibility = Visibility.Collapsed;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = Window.GetWindow(this);
            ChallengeView challengeScreen = new ChallengeView();
            if (mainWindow != null) mainWindow.Content = challengeScreen;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists("autosave.json")) return;

            GameView gameScreen = new GameView();
            if (gameScreen.DataContext is GameViewModel vm)
            {
                if (vm.LoadGame("autosave.json"))
                {
                    Window mainWindow = Window.GetWindow(this);
                    if (mainWindow != null) mainWindow.Content = gameScreen;
                }
                else
                {
                    MessageBox.Show("File save lỗi!", "LỖI");
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
                    AudioHelper.PauseBGM(true);
                }
            };
            if (mainWindow != null) mainWindow.Content = settingsScreen;
        }

        private void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}