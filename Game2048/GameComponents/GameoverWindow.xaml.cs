using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for GameoverWindow.xaml
    /// </summary>
    public partial class GameoverWindow : Window
    {
        private int _score;

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                ScoreBox.Text = value.ToString();
            }
        }
        public byte[] ImageBytes { get; set; }

        public GameoverWindow()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShareBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ImageBytes == null) { return; }
            Microsoft.Win32.SaveFileDialog filePicker = new Microsoft.Win32.SaveFileDialog();
            filePicker.FileName = "2048.jpg";
            filePicker.DefaultExt = ".jpg";
            filePicker.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
            filePicker.ShowDialog();
            using (FileStream fileStr = File.Create(filePicker.FileName))
            {
                fileStr.Write(ImageBytes, 0, ImageBytes.Length - 1);
            }
        }
    }
}
