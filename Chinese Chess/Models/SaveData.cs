using Chinese_Chess.Models.Chinese_Chess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Models
{
    public class MoveRecord
    {
        public string PieceId { get; set; } 
        public string CapturedPieceId { get; set; }
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
    }

    public class PieceRecord
    {
        public string Id { get; set; }
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsAlive { get; set; }
        public string ImagePath { get; set; }
    }

    public class GameSaveData
    {
        public DateTime SaveDate { get; set; } = DateTime.Now;
        public int GameTimeSeconds { get; set; }
        public string CurrentTurn { get; set; }

        public int Difficulty { get; set; }

        // Bàn cờ & Quân
        public List<PieceRecord> AllPieces { get; set; } = new List<PieceRecord>();

        // Lịch sử
        public List<MoveRecord> MoveHistory { get; set; } = new List<MoveRecord>();
        public List<MoveRecord> RedoStack { get; set; } = new List<MoveRecord>();

        // Chat
        public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();
    }
}
