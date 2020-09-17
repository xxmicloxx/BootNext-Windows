using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BootNext
{
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class IconConverter : IValueConverter
    {
        public string? BootNextRoot { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bootRoot = BootNextRoot;
            if (bootRoot != null)
            {
                var bootPath = Path.Combine(bootRoot, $"icons\\{value}.png");
                if (File.Exists(bootPath))
                {
                    return new BitmapImage(new Uri(bootPath));
                }
            }
            
            var path = $"Resources\\Icons\\{value}.png";
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            return new BitmapImage(new Uri(Path.Combine(dir, path)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}