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
using static LoginClient2048.LoginClient;

namespace Theme2048
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector : Window
    {
        ThemeSelectorModel model;

        public string Sid { get; set; } = "";
        public string Username { get; set; } = "";
        public ThemeSelectorEntryModel Selected { get; set; }
        public string ThemeContent { get; set; }

        public ThemeSelector()
        {
            InitializeComponent();
        }

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if(model.CreateNew == true)
            {
                Selected = new ThemeSelectorEntryModel(0, model.NewName, Username);
            }
            else
            {
                Selected = model.SelectedEntry;
                try
                {
                    ThemeContent = await GetThemeContent(Selected.Id);
                }
                catch(ThemeNotFoundException)
                {
                    MessageBoxResult result = MessageBox.Show("Theme not found. Please contact technical support.", "Theme not found");
                    return;
                }
                catch (Exception)
                {
                    MessageBoxResult result = MessageBox.Show("Fetch theme list failed. Please check your Internet connection.", "Connection failed");
                    return;
                }
            }
            DialogResult = true;
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string entries = "";
            while (true)
            {
                try
                {
                    entries = await GetThemes(Sid);
                    break;
                }
                catch (InvalidCredentialException)
                {
                    MessageBox.Show("Invalid credential. Please contact technical support.", "Invalid Credential");
                    this.Close();
                    return;
                }
                catch (Exception)
                {
                    MessageBoxResult result = MessageBox.Show("Fetch theme list failed. Please check your Internet connection and click OK to retry.", "Connection failed", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.Cancel)
                    {
                        this.Close();
                        return;
                    }
                }
            }

            model = ThemeSelectorModel.CreateModel(entries);
            this.DataContext = model;
        }
    }
}
