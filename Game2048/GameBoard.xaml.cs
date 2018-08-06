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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lib2048;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for GameBoard.xaml
    /// </summary>
    public partial class GameBoard : UserControl
    {
        double cellSize;
        int size;
        Board refBoard;
        Tile[,] tiles;
        ITileTheme brushSet = new DefaultTheme();

        public GameBoard(Board refBoard)
        {
            InitializeComponent();
            this.refBoard = refBoard;
            size = refBoard.Size;
            cellSize = 1000 / size;
            for (int i = 0; i < size; i++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
                BoardGrid.RowDefinitions.Add(new RowDefinition());
            }
            this.refBoard.TilesMoved += RefBoard_TilesMovedEvent;
            this.refBoard.TileAdded += RefBoard_TileAdded;
            tiles = new Tile[size, size];
            DisplayBoard();
        }

        private void RefBoard_TileAdded(object sender, Board.TileAddedEventArgs e)
        {
            if(!e.MoveInitiated)
            {
                Dispatcher.Invoke(() =>
                    {
                        int i = e.Location.row;
                        int j = e.Location.column;
                        tiles[i, j] = new Tile(e.Value, brushSet);
                        Grid.SetRow(tiles[i, j], i);
                        Grid.SetColumn(tiles[i, j], j);
                        BoardGrid.Children.Add(tiles[i, j]);
                    });
            }
        }

        private void RefBoard_TilesMovedEvent(object sender, Board.TilesMovedEventArgs e)
        {
            MoveTiles(e.Moves);
        }

        void DisplayBoard()
        {
            BoardGrid.Children.Clear();
            for (int i = 0; i < refBoard.Size; i++)
            {
                for(int j = 0; j < refBoard.Size; j++)
                {
                    if(refBoard[i,j] != 0)
                    {
                        tiles[i, j] = new Tile(refBoard[i, j], brushSet);
                        Grid.SetRow(tiles[i, j], i);
                        Grid.SetColumn(tiles[i, j], j);
                        BoardGrid.Children.Add(tiles[i, j]);
                    }
                    else { tiles[i, j] = null; }
                }
            }
        }
        

        void MoveTiles(Board.MoveResult moves)
        {
            if(moves.Count == 0) { DisplayBoard(); return; }
            int ongoingAnimation = 0;
            foreach(Board.MoveResult.Move move in moves)
            {
                Tile tile = tiles[move.Original.row, move.Original.column];
                bool isX = false;
                double distance = 0;
                TranslateTransform translateTransform = new TranslateTransform();
                
                if(move.Original.row == move.Destination.row)
                {
                    distance = cellSize * (move.Destination.column - move.Original.column);
                }
                else
                {
                    distance = cellSize * (move.Destination.row - move.Original.row);
                    isX = true;
                }
                DoubleAnimation moveAnimation = new DoubleAnimation();
                moveAnimation.By = distance;
                moveAnimation.Duration = new Duration(new TimeSpan(2000000));
                moveAnimation.Completed += MoveAnimation_Completed;
                if(isX)
                {
                    try
                    {
                        tile.RenderTransform = new TranslateTransform(0, 0);
                        ongoingAnimation++;
                        tile.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
                    }
                    catch (NullReferenceException) { }
                }
                else
                {
                    try
                    {
                        tile.RenderTransform = new TranslateTransform(0, 0);
                        ongoingAnimation++;
                        tile.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
                    }
                    catch (NullReferenceException) { }
                }
            }

            void MoveAnimation_Completed(object sender, EventArgs e)
            {
                ongoingAnimation--;
                if(ongoingAnimation == 0)
                {
                    DisplayBoard();
                }
            }
        }

    }
}
