using System;
using System.Windows;
using static LoginClient2048.LoginClient;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector : Window
    {
        ThemeSelectorModel model;
        bool _requesting = false;

        public string Sid { get; set; } = "";
        public string Username { get; set; } = "";
        public ThemeSelectorEntryModel Selected { get; set; }
        public string ThemeContent { get; set; }
        public bool Requesting
        {
            get => _requesting;
            set
            {
                _requesting = value;
                OkBtn.IsEnabled = !value;
            }
        }

        public ThemeSelector()
        {
            InitializeComponent();
        }

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!Requesting)
            {
                Requesting = true;
                Selected = model.SelectedEntry;
                try
                {
                    ThemeContent = await GetThemeContent(Selected.Id);
                }
                catch (ThemeNotFoundException)
                {
                    MessageBoxResult result = MessageBox.Show("Theme not found. Please contact technical support.", "Theme not found");
                    Requesting = false;
                    return;
                }
                catch (Exception)
                {
                    MessageBoxResult result = MessageBox.Show("Fetch theme list failed. Please check your Internet connection.", "Connection failed");
                    Requesting = false;
                    return;
                }
                Requesting = false;
                DialogResult = true;
                this.Close();
            }
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            string entries = "";
            while(true)
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
