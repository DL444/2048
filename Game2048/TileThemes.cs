using System;
using System.Linq;
using System.Windows.Media;
using ThemeSerializer2048;

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

    class UserTheme : ITileTheme
    {
        public FontFamily _font;

        public UserTheme(string xmlString)
        {
            XmlString = xmlString ?? throw new ArgumentNullException(nameof(xmlString));
            try
            {
                Theme = new Theme(XmlString);
            }
            catch(ArgumentException)
            {
                throw new ArgumentException(nameof(xmlString));
            }
            foreach (FontFamily f in Fonts.SystemFontFamilies)
            {
                if (f.Source == "Segoe UI")
                {
                    _font = f;
                    return;
                }
                _font = new FontFamily("Segoe UI");
            }
        }

        public FontFamily Font => _font;

        public string XmlString { get; private set; }
        public Theme Theme { get; private set; }

        public Brush GetBackgroundBrush(int level)
        {
            return new SolidColorBrush(GetColor(GetLevel(level), true));
        }
        public Brush GetForegroundBrush(int level)
        {
            return new SolidColorBrush(GetColor(GetLevel(level), false));
        }

        int GetLevel(int input)
        {
            if(input < 0) { return -1; }
            if(input == 0) { return 0; }
            int maxLevel = -1;
            foreach (ThemeEntry e in Theme.Entries)
            {
                if (e.Level > maxLevel)
                {
                    maxLevel = e.Level;
                }
            }

            if(Theme.Repeat)
            {
                return input % maxLevel == 0 ? maxLevel : input % maxLevel;
            }
            else
            {
                return input > maxLevel ? maxLevel : input;
            }
        }
        Color GetColor(int level, bool background)
        {
            string colorStr = "";
            if(background)
            {
                colorStr = Theme.Entries.First(x => x.Level == level).BackgroundColor;
            }
            else
            {
                colorStr = Theme.Entries.First(x => x.Level == level).ForegroundColor;
            }
            return (Color)ColorConverter.ConvertFromString(colorStr);
        }
    }
}
