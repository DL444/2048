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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lib2048;

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

        Random randomizer = new Random(DateTime.Now.Millisecond);

        System.Timers.Timer timer = new System.Timers.Timer(1500);

        public MainWindow()
        {
            InitializeComponent();
            ParentGrid.Focus();
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\SavedGame.dat");
            timer.Elapsed += Timer_Elapsed;
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
                gameBoard = new GameBoard(brd);
                enabled = state.Enabled;
                ShowSaved(brd);
            }
            else
            {
                NewGame(4);
                timer.Start();
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

            gameBoard = new GameBoard(brd);
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
            brd.Move((Board.MoveDirection)randomizer.Next(0, 4));
        }

        private void UpBtn_Click(object sender, RoutedEventArgs e)
        {
            brd.Move(Board.MoveDirection.Up);
        }

        private void DownBtn_Click(object sender, RoutedEventArgs e)
        {
            brd.Move(Board.MoveDirection.Down);
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            brd.Move(Board.MoveDirection.Left);
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            brd.Move(Board.MoveDirection.Right);
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
            if(timer.Enabled == true) { return; }
            GameState state = new GameState(brd, enabled);
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2048\\SavedGame.dat");
            Stream saveStream;
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
            BinaryFormatter formatter = new BinaryFormatter();
            if(saveStream != null)
            {
                formatter.Serialize(saveStream, state);
                saveStream.Close();
            }
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
