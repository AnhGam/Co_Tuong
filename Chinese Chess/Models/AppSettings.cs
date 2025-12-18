using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chinese_Chess.Models
{
    public static class AppSettings
    {

        public static double MusicVolume { get; set; } = 0.5;

        public static double SFXVolume { get; set; } = 1.0;

        public static string PlayerName { get; set; } = "Player 1";

        public static string AvatarPath { get; set; } = "/Assets/ImagePiece/red_general.png";

        public static bool IsTextPieceStyle { get; set; } = true;

        public static string CurrentMusicTrack { get; set; } = "Special.mp3";

        public static string CurrentBoardBackground { get; set; } = "/Assets/Background/background.png";
    }
}