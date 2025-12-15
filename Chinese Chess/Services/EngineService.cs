using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Chinese_Chess.Services
{
    public class BotMove
    {
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
    }

    public class EngineService
    {
        private Process _engineProcess;
        private const string EngineFileName = "Assets/Engine/pikafish.exe"; 

        public void StartEngine()
        {
            try
            {
                if (!File.Exists(EngineFileName))
                {
                    MessageBox.Show($"Không tìm thấy file bot tại: {Path.GetFullPath(EngineFileName)}");
                    return;
                }

                _engineProcess = new Process();
                _engineProcess.StartInfo.FileName = EngineFileName;
                _engineProcess.StartInfo.UseShellExecute = false;
                _engineProcess.StartInfo.RedirectStandardInput = true;  // Để gửi lệnh vào
                _engineProcess.StartInfo.RedirectStandardOutput = true; // Để đọc kết quả ra
                _engineProcess.StartInfo.CreateNoWindow = true;         // Chạy ngầm, không hiện cửa sổ đen
                _engineProcess.Start();

                // Gửi lệnh khởi tạo UCI
                SendCommand("uci");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động Bot: " + ex.Message);
            }
        }

        public void StopEngine()
        {
            try
            {
                if (_engineProcess != null && !_engineProcess.HasExited)
                {
                    SendCommand("quit"); // Gửi lệnh thoát lịch sự
                    _engineProcess.Kill(); 
                    _engineProcess.Dispose();
                }
            }
            catch { }
        }


        public async Task<BotMove> GetBestMoveAsync(string fen, int depth)
        {
            if (_engineProcess == null || _engineProcess.HasExited) StartEngine();

            SendCommand($"position fen {fen}");
            SendCommand($"go depth {depth}");
            return await Task.Run(() =>
            {
                while (!_engineProcess.StandardOutput.EndOfStream)
                {
                    string line = _engineProcess.StandardOutput.ReadLine();
                    Debug.WriteLine($"[BOT SAY]: {line}");
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.StartsWith("bestmove"))
                    {
                        string moveStr = line.Split(' ')[1]; 
                        return ParseUciMove(moveStr); 
                    }
                }
                return null;
            });
        }
        public Action<string> OnLog { get; set; }
        private void SendCommand(string command)
        {
            if (_engineProcess != null && !_engineProcess.HasExited)
            {
                Debug.WriteLine($"[ME -> BOT]: {command}");
                OnLog?.Invoke($"ME: {command}");
                _engineProcess.StandardInput.WriteLine(command);
                _engineProcess.StandardInput.Flush();
            }
        }

        // Hàm dịch: "h2e2" -> (7, 2) đến (4, 2)
        // Chuẩn UCI: a=0, b=1, ... i=8 (Cột). 0=Dưới, 9=Trên (Hàng).
        private BotMove ParseUciMove(string uciMove)
        {
            // UCI: a0 là góc dưới trái (Đỏ). a9 là góc trên trái (Đen).
            // UI: (0,9) là góc dưới trái (Đỏ). (0,0) là góc trên trái (Đen).
            // => Cột X giữ nguyên. Hàng Y phải đảo ngược (9 - y).

            if (uciMove.Length < 4) return null;

            int fromX = uciMove[0] - 'a';
            int fromY = 9 - (uciMove[1] - '0'); // <--- ĐẢO NGƯỢC Y Ở ĐÂY (QUAN TRỌNG)

            int toX = uciMove[2] - 'a';
            int toY = 9 - (uciMove[3] - '0');   // <--- ĐẢO NGƯỢC Y Ở ĐÂY

            return new BotMove { FromX = fromX, FromY = fromY, ToX = toX, ToY = toY };
        }
    }
}