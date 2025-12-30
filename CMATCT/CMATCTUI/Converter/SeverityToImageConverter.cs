using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMATCTUI.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class SeverityToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string severity = value as string;

            if (severity == "Information")
            {
                return new BitmapImage(new Uri("/Images/Information.png", UriKind.Relative));
            }
            else if (severity == "Warning")
            {
                return new BitmapImage(new Uri("/Images/Warning.png", UriKind.Relative));
            }
            else if (severity == "Error")
            {
                return new BitmapImage(new Uri("/Images/Error.png", UriKind.Relative));
            }
            // Return null to show nothing
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
