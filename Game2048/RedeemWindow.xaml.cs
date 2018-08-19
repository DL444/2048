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

namespace Game2048
{
    /// <summary>
    /// Interaction logic for RedeemWindow.xaml
    /// </summary>
    public partial class RedeemWindow : Window
    {
        bool requesting = false;
        public string Sid { get; set; } = "";
        public string Username { get; set; } = "";
        public int Coins { get; set; } = 0;
        bool Requesting
        {
            get => requesting;
            set
            {
                requesting = value;
                KeyBox.IsEnabled = !value;
                OkBtn.IsEnabled = !value;
            }
        }

        public RedeemWindow()
        {
            InitializeComponent();
        }

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (requesting) { return; }
            Requesting = true;
            try
            {
                Coins = await RedeemCard(Username, Sid, KeyBox.Text);
            }
            catch(CardInvalidException)
            {
                MessageBox.Show("The key entered is not valid. Please check and try again.", "Key invalid");
                return;
            }
            catch(CardRedeemedException)
            {
                MessageBox.Show("This card is already redeemed. Please try a new one.", "Card redeemed");
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
                MessageBox.Show("Redeem failed. Please check your Internet connection.", "Connection failed");
                return;
            }
            finally
            {
                Requesting = false;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void KeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(!Requesting && e.Key == Key.Enter)
            {
                OkBtn_Click(this, null);
            }
        }
    }
}
