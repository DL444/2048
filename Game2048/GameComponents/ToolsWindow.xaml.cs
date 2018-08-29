using System;
using System.Windows;
using static LoginClient2048.LoginClient;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for Tools.xaml
    /// </summary>
    public partial class ToolsWindow : Window
    {
        bool requesting = false;
        public int Coins { get; private set; } = 0;
        public string Sid { get; set; } = "";
        public string Username { get; set; } = "";

        public ToolsWindow()
        {
            InitializeComponent();
        }

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (requesting) { return; }
            requesting = true;
            try
            {
                Coins = await InitiateTransaction(Username, Sid, (this.DataContext as ToolsWindowViewModel).Cost);
            }
            catch (InsufficientFundException)
            {
                MessageBoxResult result = MessageBox.Show("You do not have enough coins. Redeem some now?", "Insufficient fund", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    RedeemWindow redeemWindow = new RedeemWindow();
                    redeemWindow.Username = Username;
                    redeemWindow.Sid = Sid;
                    redeemWindow.ShowDialog();
                }
                return;
            }
            catch (InvalidCredentialException)
            {
                MessageBox.Show("Your login session has expired. Please sign in again.", "Session expired");
                return;
            }
            catch (UserNotFoundException)
            {
                MessageBox.Show("User not found. Please contact technical support.", "User not found");
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("Transaction failed. Please check your Internet connection.", "Connection failed");
                return;
            }
            finally
            {
                requesting = false;
            }
            DialogResult = true;
            this.Close();
        }
    }
}
