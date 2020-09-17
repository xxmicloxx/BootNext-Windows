using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BootNext
{
    [ValueConversion(typeof(bool), typeof(FontWeight))]
    public class BoolToFontWeightConverter : IValueConverter
    {
        public FontWeight TrueValue { get; set; }
        
        public FontWeight FalseValue { get; set; }

        public BoolToFontWeightConverter()
        {
            TrueValue = FontWeights.Bold;
            FalseValue = FontWeights.Normal;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}