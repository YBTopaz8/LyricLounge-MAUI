using System.Globalization;

namespace YB.Utilities.TypeConverters;

public class CurrentTimeConverterFromSecToTimeSpan : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            TimeSpan time = TimeSpan.FromSeconds(d);
            return time.ToString(@"mm\:ss");
        }
        return "00:01";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            return ts.TotalSeconds;
        }
        return 0.0;
    }
}