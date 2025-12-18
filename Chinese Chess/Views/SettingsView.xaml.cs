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
        public SettingsView()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Load Volume
            MusicSlider.Value = AppSettings.MusicVolume;
            SFXSlider.Value = AppSettings.SFXVolume;

            // Load Profile
            NameInput.Text = AppSettings.PlayerName;
            try
            {
                AvatarPreview.ImageSource = new BitmapImage(new Uri(AppSettings.AvatarPath, UriKind.RelativeOrAbsolute));
            }
            catch { }

            // Load Piece Style
            if (AppSettings.PieceStyleSuffix == "_text") RadioText.IsChecked = true;
            else RadioImage.IsChecked = true;

            // Load Music (Chọn đúng bài đang phát)
            foreach (ComboBoxItem item in MusicCombo.Items)
            {
                if (item.Tag.ToString() == AppSettings.CurrentMusicTrack)
                {
                    MusicCombo.SelectedItem = item;
                    break;
                }
            }
        }

        // --- ÂM THANH ---
        private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.MusicVolume = e.NewValue;
            AudioHelper.SetBGMVolume(e.NewValue);
        }

        private void SFXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppSettings.SFXVolume = e.NewValue;
        }

        // --- PROFILE ---
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

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.PlayerName = NameInput.Text;
            MessageBox.Show("Đã lưu thông tin!", "Thông báo");
            AppSettings.TriggerChange();
        }

        // --- GAME SETTINGS ---
        private void PieceStyle_Checked(object sender, RoutedEventArgs e)
        {
            if (RadioText.IsChecked == true) AppSettings.PieceStyleSuffix = "_text";
            else AppSettings.PieceStyleSuffix = "_img";

            AppSettings.TriggerChange(); // Báo GameView cập nhật quân cờ
        }

        private void Background_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                AppSettings.CurrentBoardBackground = rb.Tag.ToString();
                AppSettings.TriggerChange(); // Báo GameView cập nhật nền
            }
        }

        private void MusicCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicCombo.SelectedItem is ComboBoxItem item)
            {
                string fileName = item.Tag.ToString();
                if (AppSettings.CurrentMusicTrack != fileName)
                {
                    AppSettings.CurrentMusicTrack = fileName;
                    AudioHelper.PlayBGM(fileName); // Phát bài mới
                }
            }
        }
    }
}