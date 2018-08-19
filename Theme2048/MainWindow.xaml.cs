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
using ThemeSerializer2048;

namespace Theme2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string sid = "";
        string username = "";
        ThemeModel model = null;
        long id = 0;

        public MainWindow()
        {
            InitializeComponent();

            LoginDialog loginDlg = new LoginDialog();
            bool? loginResult = loginDlg.ShowDialog();
            if (loginResult == true)
            {
                sid = loginDlg.Sid;
                username = loginDlg.Username;
            }
            else
            {
                Application.Current.Shutdown();
                return;
            }
            SelectTheme();
        }

        void SelectTheme()
        {
            ThemeSelectorEntryModel selected = null;
            ThemeSelector selector = new ThemeSelector();
            selector.Sid = sid;
            selector.Username = username;
            bool? selectResult = selector.ShowDialog();
            if (selectResult == true)
            {
                selected = selector.Selected;
            }
            else
            {
                Application.Current.Shutdown();
                return;
            }

            model = new ThemeModel();
            id = selected.Id;
            if (id == 0)
            {
                model.Name = selected.Name;
            }
            else
            {
                Theme t = new Theme(selector.ThemeContent);
                model.Name = t.Name;
                model.Repeat = t.Repeat;
                model.TileThemes.Clear();
                foreach (var e in t.Entries)
                {
                    model.TileThemes.Add(new TileThemeEntry(e.Level,
                        (Color)ColorConverter.ConvertFromString(e.BackgroundColor),
                        (Color)ColorConverter.ConvertFromString(e.ForegroundColor)));
                }
            }
            DataContext = model;
        }

        private async void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            Theme theme = new Theme();
            theme.Name = model.Name;
            theme.Repeat = model.Repeat;
            foreach(var entry in model.TileThemes)
            {
                try
                {
                    theme.Entries.Add(new ThemeEntry()
                    {
                        Level = entry.Level,
                        BackgroundColor = entry.BackgroundColor.ToString(),
                        ForegroundColor = entry.ForegroundColor.ToString()
                    });
                }
                catch(FormatException)
                {
                    MessageBox.Show("Please correct all errors before uploading.", "Error");
                    return;
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(@"{""Name"":");
            builder.Append($"\"{theme.Name}\",");
            builder.Append(@"""Id"":");
            builder.Append($"{id},");
            builder.Append(@"""Content"":");
            builder.Append($"\"{theme.GetXmlString(true)}\",");
            builder.Append(@"""Uploader"":");
            builder.Append($"\"{username}\"}}");

            try
            {
                await LoginClient2048.LoginClient.EditTheme(builder.ToString(), sid);
            }
            catch(ArgumentException)
            {
                MessageBox.Show("Please correct all errors before uploading.", "Error");
                return;
            }
            catch(LoginClient2048.LoginClient.InvalidCredentialException)
            {
                MessageBox.Show("Invalid credential. Please contact technical support.", "Invalid Credential");
                return;
            }
            catch (LoginClient2048.LoginClient.ThemeNotFoundException)
            {
                MessageBox.Show("Theme ID not found. Please contact technical support.", "Theme Not Found");
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("Upload failed. Please check your Internet connection.", "Connection failed");
                return;
            }
            MessageBox.Show("Upload success.", "Success");
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("You will lose all changes that are not uploaded. Are you sure?", "Confirm", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                this.Hide();
                SelectTheme();
                this.Show();
            }
        }
    }
}
