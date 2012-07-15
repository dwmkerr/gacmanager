using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace GACManager
{
    public class WindowStateToButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //  Get the value and parameter.
            var windowState = (WindowState)value;
            var button = (string)parameter;

            //  Return the border thickness.
            switch (button)
            {
                case "Minimize":
                    return windowState == WindowState.Minimized ? Visibility.Collapsed : Visibility.Visible;
                case "Maximize":
                    return windowState == WindowState.Maximized ? Visibility.Collapsed : Visibility.Visible;
                case "Restore":
                    return windowState == WindowState.Normal ? Visibility.Collapsed : Visibility.Visible;
                default:
                    return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
