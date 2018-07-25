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
        static Color[] backgrounds = new Color[]
        {
            Colors.Transparent, // 0
            Colors.Indigo, // 2
            Colors.DarkBlue, // 4
            Colors.RoyalBlue, // 8
            Colors.DeepSkyBlue, // 16
            Colors.Teal, // 32
            Colors.Green, // 64
            Colors.LimeGreen, // 128
            Colors.Gold, // 256
            Colors.Orange, // 512
            Colors.Tomato, // 1024
            Colors.Red, // 2048
            Colors.DarkRed // 4096
        };

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                if (value == -1)
                {
                    NumberBox.Content = "X";
                    NumberBox.Foreground = new SolidColorBrush(Colors.Black);
                    BackGrid.Background = new SolidColorBrush(Colors.DarkGray);
                }
                else
                {
                    int level = (int)Math.Log(value, 2);
                    level = level > 12 ? 12 : level;
                    number = value;
                    BackGrid.Background = new SolidColorBrush(backgrounds[level]);
                    NumberBox.Content = value;
                    NumberBox.Foreground = level == 0 ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
                }
            }
        }


        public Tile(int number)
        {
            InitializeComponent();
            Number = number;
        }

    }
}
