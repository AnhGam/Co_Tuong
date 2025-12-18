using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public static string CurrentBoardBackground { get; set; } = "pack://application:,,,/Chinese Chess;component/Assets/Background/background.png";

        public static void TriggerChange()
        {
            OnSettingsChanged?.Invoke();
        }
    }
}