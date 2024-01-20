using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Schema;

namespace Client
{
    public class MessageColor
    {
        public static SolidColorBrush Red => new SolidColorBrush(Color.FromRgb(186, 52, 52));
        public static SolidColorBrush Green => new SolidColorBrush(Color.FromRgb(104, 163, 62));
    }
}
