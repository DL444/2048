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
using System.Windows.Shapes;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for HighscoreWindow.xaml
    /// </summary>
    public partial class HighscoreWindow : Window
    {
        HighscoreViewModel vm = new HighscoreViewModel();
        public HighscoreWindow()
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Size = int.Parse((SizeCombo.SelectedItem as ComboBoxItem).Tag as string);
            vm.RefreshListEx.Execute(null);
        }

        private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Mode = int.Parse((ModeCombo.SelectedItem as ComboBoxItem).Tag as string);
            vm.RefreshListEx.Execute(null);
            vm.RefreshListEx.Execute(null);
        }
    }
}
