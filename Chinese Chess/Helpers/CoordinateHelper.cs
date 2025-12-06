using System;

namespace Chinese_Chess.Helpers
{
    public static class CoordinateHelper
    {

        public const int CellWidth = 60;
        public const int CellHeight = 58;
        public const int OffsetX = 60;
        public const int OffsetY = 60;

        public const int ClickRadius = 25;


        public static (int x, int y) ToPixelCoordinate(int col, int row)
        {

            int x = OffsetX + col * CellWidth;
            int y = OffsetY + row * CellHeight;
            return (x, y);
        }


        public static (int col, int row) GetExactCoordinate(double mouseX, double mouseY)
        {

            int col = (int)Math.Round((mouseX - OffsetX) / CellWidth);
            int row = (int)Math.Round((mouseY - OffsetY) / CellHeight);


            double centerX = OffsetX + col * CellWidth;
            double centerY = OffsetY + row * CellHeight;

 
            double distance = Math.Sqrt(Math.Pow(mouseX - centerX, 2) + Math.Pow(mouseY - centerY, 2));

            if (distance <= ClickRadius && col >= 0 && col <= 8 && row >= 0 && row <= 9)
            {
                return (col, row);
            }

            return (-1, -1);
        }
    }
}