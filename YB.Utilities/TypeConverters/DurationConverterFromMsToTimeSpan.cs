using System.Globalization;

namespace YB.Utilities.TypeConverters;
public class DurationConverterFromMsToTimeSpan : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double milliseconds)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
            return time.ToString(@"mm\:ss");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
