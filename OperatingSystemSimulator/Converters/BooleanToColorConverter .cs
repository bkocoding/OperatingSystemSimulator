using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using OperatingSystemSimulator.MemoryHelper;
using Windows.UI;

namespace OperatingSystemSimulator.Converters;

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PageBlock pageBlock)
        {
            if (pageBlock.IsSelected)
            {
                return new SolidColorBrush(Color.FromArgb(255, 173, 216, 230));
            }
            else if (pageBlock.IsAllocated)
            {
                return new SolidColorBrush(Color.FromArgb(255, 0, 155, 0));
            }
            else
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        //if (value is bool IsAllocated) 
        //{
        //        return IsAllocated ? new SolidColorBrush(Color.FromArgb(255, 173, 216, 230)) : new SolidColorBrush(Colors.Gray);
        //}
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
