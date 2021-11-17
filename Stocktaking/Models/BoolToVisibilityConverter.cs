using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Stocktaking.Models;

internal class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = false;
        if (value is bool boolean)
        {
            flag = boolean;
        }
        else if (value is bool?)
        {
            bool? flag2 = (bool?)value;
            flag = (flag2.HasValue && flag2.Value);
        }

        return flag ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility visibility ? visibility == Visibility.Visible : (object)false;
    
}
