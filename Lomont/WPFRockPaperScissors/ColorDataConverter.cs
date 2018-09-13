using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFRockPaperScissors
{
    public class ColorDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var score = value as Score;
            if (score != null)
            {
                
                if (score.wins > score.losses)
                    return new SolidColorBrush(Colors.LightGreen);
                if (score.wins < score.losses)
                    return new SolidColorBrush(Colors.Pink);
                return new SolidColorBrush(Colors.Yellow);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
