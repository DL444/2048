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

namespace Game2048.Help
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        int index = 0;

        public int Index
        {
            get => index;
            set
            {
                if(value < 0 || value >= HelpPageFactory.PageCount) { return; }
                index = value;
                ContentFrame.Navigate(HelpPageFactory.GetInstance(Index));
            }
        }

        public HelpWindow()
        {
            InitializeComponent();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(HelpPageFactory.GetInstance(Index));
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Index--;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if(Index + 1 == HelpPageFactory.PageCount) { this.Close(); return; }
            Index++;
        }
    }

    public static class HelpPageFactory
    {
        static Type[] pageTypes = new Type[] { typeof(HelpPage0), typeof(HelpPage1), typeof(HelpPage2), typeof(HelpPage25), typeof(HelpPage3), typeof(HelpPage4) };
        public static int PageCount => pageTypes.Length;
        public static Page GetInstance(int index)
        {
            if(index < 0 || index >= pageTypes.Length) { return null; }
            var ctor = pageTypes[index].GetConstructor(Type.EmptyTypes);
            return ctor.Invoke(null) as Page;
        }
    }
}
