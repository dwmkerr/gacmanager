using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace GACManager
{
    public class WindowStateToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //  Get the value and parameter.
            var windowState = (WindowState)value;
            var borderLocation = (string)parameter;
            var isMaximised = windowState == WindowState.Maximized;

            //  Return the border thickness.
            switch (borderLocation)
            {
                case "Left":
                    return isMaximised ? 0.0 : 4.0;
                case "Top":
                    return isMaximised ? 0.0 : 4.0;
                case "TopLeft":
                    return isMaximised ? 0.0 : 4.0;
                case "TopRight":
                    return isMaximised ? 0.0 : 4.0;
                case "Right":
                    return isMaximised ? 0.0 : 4.0;
                case "BottomLeft":
                    return isMaximised ? 0.0 : 4.0;
                case "Bottom":
                    return isMaximised ? 0.0 : 4.0;
                case "BottomRight":
                    return isMaximised ? 0.0 : 4.0;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
