using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        static DefaultBrushSet defaultBrushSet = new DefaultBrushSet();
        IBrushSet brushSet;

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                if (value == -1)
                {
                    NumberBox.Content = "X";
                    NumberBox.Foreground = brushSet.GetForegroundBrush(value);
                    BackGrid.Background = brushSet.GetBackgroundBrush(value);
                }
                else
                {
                    number = value;
                    NumberBox.Content = value;
                    int level = (int)Math.Log(value, 2);
                    BackGrid.Background = brushSet.GetBackgroundBrush(level);
                    //NumberBox.Foreground = level == 0 ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
                    NumberBox.Foreground = brushSet.GetForegroundBrush(level);
                }
            }
        }


        public Tile(int number) : this(number, defaultBrushSet) { }
        public Tile(int number, IBrushSet brushSet)
        {
            InitializeComponent();
            this.brushSet = brushSet;
            Number = number;
        }
    }
}
