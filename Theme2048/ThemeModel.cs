using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MicroMvvm;
using System.Text.RegularExpressions;

namespace Theme2048
{
    public class ThemeModel : INotifyPropertyChanged
    {
        public bool _repeat = false;
        public string _name = "";

        public ObservableCollection<TileThemeEntry> TileThemes { get; set; } = new ObservableCollection<TileThemeEntry>();
        public bool Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Repeat"));
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public ThemeModel()
            : this
            (     "Default", 
                  new[]
                  {
                      new TileThemeEntry(-1, Colors.DarkGray, Colors.Black), 
                      new TileThemeEntry(1, Colors.Indigo, Colors.White),
                      new TileThemeEntry(2, Colors.DarkBlue, Colors.White),
                      new TileThemeEntry(3, Colors.RoyalBlue, Colors.White),
                      new TileThemeEntry(4, Colors.DeepSkyBlue, Colors.White),
                      new TileThemeEntry(5, Colors.Teal, Colors.White),
                      new TileThemeEntry(6, Colors.Green, Colors.White),
                      new TileThemeEntry(7, Colors.LimeGreen, Colors.White),
                      new TileThemeEntry(8, Colors.Gold, Colors.White),
                      new TileThemeEntry(9, Colors.Orange, Colors.White),
                      new TileThemeEntry(10, Colors.Tomato, Colors.White),
                      new TileThemeEntry(11, Colors.Red, Colors.White),
                      new TileThemeEntry(12, Colors.DarkRed, Colors.White),
                  }, 
                  false
            )
        { }
        public ThemeModel(string name, IEnumerable<TileThemeEntry> entries, bool repeat)
        {
            foreach(var e in entries)
            {
                TileThemes.Add(e);
            }
            Repeat = repeat;
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class TileThemeEntry : INotifyPropertyChanged
    {
        private int _level;
        private Color _backgroundColor;
        private Color _foregroundColor;

        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BackgroundColor"));
            }
        }
        public Color ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ForegroundColor"));
            }
        }

        public TileThemeEntry(int level, Color background, Color foreground)
        {
            Level = level;
            BackgroundColor = background;
            ForegroundColor = foreground;
        }
        public TileThemeEntry() : this(0, Colors.Transparent, Colors.Transparent) { }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color;
        }
    }
    public class LevelStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int level = (int)value;
            if(level == -1)
            {
                return "X";
            }
            else
            {
                return ((int)Math.Pow(2, level)).ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ThemeSelectorModel : INotifyPropertyChanged
    {
        private bool _createNew = true;
        private string _newName = "";
        private ThemeSelectorEntryModel _selectedEntry;

        public bool CreateNew
        {
            get => _createNew;
            set
            {
                _createNew = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CreateNew"));
            }
        }
        public string NewName
        {
            get => _newName;
            set
            {
                _newName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewName"));
            }
        }

        public ObservableCollection<ThemeSelectorEntryModel> Entries { get; set; } = new ObservableCollection<ThemeSelectorEntryModel>();
        public ThemeSelectorEntryModel SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedEntry"));
            }
        }

        public ICommand Ok { get => new RelayCommand(OkEx, CanOkEx); }
        bool CanOkEx()
        {
            if (CreateNew)
            {
                if (NewName != "" && !NewName.Contains("\"") && !NewName.Contains(",") && !NewName.Contains(":"))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if(SelectedEntry != null)
                {
                    return true;
                }
                return false;
            }
        }
        void OkEx() { }

        public static ThemeSelectorModel CreateModel(string themeList)
        {
            ThemeSelectorModel model = new ThemeSelectorModel();
            Regex r = new Regex("{.*?}");
            MatchCollection matches = r.Matches(themeList);
            foreach(Match m in matches)
            {
                long id = 0;
                string name = "";
                string uploader = "";
                string[] props = m.Value.Split(',');
                foreach(string p in props)
                {
                    string[] pair = p.Split(':');
                    if(pair[0].Contains("id"))
                    {
                        id = long.Parse(pair[1]);
                    }
                    else if(pair[0].Contains("name"))
                    {
                        name = pair[1].Replace("\"", "");
                    }
                    else if(pair[0].Contains("uploader"))
                    {
                        uploader = pair[1].Replace("\"", "");
                    }
                }
                ThemeSelectorEntryModel em = new ThemeSelectorEntryModel(id, name, uploader);
                model.Entries.Add(em);
            }
            return model;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class ThemeSelectorEntryModel : INotifyPropertyChanged
    {
        long _id = 0;
        string _name = "";
        string _uploader = "";

        public long Id
        {
            get => _id;
            set
            {
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Id"));
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public string Uploader
        {
            get => _uploader;
            set
            {
                _uploader = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Uploader"));
            }
        }

        public ThemeSelectorEntryModel() { }
        public ThemeSelectorEntryModel(long id, string name, string uploader)
        {
            Id = id;
            Name = name;
            Uploader = uploader;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
