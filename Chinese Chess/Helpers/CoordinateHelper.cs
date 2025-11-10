using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Helpers
{
    public static class CoordinateHelper
    {
        public const int CellSize = 70;

        public const int OffsetX = 30;
        public const int OffsetY = 30;

        public static (int x, int y) ToPixelCoordinate(int col, int row)
        {
            int x = OffsetX + col * CellSize;
            int y = OffsetY + row * CellSize;
            return (x, y);
        }

        public static (int x, int y) ToBoardCoordinate(int pixelX, int pixelY)
        {
            int col = (pixelX - OffsetX + CellSize / 2) / CellSize;
            int row = (pixelY - OffsetY + CellSize / 2) / CellSize;
            return (col, row);
        }
    }
}
