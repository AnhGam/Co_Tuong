using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Models
{
    public class SettingsData
    {
        public double MusicVolume { get; set; }
        public double SFXVolume { get; set; }
        public string PlayerName { get; set; }
        public string AvatarPath { get; set; }
        public string PieceStyleSuffix { get; set; }
        public string CurrentMusicTrack { get; set; }
        public string CurrentBoardBackground { get; set; }
    }
}