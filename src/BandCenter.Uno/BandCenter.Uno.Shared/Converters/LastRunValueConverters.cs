using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace BandCenter.Uno.Converters
{
    internal class LastRunEndTimeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string returnValue = "Run";
            if (value is DateTime endTime)
            {
                returnValue = $"{endTime:ddd M/dd}";
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    internal class LastRunDistanceValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string returnValue = "loading...";
            if (value is uint distance)
            {
                returnValue = $"{distance / (1609.344 * 100):##0.0}<s>mi</s>";
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
