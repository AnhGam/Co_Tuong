using Chinese_Chess.Helpers;
using Chinese_Chess.Models;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chinese_Chess.Views
{
    public partial class SettingsView : UserControl
    {
        private bool _isLoaded = false;
        private SettingsData _originalState;
        private bool _isSaved = false;
        public event Action OnCloseRequest;
        public SettingsView()
        {
            InitializeComponent();

            _originalState = AppSettings.CreateSnapshot();

            LoadCurrentSettings();
            _isLoaded = true;
        }

        private void LoadCurrentSettings()
        {
            MusicSlider.Value = AppSettings.MusicVolume;
            SFXSlider.Value = AppSettings.SFXVolume;
            NameInput.Text = AppSettings.PlayerName;

            try { AvatarPreview.ImageSource = new BitmapImage(new Uri(AppSettings.AvatarPath, UriKind.RelativeOrAbsolute)); } catch { }

            if (AppSettings.PieceStyleSuffix == "_text") RadioText.IsChecked = true;
            else RadioImage.IsChecked = true;

            if (AppSettings.AppTheme == "light") RadioLight.IsChecked = true;
            else RadioDark.IsChecked = true;

            foreach (ComboBoxItem item in MusicCombo.Items)
            {
                if (item.Tag.ToString() == AppSettings.CurrentMusicTrack)
                {
                    MusicCombo.SelectedItem = item;
                    break;
                }
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsSettingsChanged() && !_isSaved)
            {
                var msgBox = new MessageBox(
                    "Cảnh báo thoát",
                    "Bạn có thay đổi chưa lưu.\nBạn có muốn lưu lại không?",
                    "Lưu",
                    "Không lưu");
                var result = msgBox.ShowDialog();

                if (result == true)
                {
                    SaveAndExit();
                }
                else if (result == false)
                {
                    AppSettings.RestoreSnapshot(_originalState);
                    AudioHelper.PlayBGM(AppSettings.CurrentMusicTrack);

                    CloseThisView();
                }
            }
            else
            {
                CloseThisView();
            }
        }

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAndExit();
        }

        private void SaveAndExit()
        {
            AppSettings.PlayerName = NameInput.Text;
            AppSettings.SaveSettings();
            _isSaved = true;
            AppSettings.TriggerChange();
            CloseThisView();
        }


        private void CloseThisView()
        {
            if (OnCloseRequest != null)
            {
                OnCloseRequest.Invoke();
                return;
            }
            Window parent = Window.GetWindow(this);
            if (parent != null && parent.GetType().Name == "SettingsWindow")
            {
                parent.Close();
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                if (parent != null) parent.Effect = null;
            }
        }

        private bool IsSettingsChanged()
        {
            if (NameInput.Text != _originalState.PlayerName) return true;
            if (Math.Abs(MusicSlider.Value - _originalState.MusicVolume) > 0.01) return true;
            if (Math.Abs(SFXSlider.Value - _originalState.SFXVolume) > 0.01) return true;

            if (AppSettings.PieceStyleSuffix != _originalState.PieceStyleSuffix) return true;
            if (AppSettings.AppTheme != _originalState.AppTheme) return true;
            if (AppSettings.CurrentBoardBackground != _originalState.CurrentBoardBackground) return true;
            if (AppSettings.CurrentMusicTrack != _originalState.CurrentMusicTrack) return true;
            if (AppSettings.AvatarPath != _originalState.AvatarPath) return true;

            return false;
        }


        private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.MusicVolume = e.NewValue;
            AudioHelper.SetBGMVolume(e.NewValue);
        }

        private void SFXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.SFXVolume = e.NewValue;
        }

        private void ChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";
            if (dlg.ShowDialog() == true)
            {
                AppSettings.AvatarPath = dlg.FileName;
                AvatarPreview.ImageSource = new BitmapImage(new Uri(dlg.FileName));
                AppSettings.TriggerChange();
            }
        }

        private void PieceStyle_Checked(object sender, RoutedEventArgs e)
        {
            if (RadioText.IsChecked == true) AppSettings.PieceStyleSuffix = "_text";
            else AppSettings.PieceStyleSuffix = "_img";
            AppSettings.TriggerChange();
        }

        private void AppearanceMode_Checked(object sender, RoutedEventArgs e)
        {
            if (RadioLight.IsChecked == true) AppSettings.AppTheme = "light";
            else AppSettings.AppTheme = "dark";
            AppSettings.TriggerChange();
        }

        private void Background_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                AppSettings.CurrentBoardBackground = rb.Tag.ToString();
                AppSettings.TriggerChange();
            }
        }

        private void MusicCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            if (MusicCombo.SelectedItem is ComboBoxItem item)
            {
                string fileName = item.Tag.ToString();
                if (AppSettings.CurrentMusicTrack != fileName)
                {
                    AppSettings.CurrentMusicTrack = fileName;
                    AudioHelper.PlayBGM(fileName);
                }
            }
        }
    }
}