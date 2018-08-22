using Lib2048;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameBoard gameBoard;
        Board brd;
        bool ctrlDown = false;
        bool enabled = true;

        ITileTheme brushSet = new DefaultTheme();

        GameSoundPlayer soundPlayer = new GameSoundPlayer();

        string sid = "";
        string username = "";
        int coins = 0;

        Random randomizer = new Random(DateTime.Now.Millisecond);

        System.Timers.Timer timer = new System.Timers.Timer(1500);

        public int Coins
        {
            get => coins;
            set
            {
                coins = value;
                CoinBox.Text = value.ToString();
            }
        }

        public ITileTheme BrushSet
        {
            get => brushSet;
            set
            {
                brushSet = value;
                if(gameBoard != null)
                {
                    gameBoard.BrushSet = brushSet;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoginDialog loginDlg = new LoginDialog();
            bool? loginResult = loginDlg.ShowDialog();
            if(loginResult == true)
            {
                sid = loginDlg.Sid;
                username = loginDlg.Username;
                Coins = loginDlg.Coins;
                GreetBox.Text = $"Hello, {username}!";
            }
            else
            {
                Application.Current.Shutdown();
                return;
            }


            ParentGrid.Focus();
            timer.Elapsed += Timer_Elapsed;
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\SavedGame.dat");
            if (File.Exists(path))
            {
                GameState state = null;
                Stream readStream = File.OpenRead(path);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    state = formatter.Deserialize(readStream) as GameState;
                }
                catch(System.Runtime.Serialization.SerializationException)
                {

                }
                if(state == null)
                {
                    NewGame(4);
                    timer.Start();
                    return;
                }
                readStream.Close();
                brd = state.Brd;
                if(gameBoard != null)
                {
                    gameBoard.TouchMoveBoard -= GameBoard_TouchMoveBoard;
                }
                gameBoard = new GameBoard(brd, BrushSet);
                gameBoard.TouchMoveBoard += GameBoard_TouchMoveBoard;
                enabled = state.Enabled;
                ShowSaved(brd);
            }
            else
            {
                NewGame(4);
                timer.Start();
            }
            path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\Settings.dat");
            if (File.Exists(path))
            {
                SoundState state = null;
                Stream readStream = File.OpenRead(path);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    state = formatter.Deserialize(readStream) as SoundState;
                }
                catch (System.Runtime.Serialization.SerializationException)
                {

                }
                if (state == null)
                {
                    soundPlayer.Config(SoundState.Default);
                }
                else
                {
                    soundPlayer.Config(state);
                }
                readStream.Close();
            }
            else
            {
                soundPlayer.Config(SoundState.Default);
            }

            path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\Theme.xml");
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string themeStr = reader.ReadToEnd();
                    SetTheme(themeStr);
                }
            }
        }

        private void GameBoard_TouchMoveBoard(object sender, GameBoard.TouchMoveBoardEventArgs e)
        {
            if (timer.Enabled) { return; }
            if(e.Direction == GameBoard.TouchMoveDirection.Down)
            {
                Board.MoveResult result = brd.Move(Board.MoveDirection.Down);
                if (result.Count > 0)
                {
                    soundPlayer.PlaySfx(1);
                }
                else
                {
                    soundPlayer.PlaySfx(2);
                }
            }
            else if (e.Direction == GameBoard.TouchMoveDirection.Up)
            {
                Board.MoveResult result = brd.Move(Board.MoveDirection.Up);
                if (result.Count > 0)
                {
                    soundPlayer.PlaySfx(1);
                }
                else
                {
                    soundPlayer.PlaySfx(2);
                }
            }
            else if (e.Direction == GameBoard.TouchMoveDirection.Left)
            {
                Board.MoveResult result = brd.Move(Board.MoveDirection.Left);
                if (result.Count > 0)
                {
                    soundPlayer.PlaySfx(1);
                }
                else
                {
                    soundPlayer.PlaySfx(2);
                }
            }
            else
            {
                Board.MoveResult result = brd.Move(Board.MoveDirection.Right);
                if (result.Count > 0)
                {
                    soundPlayer.PlaySfx(1);
                }
                else
                {
                    soundPlayer.PlaySfx(2);
                }
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => { DemoRun(); });
            }
            catch (TaskCanceledException) { }
        }

        void NewGame(int size)
        {
            NewGame(GameMode.Normal, size);
        }
        void NewGame(GameMode mode, int size)
        {
            if(brd != null)
            {
                brd.GameFailed -= Brd_GameFailedEvent;
                brd.ScoreChanged -= Brd_ScoreChangedEvent;
                //if(brd is TimedDeathBoard) { (brd as TimedDeathBoard).ActionPerformed -= Brd_ActionPerformed; }
            }
            if(mode == GameMode.Normal)
            {
                brd = new Board(size);
            }
            else if(mode == GameMode.TimedAddTile)
            {
                brd = new TimedAddTileBoard(size);
            }
            else if(mode == GameMode.TimedReduceScore)
            {
                brd = new TimedReduceScoreBoard(size);
            }
            else if(mode == GameMode.Timed10min)
            {
                brd = new TimedDeathBoard(600, size);
            }
            else if(mode == GameMode.Obstacle)
            {
                brd = new ObstacledBoard(size);
            }
            else if(mode == GameMode.Tools)
            {
                brd = new ItemBoard(size);
            }
            else
            {
                brd = new Board(size);
            }

            if (gameBoard != null)
            {
                gameBoard.TouchMoveBoard -= GameBoard_TouchMoveBoard;
            }

            gameBoard = new GameBoard(brd, BrushSet);
            gameBoard.TouchMoveBoard += GameBoard_TouchMoveBoard;
            MainGrid.Children.Clear();
            MainGrid.Children.Add(gameBoard);
            brd.ScoreChanged += Brd_ScoreChangedEvent;
            brd.GameFailed += Brd_GameFailedEvent;

            //if(brd is TimedDeathBoard)
            //{
            //    TimeLabel.Visibility = Visibility.Visible;
            //    //(brd as TimedDeathBoard).ActionPerformed += Brd_ActionPerformed;
            //}
            //else
            //{
            //    TimeLabel.Visibility = Visibility.Collapsed;
            //}
        }

        void ShowSaved(Board board)
        {
            if (brd != null)
            {
                brd.GameFailed -= Brd_GameFailedEvent;
                brd.ScoreChanged -= Brd_ScoreChangedEvent;
            }
            if (board != null)
            {
                MainGrid.Children.Clear();
                MainGrid.Children.Add(gameBoard);
                brd.ScoreChanged += Brd_ScoreChangedEvent;
                brd.GameFailed += Brd_GameFailedEvent;

                timer.Stop();
                EntryPanel.Visibility = Visibility.Collapsed;
                ControlPanel.Visibility = Visibility.Visible;
                ScoreLabel.Content = $"Score: {brd.Score}";
                ScoreLabel.Foreground = new SolidColorBrush(Colors.Black);
                EnableControl(true);
                if(board is ItemBoard)
                {
                    ToolBox.Visibility = Visibility.Visible;
                }
            }
        }

        //private void Brd_ActionPerformed(object sender, TimedBoard.ActionPerformedEventArgs e)
        //{
        //    TimeLabel.Content = $"Time: {600000 - e.TotalTime}";
        //}

        private void Brd_GameFailedEvent(object sender, Board.GameFailedEventArgs e)
        {
            Dispatcher.Invoke(() => 
            {
                ScoreLabel.Foreground = new SolidColorBrush(Colors.Red);
                if (e is TimedDeathBoard.TimedGameFailedEventArgs)
                {
                    if ((e as TimedDeathBoard.TimedGameFailedEventArgs).Reason == TimedDeathBoard.TimedGameFailedEventArgs.FailedReason.TimesUp)
                    {
                        EnableControl(false);
                    }
                }
            });
        }

        private void Brd_ScoreChangedEvent(object sender, Board.ScoreChangedEventArgs e)
        {
            Dispatcher.Invoke(() => { ScoreLabel.Content = $"Score: {e.Score}"; });
        }

        void DemoRun()
        {
            if(timer.Enabled == false) { return; }
            brd.Move((Board.MoveDirection)randomizer.Next(0, 4));
        }

        private void UpBtn_Click(object sender, RoutedEventArgs e)
        {
            Board.MoveResult result = brd.Move(Board.MoveDirection.Up);
            if(result.Count > 0)
            {
                soundPlayer.PlaySfx(1);
            }
            else
            {
                soundPlayer.PlaySfx(2);
            }
        }

        private void DownBtn_Click(object sender, RoutedEventArgs e)
        {
            Board.MoveResult result = brd.Move(Board.MoveDirection.Down);
            if (result.Count > 0)
            {
                soundPlayer.PlaySfx(1);
            }
            else
            {
                soundPlayer.PlaySfx(2);
            }
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            Board.MoveResult result = brd.Move(Board.MoveDirection.Left);
            if (result.Count > 0)
            {
                soundPlayer.PlaySfx(1);
            }
            else
            {
                soundPlayer.PlaySfx(2);
            }
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            Board.MoveResult result = brd.Move(Board.MoveDirection.Right);
            if (result.Count > 0)
            {
                soundPlayer.PlaySfx(1);
            }
            else
            {
                soundPlayer.PlaySfx(2);
            }
        }

        private void NewGameBtns_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            NewGame((GameMode)ModeBox.SelectedIndex, int.Parse((sender as Button).Tag as string));
            EntryPanel.Visibility = Visibility.Collapsed;
            ControlPanel.Visibility = Visibility.Visible;
            if(ModeBox.SelectedIndex == 5)
            {
                ToolBox.Visibility = Visibility.Visible;
            }
            else
            {
                ToolBox.Visibility = Visibility.Collapsed;
            }
            ScoreLabel.Content = $"Score: {brd.Score}";
            ScoreLabel.Foreground = new SolidColorBrush(Colors.Black);
            EnableControl(true);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            brd.Undo();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame(4);
            timer.Start();
            EntryPanel.Visibility = Visibility.Visible;
            ControlPanel.Visibility = Visibility.Collapsed;
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\SavedGame.dat");
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void MainGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if(!enabled) { return; }
            if(timer.Enabled == true) { return; }
            switch(e.Key)
            {
                case Key.Up:
                case Key.W:
                    UpBtn_Click(null, null);
                    break;
                case Key.Down:
                case Key.S:
                    DownBtn_Click(null, null);
                    break;
                case Key.Left:
                case Key.A:
                    LeftBtn_Click(null, null);
                    break;
                case Key.Right:
                case Key.D:
                    RightBtn_Click(null, null);
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    ctrlDown = true;
                    break;
                case Key.Z:
                    if(ctrlDown == true)
                    {
                        Undo_Click(null, null);
                    }
                    break;
            }
        }
        private void MainGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (timer.Enabled == true) { return; }
            switch (e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    ctrlDown = false;
                    break;
            }
        }

        void EnableControl(bool enable)
        {
            UpBtn.IsEnabled = enable;
            DownBtn.IsEnabled = enable;
            LeftBtn.IsEnabled = enable;
            RightBtn.IsEnabled = enable;
            Undo.IsEnabled = enable;
            enabled = enable;
        }

        enum GameMode
        {
            Normal = 0, TimedAddTile = 1, TimedReduceScore = 2, Timed10min = 3, Obstacle = 4, Tools = 5
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(BrushSet is UserTheme t)
            {
                string themePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\Theme.xml");
                try
                {
                    using (StreamWriter themeWriter = new StreamWriter(themePath))
                    {
                        themeWriter.Write(t.XmlString);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048");
                    Directory.CreateDirectory(dirPath);
                    using (StreamWriter themeWriter = new StreamWriter(themePath))
                    {
                        themeWriter.Write(t.XmlString);
                    }
                }
            }
            SoundState sndState = new SoundState(soundPlayer.BgmOn, soundPlayer.BgmVolume, soundPlayer.SfxOn, soundPlayer.SfxVolume);
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\Settings.dat");
            Stream saveStream;
            try
            {
                saveStream = File.Create(path);
            }
            catch (DirectoryNotFoundException)
            {
                string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048");
                Directory.CreateDirectory(dirPath);
                saveStream = File.Create(path);
            }
            BinaryFormatter formatter = new BinaryFormatter();
            if (saveStream != null)
            {
                formatter.Serialize(saveStream, sndState);
                saveStream.Close();
            }

            if (timer.Enabled == true || sid == "") { return; }
            GameState state = new GameState(brd, enabled);
            path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\SavedGame.dat");
            try
            {
                saveStream = File.Create(path);
            }
            catch(DirectoryNotFoundException)
            {
                string dirPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048");
                Directory.CreateDirectory(dirPath);
                saveStream = File.Create(path);
            }
            formatter = new BinaryFormatter();
            if(saveStream != null)
            {
                formatter.Serialize(saveStream, state);
                saveStream.Close();
            }
        }

        private async void BombBtn_Click(object sender, RoutedEventArgs e)
        {
            if(brd is ItemBoard b)
            {
                await ProcessTools(b, ToolsMode.Bomb);
            }
        }
        private async void WildcardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (brd is ItemBoard b)
            {
                await ProcessTools(b, ToolsMode.Wildcard);
            }
        }
        private async void PromoteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (brd is ItemBoard b)
            {
                await ProcessTools(b, ToolsMode.Promote);
            }
        }

        private async Task ProcessTools(ItemBoard b, ToolsMode mode)
        {
            ToolsWindowViewModel vm = new ToolsWindowViewModel(brd.Size, mode, b);
            ToolsWindow window = new ToolsWindow();
            window.DataContext = vm;
            window.Sid = sid;
            window.Username = username;
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                switch (mode)
                {
                    case ToolsMode.Bomb:
                        b.RemoveTile(vm.Row - 1, vm.Column - 1);
                        break;
                    case ToolsMode.Promote:
                        b.PromoteTile(vm.Row - 1, vm.Column - 1);
                        break;
                    case ToolsMode.Wildcard:
                        b.AddTile(vm.Value, vm.Row - 1, vm.Column - 1);
                        break;
                }
                Coins = window.Coins;
            }
            else
            {
                try
                {
                    Coins = await LoginClient2048.LoginClient.GetCoins(username, sid);
                }
                catch (Exception) { }
            }
        }


        private void RedeemCardBtn_Click(object sender, RoutedEventArgs e)
        {
            RedeemWindow redeemWin = new RedeemWindow();
            redeemWin.Sid = sid;
            redeemWin.Username = username;
            bool? result = redeemWin.ShowDialog();
            if(result == true)
            {
                Coins = redeemWin.Coins;
            }
        }

        private void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            ThemeSelector selector = new ThemeSelector();
            bool? result = selector.ShowDialog();
            if(result == true)
            {
                string content = selector.ThemeContent;
                SetTheme(content);
            }
        }

        private void SetTheme(string content)
        {
            UserTheme theme = new UserTheme(content);
            BrushSet = theme;
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings window = new Settings();
            window.DataContext = soundPlayer;
            window.ShowDialog();
        }
    }

    [Serializable]
    class GameState
    {
        public Board Brd { get; private set; }
        public bool Enabled { get; private set; }

        public GameState(Board brd, bool enabled)
        {
            Brd = brd.Copy(); Enabled = enabled;
        }
    }
}
