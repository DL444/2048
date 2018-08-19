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

        bool isMoving = false;
        TouchPoint startPt = null;

        public ITileTheme BrushSet
        {
            get => brushSet;
            set
            {
                brushSet = value;
                DisplayBoard();
            }
        }

        public GameBoard(Board refBoard) : this(refBoard, new DefaultTheme()) { }
        public GameBoard(Board refBoard, ITileTheme brushSet)
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
            if (refBoard is ItemBoard b)
            {
                b.TilePromoted += ItemBoard_TilePromoted;
                b.TileRemoved += ItemBoard_TileRemoved;
            }
            tiles = new Tile[size, size];
            BrushSet = brushSet;
            DisplayBoard();
        }

        public (int row, int column) GetCoordinate(Tile tile)
        {
            for(int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if(ReferenceEquals(tiles[i,j], tile))
                    {
                        return (i, j);
                    }
                }
            }
            return (-1, -1);
        }

        private void ItemBoard_TilePromoted(object sender, Board.TilePromotedEventArgs e)
        {
            DisplayBoard();
        }
        private void ItemBoard_TileRemoved(object sender, Board.TileRemovedEventArgs e)
        {
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
                        tiles[i, j] = new Tile(e.Value, BrushSet);
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
                        tiles[i, j] = new Tile(refBoard[i, j], BrushSet);
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

        void AddTile()
        {

        }

        private void BoardGrid_TouchUp(object sender, TouchEventArgs e)
        {
            if(isMoving)
            {
                isMoving = false;
                TouchPoint endPt = e.GetTouchPoint(BoardGrid);
                double deltaX = startPt.Position.X - endPt.Position.X;
                double deltaY = startPt.Position.Y - endPt.Position.Y;
                if(Math.Abs(Math.Abs(deltaX) - Math.Abs(deltaY)) > 100)
                {
                    if(Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        if(deltaX > 0)
                        {
                            TouchMoveBoard?.Invoke(this, new TouchMoveBoardEventArgs(TouchMoveDirection.Left));
                        }
                        else
                        {
                            TouchMoveBoard?.Invoke(this, new TouchMoveBoardEventArgs(TouchMoveDirection.Right));
                        }
                    }
                    else
                    {
                        if (deltaY > 0)
                        {
                            TouchMoveBoard?.Invoke(this, new TouchMoveBoardEventArgs(TouchMoveDirection.Up));
                        }
                        else
                        {
                            TouchMoveBoard?.Invoke(this, new TouchMoveBoardEventArgs(TouchMoveDirection.Down));
                        }
                    }
                }
            }
        }

        private void BoardGrid_TouchDown(object sender, TouchEventArgs e)
        {
            isMoving = true;
            startPt = e.GetTouchPoint(BoardGrid);
        }

        public event EventHandler<TouchMoveBoardEventArgs> TouchMoveBoard;
        public class TouchMoveBoardEventArgs : EventArgs
        {
            public TouchMoveDirection Direction { get; private set; }

            public TouchMoveBoardEventArgs(TouchMoveDirection direction) => Direction = direction;
        }
        public enum TouchMoveDirection
        {
            Up, Down, Left, Right
        }
    }
}
