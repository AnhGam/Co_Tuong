using Chinese_Chess.Models;
using System.Text;
using System.Linq;

namespace Chinese_Chess.Helpers
{
    public static class FenHelper
    {
        // Hàm chuyển đổi bàn cờ hiện tại sang chuỗi FEN
        public static string GetFen(BoardState board)
        {
            StringBuilder fen = new StringBuilder();

            // 1. Duyệt qua 10 hàng (từ hàng 0 xuống 9 - theo chuẩn FEN)
            for (int y = 0; y <= 9; y++)
            {
                if (y > 0) fen.Append('/');
                int emptyCount = 0;
                for (int x = 0; x <= 8; x++)
                {
                    // Tìm quân cờ tại vị trí (x, y)
                    var piece = board.Pieces.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);

                    if (piece == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        // Nếu đang đếm ô trống mà gặp quân cờ -> ghi số ô trống vào
                        if (emptyCount > 0)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }

                        // Ghi ký tự đại diện cho quân cờ
                        char pieceChar = GetPieceChar(piece.Type, piece.Color);
                        fen.Append(pieceChar);
                    }
                }

                // Hết 1 hàng, nếu còn ô trống chưa ghi thì ghi nốt
                if (emptyCount > 0)
                {
                    fen.Append(emptyCount);
                }

            }

            // 2. Thêm lượt đi (w = Red/White đi, b = Black đi)
            // Trong FEN chuẩn cờ tướng: w là Đỏ, b là Đen
            string turn = (board.CurrentTurn == PieceColor.Red) ? " w" : " b";
            fen.Append(turn);

            // 3. Các chỉ số phụ (nước đi không ăn quân, tổng số nước) - Bot thường không quan trọng lắm cái này
            // Gán mặc định là "- - 0 1" cho đơn giản
            fen.Append(" - - 0 1");

            return fen.ToString();
        }

        private static char GetPieceChar(PieceType type, PieceColor color)
        {
            char c = ' ';
            switch (type)
            {
                case PieceType.General: c = 'k'; break; 
                case PieceType.Advisor: c = 'a'; break;
                case PieceType.Elephant: c = 'b'; break; 
                case PieceType.Horse: c = 'n'; break;   
                case PieceType.Rook: c = 'r'; break;
                case PieceType.Cannon: c = 'c'; break;
                case PieceType.Soldier: c = 'p'; break; 
            }

            // Quân Đỏ viết Hoa, Quân Đen viết thường
            return (color == PieceColor.Red) ? char.ToUpper(c) : c;
        }
    }
}