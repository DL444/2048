using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        static DefaultTheme defaultBrushSet = new DefaultTheme();
        ITileTheme theme;

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                if (value == -1)
                {
                    NumberBox.Content = "X";
                    NumberBox.Foreground = theme.GetForegroundBrush(value);
                    BackGrid.Background = theme.GetBackgroundBrush(value);
                }
                else
                {
                    number = value;
                    NumberBox.Content = value;
                    int level = (int)Math.Log(value, 2);
                    BackGrid.Background = theme.GetBackgroundBrush(level);
                    //NumberBox.Foreground = level == 0 ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
                    NumberBox.Foreground = theme.GetForegroundBrush(level);
                }
                NumberBox.FontFamily = theme.Font;
            }
        }


        public Tile(int number) : this(number, defaultBrushSet) { }
        public Tile(int number, ITileTheme brushSet)
        {
            InitializeComponent();
            this.theme = brushSet;
            Number = number;
        }

        public event EventHandler<TileTappedEventArgs> TileTapped;
        public class TileTappedEventArgs : EventArgs
        {
            Tile TileTapped { get; }
            public TileTappedEventArgs(Tile tileTapped)
            {
                TileTapped = tileTapped;
            }
        }

        private void Tile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TileTapped?.Invoke(this, new TileTappedEventArgs(this));
        }
    }
}
