using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Chinese_Chess.Helpers
{
    
    public class BoardCoordinateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int coord && parameter is string axis)
            {
                
                int cellSize = (axis == "X") ? CoordinateHelper.CellWidth : CoordinateHelper.CellHeight;
                int offset = (axis == "X") ? CoordinateHelper.OffsetX : CoordinateHelper.OffsetY;
                int pieceSize = 50; 

                
                return (double)(offset + (coord * cellSize) - (pieceSize / 2));
            }
            return 0.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}