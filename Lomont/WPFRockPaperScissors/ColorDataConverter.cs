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
        // 
        static byte L(double alpha, int a, int b)
        {
            return (byte)(a * alpha + (1 - alpha) * b + 0.5);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var score = value as Score;
            if (score != null)
            {
                // score: wins - losses
                var total = score.wins + score.ties + score.losses;
                var ratio = (double)(score.wins)/(score.wins + score.losses+1);

                Color c;
                if (ratio > 0.50)
                {
                    var blend = (ratio - 0.5) * 2;
                    c = Color.FromRgb(L(blend,0,255), L(blend, 255, 255), L(blend, 0, 0));
                }
                else
                {
                    var blend = ratio * 2;
                    c = Color.FromRgb(L(blend, 255, 255), L(blend, 255, 0), L(blend, 0, 0));
                }
                return new SolidColorBrush(c);
                // 

                //if (score.wins > score.losses+gap)
                //    return new SolidColorBrush(Colors.LightGreen);
                //if (score.wins < score.losses-gap)
                //    return new SolidColorBrush(Colors.Pink);
                //return new SolidColorBrush(Colors.Yellow);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
