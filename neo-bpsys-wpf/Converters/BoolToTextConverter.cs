using System;
using System.Globalization;
using System.Windows.Data;

namespace neo_bpsys_wpf.Converters;

/// <summary>
/// 布尔值到文本的转换器
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "已显示" : "已隐藏";
        }
        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
