using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Game2048
{
    public interface ITileTheme
    {
        FontFamily Font { get; }
        Brush GetForegroundBrush(int level);
        Brush GetBackgroundBrush(int level);
    }

    class DefaultTheme : ITileTheme
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
        static FontFamily font;
        public FontFamily Font => font;

        static DefaultTheme()
        {
            foreach(FontFamily f in Fonts.SystemFontFamilies)
            {
                if(f.Source == "Segoe UI")
                {
                    font = f;
                    return;
                }
                font = new FontFamily("Segoe UI");
            }
        }

        public Brush GetBackgroundBrush(int level)
        {
            if (level < 0) { return Brushes.DarkGray; }
            level = level > 12 ? 12 : level;
            return backgrounds[level];
        }

        public virtual Brush GetForegroundBrush(int level)
        {
            if (level < 0) { return Brushes.Black; }
            return Brushes.White;
        }
    }
    class DefaultNoLabelTheme : DefaultTheme
    {
        public override Brush GetForegroundBrush(int level)
        {
            return Brushes.Transparent;
        }
    }
}
