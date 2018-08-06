using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Game2048
{
    public interface IBrushSet
    {
        Brush GetForegroundBrush(int level);
        Brush GetBackgroundBrush(int level);
    }

    class DefaultBrushSet : IBrushSet
    {
        static SolidColorBrush[] backgrounds = new SolidColorBrush[]
        {
            Brushes.Transparent, // 0
            Brushes.Indigo, // 2
            Brushes.DarkBlue, // 4
            Brushes.RoyalBlue, // 8
            Brushes.DeepSkyBlue, // 16
            Brushes.Teal, // 32
            Brushes.Green, // 64
            Brushes.LimeGreen, // 128
            Brushes.Gold, // 256
            Brushes.Orange, // 512
            Brushes.Tomato, // 1024
            Brushes.Red, // 2048
            Brushes.DarkRed // 4096
        };
        static readonly SolidColorBrush blackBrush = Brushes.Black;
        static readonly SolidColorBrush whiteBrush = Brushes.White;
        static readonly SolidColorBrush grayBrush = Brushes.DarkGray;

        public Brush GetBackgroundBrush(int level)
        {
            if (level < 0) { return grayBrush; }
            level = level > 12 ? 12 : level;
            return backgrounds[level];
        }

        public Brush GetForegroundBrush(int level)
        {
            if (level < 0) { return blackBrush; }
            return whiteBrush;
        }
    }
}
