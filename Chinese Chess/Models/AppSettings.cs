using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Chinese_Chess.Models
{
    public static class AppSettings
    {
        public static event Action OnSettingsChanged;

        public static double MusicVolume { get; set; } = 0.5;
        public static double SFXVolume { get; set; } = 1.0;
        public static string PlayerName { get; set; } = "Player 1";
        public static string AvatarPath { get; set; } = "pack://application:,,,/Chinese Chess;component/Assets/ImagePiece/red_general_img.png";
        public static string PieceStyleSuffix { get; set; } = "_text";
        public static string CurrentMusicTrack { get; set; } = "1.mp3";
        public static string CurrentBoardBackground { get; set; } = "pack://application:,,,/Chinese Chess;component/Assets/Background/Background.png";

        public static void TriggerChange() => OnSettingsChanged?.Invoke();


        private const string SettingsFile = "settings.json";

        public static void SaveSettings()
        {
            var data = new SettingsData
            {
                MusicVolume = MusicVolume,
                SFXVolume = SFXVolume,
                PlayerName = PlayerName,
                AvatarPath = AvatarPath,
                PieceStyleSuffix = PieceStyleSuffix,
                CurrentMusicTrack = CurrentMusicTrack,
                CurrentBoardBackground = CurrentBoardBackground
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }

        public static void LoadSettings()
        {
            if (!File.Exists(SettingsFile)) return;
            try
            {
                string json = File.ReadAllText(SettingsFile);
                var data = JsonSerializer.Deserialize<SettingsData>(json);

                MusicVolume = data.MusicVolume;
                SFXVolume = data.SFXVolume;
                PlayerName = data.PlayerName;
                AvatarPath = data.AvatarPath;
                PieceStyleSuffix = data.PieceStyleSuffix;
                CurrentMusicTrack = data.CurrentMusicTrack;
                CurrentBoardBackground = data.CurrentBoardBackground;
            }
            catch {  }
        }

        public static SettingsData CreateSnapshot()
        {
            return new SettingsData
            {
                MusicVolume = MusicVolume,
                SFXVolume = SFXVolume,
                PlayerName = PlayerName,
                AvatarPath = AvatarPath,
                PieceStyleSuffix = PieceStyleSuffix,
                CurrentMusicTrack = CurrentMusicTrack,
                CurrentBoardBackground = CurrentBoardBackground
            };
        }


        public static void RestoreSnapshot(SettingsData data)
        {
            MusicVolume = data.MusicVolume;
            SFXVolume = data.SFXVolume;
            PlayerName = data.PlayerName;
            AvatarPath = data.AvatarPath;
            PieceStyleSuffix = data.PieceStyleSuffix;
            CurrentMusicTrack = data.CurrentMusicTrack;
            CurrentBoardBackground = data.CurrentBoardBackground;
            TriggerChange(); 
        }
    }
}