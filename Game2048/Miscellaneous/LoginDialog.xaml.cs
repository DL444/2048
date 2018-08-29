using System;
using System.Windows;
using System.Windows.Input;
using static LoginClient2048.LoginClient;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        bool requesting = false;

        public string Sid { get; private set; } = "";
        public string Username { get; private set; } = "";
        public int Coins { get; private set; } = 0;
        bool Requesting
        {
            get => requesting;
            set
            {
                requesting = value;
                UserBox.IsEnabled = !value;
                PwdBox.IsEnabled = !value;
                OkBtn.IsEnabled = !value;
                RegBtn.IsEnabled = !value;
            }
        }

        public LoginDialog()
        {
            InitializeComponent();
        }

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if(Requesting == true) { return; }
            Requesting = true;
            try
            {
                Sid = await Login(UserBox.Text, GetPasswordHash(PwdBox.Password));
                Coins = await GetCoins(UserBox.Text, Sid);
            }
            catch(InvalidCredentialException)
            {
                MessageBox.Show("Username and password do not match. Please try again.", "Password mismatch");
                return;
            }
            catch(UserNotFoundException)
            {
                MessageBox.Show("User not found. Please Sign up first.", "User not found");
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("Login failed. Please check your Internet connection.", "Connection failed");
                return;
            }
            finally
            {
                Requesting = false;
            }
            this.DialogResult = true;
            Username = UserBox.Text.Trim();
            this.Close();
        }

        private async void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Requesting == true) { return; }
            Requesting = true;
            try
            {
                Sid = await Register(UserBox.Text, GetPasswordHash(PwdBox.Password));
            }
            catch (DuplicateRegistrationException)
            {
                MessageBox.Show("Username already taken. Please try another one.", "Username unavailable");
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("Registration failed. Please check your Internet connection.", "Connection failed");
                return;
            }
            finally
            {
                Requesting = false;
            }
            this.DialogResult = true;
            Username = UserBox.Text;
            this.Close();
        }

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            if (Requesting) { return; }
            if(e.Key == Key.Enter)
            {
                OkBtn_Click(this, null);
            }
        }
    }
}
