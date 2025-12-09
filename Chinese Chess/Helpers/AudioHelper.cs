using System;
using System.IO;
using System.Windows.Media;

namespace Chinese_Chess.Helpers
{
    public static class AudioHelper
    {
        private static MediaPlayer _bgmPlayer = new MediaPlayer(); 
        private static bool _isMuted = false;

        //Phát nhạc nền (BGM)
        public static void PlayBGM(string fileName)
        {
            string path = GetSoundPath(fileName);
            if (File.Exists(path))
            {
                _bgmPlayer.Open(new Uri(path));
                _bgmPlayer.MediaEnded += (s, e) =>
                {
                    _bgmPlayer.Position = TimeSpan.Zero;
                    _bgmPlayer.Play(); 
                };
                _bgmPlayer.Volume = 0.1; 
                if (!_isMuted) _bgmPlayer.Play();
            }
        }

        //  Phát hiệu ứng (SFX) - Đi quân, ăn quân...
        public static void PlaySFX(string fileName)
        {
            if (_isMuted) return;

            string path = GetSoundPath(fileName);
            if (File.Exists(path))
            {
                MediaPlayer sfx = new MediaPlayer();
                sfx.Open(new Uri(path));
                sfx.Volume = 1.0; 
                sfx.Play();
            }
        }

        // Tạm dừng / Tiếp tục nhạc nền 
        public static void PauseBGM(bool isPause)
        {
            if (isPause) _bgmPlayer.Pause();
            else if (!_isMuted) _bgmPlayer.Play();
        }

        // Tắt/Bật tiếng toàn bộ 
        public static void ToggleMute(bool isMuted)
        {
            _isMuted = isMuted;
            if (_isMuted) _bgmPlayer.Pause();
            else _bgmPlayer.Play();
        }


        private static string GetSoundPath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", fileName);
        }
    }
}