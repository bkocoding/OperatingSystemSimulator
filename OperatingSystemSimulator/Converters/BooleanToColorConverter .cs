using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Windows.UI;

namespace OperatingSystemSimulator.Converters;

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool IsAllocated)
        {
            return IsAllocated ? new SolidColorBrush(Color.FromArgb(255, 0, 155, 0)) : new SolidColorBrush(Colors.Gray);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
