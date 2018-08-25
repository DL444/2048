#region Build Notification
#if RELEASE
#warning Change version before build!
#endif
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lib2048
{
    /// <summary>
    /// A standard 2048 board.
    /// </summary>
    [Serializable]
    public class Board : ISerializable
    {
        /// <summary>
        /// Storage for board.
        /// </summary>
        int[,] board;
        /// <summary>
        /// A list for free cells.
        /// </summary>
        /// <remarks>
        /// This is used to ensure new tile is generated in a unoccupied cell.
        /// </remarks>
        List<int> freeTiles;
        /// <summary>
        /// A list recording previous states of the board.
        /// </summary>
        /// <remarks>
        /// Used for undo feature.
        /// </remarks>
        List<(int[,] board, int score)> history = new List<(int[,], int)>();
        /// <summary>
        /// Internal pseudorandom number generator.
        /// </summary>
        Random randomizer = new Random((int)DateTime.Now.Ticks);
        /// <summary>
        /// Current score.
        /// </summary>
        int score;
        /// <summary>
        /// Initializes a new instance of <see cref="Board"/> with specified size.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        public Board(int size)
        {
            Size = size;
            board = new int[size, size];
            freeTiles = new List<int>(size * size);
            for (int i = 0; i < size * size; i++)
            {
                freeTiles.Add(i);
            }
            GenerateTile();
            GenerateTile();
        }
        /// <summary>
        /// Initializes a new instance of <see cref="Board"/> with 4 * 4 size.
        /// </summary>
        public Board() : this(4) { }
        /// <summary>
        /// Initializes a clone of specified <see cref="Board"/> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        public Board(Board board) : this(board.Size)
        {
            for(int i = 0; i < board.Size; i++)
            {
                for(int j = 0; j < board.Size; j++)
                {
                    this.board[i, j] = board[i, j];
                }
            }
            UpdateFreeCells();
            score = board.score;
            foreach((int[,] hBoard, int hScore) in board.history)
            {
                history.Add((hBoard, hScore));
            }
        }
        /// <summary>
        /// Initializes a <see cref="Board"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        protected Board(SerializationInfo info, StreamingContext context)
        {
            Size = (int)info.GetValue("size", typeof(int));
            board = (int[,])info.GetValue("board", typeof(int[,]));
            score = (int)info.GetValue("score", typeof(int));
            history = (List<(int[,], int)>)info.GetValue("history", typeof(List<(int[,], int)>));
            randomizer = new Random(DateTime.Now.Millisecond);
            freeTiles = new List<int>(Size * Size);
            UpdateFreeCells();
        }
        /// <summary>
        /// Occurs when the game failed.
        /// </summary>
        public event EventHandler<GameFailedEventArgs> GameFailed;
        /// <summary>
        /// Occurs when the score changed.
        /// </summary>
        public event EventHandler<ScoreChangedEventArgs> ScoreChanged;
        /// <summary>
        /// Occurs when a new tile is added.
        /// </summary>
        public event EventHandler<TileAddedEventArgs> TileAdded;
        /// <summary>
        /// Occurs when a tile is promoted by calling <see cref="PromoteTile(int, int)"/> method.
        /// </summary>
        protected event EventHandler<TilePromotedEventArgs> TilePromoted;
        /// <summary>
        /// Occurs when a tile is removed by calling <see cref="RemoveTile(int, int)"/> method.
        /// </summary>
        protected event EventHandler<TileRemovedEventArgs> TileRemoved;
        /// <summary>
        /// Occurs when tiles are moved.
        /// </summary>
        public event EventHandler<TilesMovedEventArgs> TilesMoved;
        /// <summary>
        /// Gets the score of the board.
        /// </summary>
        /// <value>
        /// Current score.
        /// </value>
        public int Score
        {
            get { return score; }
            set
            {
                if (score != value)
                {
                    ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(value));
                }
                score = value;
            }
        }
        /// <summary>
        /// Gets the size of the board.
        /// </summary>
        /// <value>
        /// The number of cells in one dimension.
        /// </value>
        public int Size { get; }
        /// <summary>
        /// Gets the content of specified cell.
        /// </summary>
        /// <param name="row">The row of the desired cell.</param>
        /// <param name="column">The column of the desired cell.</param>
        /// <returns>The number representation of the requested cell.</returns>
        public int this[int row, int column]
        {
            get => board[row, column];
        }
        /// <summary>
        /// Gets the content of specified cell.
        /// </summary>
        /// <param name="tile">The cell number.</param>
        /// <returns>The number representation of the requested cell.</returns>
        /// <remarks>The tile number is assigned starting from the cell on the upper-left corner, going right
        /// and wraping on new line, ending at the cell on the lower-right corner.</remarks>
        public int this[int tile]
        {
            get => board[tile / Size, tile % Size];
        }
        /// <summary>
        /// Add the current state to the history record of the board.
        /// </summary>
        void AddHistoryRecord()
        {
            int[,] tempBrd = board.Clone() as int[,];
            history.Add((tempBrd, Score));
        }
        /// <summary>
        /// Remove the last state in the history record.
        /// </summary>
        void RemoveHistoryRecord()
        {
            history.RemoveAt(history.Count - 1);
        }
        /// <summary>
        /// Generate a tile with specified value in a random empty cell.
        /// </summary>
        /// <param name="value">The desired value.</param>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the board is full.</returns>
        /// <remarks>
        /// If the board is full, the method will also check if the game has failed. See <see cref="AssertFailure"/>.
        /// </remarks>
        protected (int row, int column) AddTile(int value)
        {
            return AddTile(value, false);
        }
        /// <summary>
        /// Generate a tile with specified value in a random empty cell, and specify whether the move is initiated by a board movement.
        /// </summary>
        /// <param name="value">The desired value.</param>
        /// <param name="moveInit">Whether the move is initiated by a board movement.</param>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the board is full.</returns>
        /// <remarks>
        /// If the board is full, the method will also check if the game has failed. See <see cref="AssertFailure"/>.
        /// </remarks>
        (int row, int column) AddTile(int value, bool moveInit)
        {
            UpdateFreeCells();
            if (freeTiles.Count > 0)
            {
                int tile = freeTiles[randomizer.Next(0, freeTiles.Count)];
                return AddTile(value, tile / Size, tile % Size, moveInit);
            }
            else { return (-1, -1); }
        }
        /// <summary>
        /// Generate a tile with specified value in specified cell.
        /// </summary>
        /// <param name="value">The desired value.</param>
        /// <param name="row">The row of the new tile.</param>
        /// <param name="column">The column of the new tile.</param>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the specified cell is occupied.</returns>
        /// <remarks>
        /// If a tile was successfully added, the method will fire <see cref="TileAdded"/> event.
        /// </remarks>
        protected (int row, int column) AddTile(int value, int row, int column)
        {
            return AddTile(value, row, column, false);
        }
        /// <summary>
        /// Generate a tile with specified value in specified cell, and specify whether the move is initiated by a board movement.
        /// </summary>
        /// <param name="value">The desired value.</param>
        /// <param name="row">The row of the new tile.</param>
        /// <param name="column">The column of the new tile.</param>
        /// <param name="moveInit">Whether the move is initiated by a board movement.</param>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the specified cell is occupied.</returns>
        /// <remarks>
        /// If a tile was successfully added, the method will fire <see cref="TileAdded"/> event.
        /// </remarks>
        (int row, int column) AddTile(int value, int row, int column, bool moveInit)
        {
            if (board[row, column] != 0) { return (-1, -1); }
            board[row, column] = value;
            TileAdded?.Invoke(this, new TileAddedEventArgs(row, column, value, moveInit));
            UpdateFreeCells();
            if (freeTiles.Count == 0)
            {
                if (AssertFailure() == true)
                {
                    FailGame();
                }
            }
            return (row, column);
        }
        /// <summary>
        /// Determine if the game has failed.
        /// </summary>
        /// <returns><see langword="true"/> if the game has failed, or otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// The method also fires the <see cref="GameFailed"/> event if the game has failed.
        /// </remarks>
        bool AssertFailure()
        {
            Board tempBoard = this.Copy();
            tempBoard.Move(MoveDirection.Up);
            if (!tempBoard.StatesEqual()) { return false; }
            tempBoard.Move(MoveDirection.Down);
            if (!tempBoard.StatesEqual()) { return false; }
            tempBoard.Move(MoveDirection.Left);
            if (!tempBoard.StatesEqual()) { return false; }
            tempBoard.Move(MoveDirection.Right);
            if (!tempBoard.StatesEqual()) { return false; }
            return true;
        }
        /// <summary>
        /// Returns a clone of current object.
        /// </summary>
        /// <returns>A clone of current object.</returns>
        public virtual Board Copy()
        {
            return new Board(this);
        }
        /// <summary>
        /// Credit specified score to the game.
        /// </summary>
        /// <param name="score">The score to credit.</param>
        protected virtual void CreditScore(int score)
        {
            Score += score;
        }
        /// <summary>
        /// Fires the <see cref="GameFailed"/> event.
        /// </summary>
        protected void FailGame()
        {
            GameFailed?.Invoke(this, new GameFailedEventArgs(Score));
        }
        /// <summary>
        /// Fires the <see cref="GameFailed"/> event with specified <see cref="GameFailedEventArgs"/> argument.
        /// </summary>
        /// <param name="e">The argument for firing the event.</param>
        protected void FailGame(GameFailedEventArgs e)
        {
            GameFailed?.Invoke(this, e);
        }
        /// <summary>
        /// Generate a random tile in a random empty cell.
        /// </summary>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the board is full.</returns>
        /// <remarks>
        /// If the board is full, the method will also check if the game has failed. See <see cref="AssertFailure"/>.
        /// </remarks>
        protected (int row, int column) GenerateTile()
        {
            return GenerateTile(false);
        }
        /// <summary>
        /// Generate a random tile in a random empty cell, and specify whether the move is initiated by a board movement.
        /// </summary>
        /// <param name="moveInit">Whether the move is initiated by a board movement.</param>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the board is full.</returns>
        /// <remarks>
        /// If the board is full, the method will also check if the game has failed. See <see cref="AssertFailure"/>.
        /// </remarks>
        (int row, int column) GenerateTile(bool moveInit)
        {
            return AddTile(randomizer.Next(0, 3) == 2 ? 4 : 2, moveInit);
        }
        /// <summary>
        /// Update the free cell list.
        /// </summary>
        void UpdateFreeCells()
        {
            freeTiles.Clear();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0) { freeTiles.Add(i * Size + j); }
                }
            }
        }
        /// <summary>
        /// Populate a <see cref="SerializationInfo"/> object with data needed for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to populate.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("board", board, typeof(int[,]));
            info.AddValue("history", history, typeof(List<(int[,], int)>));
            info.AddValue("score", score, typeof(int));
            info.AddValue("size", Size, typeof(int));
        }
        /// <summary>
        /// Get the row and column of a tile specified by its number.
        /// </summary>
        /// <param name="tile">The cell number.</param>
        /// <returns>A tuple containing the row and column of the tile, or (-1, -1) if the cell does not exist in the current board.</returns>
        /// <remarks>The tile number is assigned starting from the cell on the upper-left corner, going right
        /// and wraping on new line, ending at the cell on the lower-right corner.</remarks>
        public (int row, int column) GetTileCoordinate(int tile)
        {
            if(tile > Size * Size - 1) { return (-1, -1); }
            return (tile / Size, tile % Size);
        }
        /// <summary>
        /// Move all tiles to specified direction.
        /// </summary>
        /// <param name="direction">A <see cref="MoveDirection"/> value representing the direction.</param>
        /// <returns>A <see cref="MoveResult"/> object detailing the move performed.</returns>
        /// <remarks>If any tile moved, the method will also fire the <see cref="TilesMoved"/> event.</remarks>
        public MoveResult Move(MoveDirection direction)
        {
            MoveResult result = new MoveResult();
            AddHistoryRecord();
            Concentrate();
            Combine();
            Concentrate();
            if (!StatesEqual())
            {
                GenerateTile(true);
                TilesMoved?.Invoke(this, new TilesMovedEventArgs(result));
            }
            else
            {
                RemoveHistoryRecord();
            }
            return result;
            void Concentrate()
            {
                switch (direction)
                {
                    case MoveDirection.Up:
                        for (int j = 0; j < Size; j++)
                        {
                            int blanks = 0;
                            for (int i = 0; i < Size; i++)
                            {
                                if (board[i, j] == 0)
                                {
                                    blanks++;
                                }
                                else
                                {
                                    int temp = board[i - blanks, j];
                                    board[i - blanks, j] = board[i, j];
                                    board[i, j] = temp;
                                    result.Add(new MoveResult.Move((i, j), (i - blanks, j)));
                                }
                            }
                        }
                        break;
                    case MoveDirection.Down:
                        for (int j = 0; j < Size; j++)
                        {
                            int blanks = 0;
                            for (int i = Size - 1; i >= 0; i--)
                            {
                                if (board[i, j] == 0)
                                {
                                    blanks++;
                                }
                                else
                                {
                                    int temp = board[i + blanks, j];
                                    board[i + blanks, j] = board[i, j];
                                    board[i, j] = temp;
                                    result.Add(new MoveResult.Move((i, j), (i + blanks, j)));
                                }
                            }
                        }
                        break;
                    case MoveDirection.Left:
                        for (int i = 0; i < Size; i++)
                        {
                            int blanks = 0;
                            for (int j = 0; j < Size; j++)
                            {
                                if (board[i, j] == 0)
                                {
                                    blanks++;
                                }
                                else
                                {
                                    int temp = board[i, j - blanks];
                                    board[i, j - blanks] = board[i, j];
                                    board[i, j] = temp;
                                    result.Add(new MoveResult.Move((i, j), (i, j - blanks)));
                                }
                            }
                        }
                        break;
                    case MoveDirection.Right:
                        for (int i = 0; i < Size; i++)
                        {
                            int blanks = 0;
                            for (int j = Size - 1; j >= 0; j--)
                            {
                                if (board[i, j] == 0)
                                {
                                    blanks++;
                                }
                                else
                                {
                                    int temp = board[i, j + blanks];
                                    board[i, j + blanks] = board[i, j];
                                    board[i, j] = temp;
                                    result.Add(new MoveResult.Move((i, j), (i, j + blanks)));
                                }
                            }
                        }
                        break;
                }
            }
            void Combine()
            {
                switch (direction)
                {
                    case MoveDirection.Up:
                        for (int j = 0; j < Size; j++)
                        {
                            for (int i = 0; i < Size - 1; i++)
                            {
                                if (board[i, j] <= 0) { continue; }
                                if (board[i, j] == board[i + 1, j])
                                {
                                    board[i, j] += board[i + 1, j];
                                    CreditScore(board[i, j]);
                                    board[i + 1, j] = 0;
                                    result.Add(new MoveResult.Move((i + 1, j), (i, j)));
                                    i++;
                                }
                            }
                        }
                        break;
                    case MoveDirection.Down:
                        for (int j = 0; j < Size; j++)
                        {
                            for (int i = Size - 1; i > 0; i--)
                            {
                                if (board[i, j] <= 0) { continue; }
                                if (board[i, j] == board[i - 1, j])
                                {
                                    board[i, j] += board[i - 1, j];
                                    CreditScore(board[i, j]);
                                    board[i - 1, j] = 0;
                                    result.Add(new MoveResult.Move((i - 1, j), (i, j)));
                                    i--;
                                }
                            }
                        }
                        break;
                    case MoveDirection.Left:
                        for (int i = 0; i < Size; i++)
                        {
                            for (int j = 0; j < Size - 1; j++)
                            {
                                if (board[i, j] <= 0) { continue; }
                                if (board[i, j] == board[i, j + 1])
                                {
                                    board[i, j] += board[i, j + 1];
                                    CreditScore(board[i, j]);
                                    board[i, j + 1] = 0;
                                    result.Add(new MoveResult.Move((i, j + 1), (i, j)));
                                    j++;
                                }
                            }
                        }
                        break;
                    case MoveDirection.Right:
                        for (int i = 0; i < Size; i++)
                        {
                            for (int j = Size - 1; j > 0; j--)
                            {
                                if (board[i, j] <= 0) { continue; }
                                if (board[i, j] == board[i, j - 1])
                                {
                                    board[i, j] += board[i, j - 1];
                                    CreditScore(board[i, j]);
                                    board[i, j - 1] = 0;
                                    result.Add(new MoveResult.Move((i, j - 1), (i, j)));
                                    j--;
                                }
                            }
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// Promote a specified tile by one level.
        /// </summary>
        /// <param name="row">The row of the tile to promote.</param>
        /// <param name="column">The column of the tile to promote.</param>
        /// <returns><see langword="true"/> if the specified tile is removed, 
        /// <see langword="false"/> if no tile exists at the specified location or the tile is too large to promote.</returns>
        protected bool PromoteTile(int row, int column)
        {
            int value = board[row, column];
            if(value > 0 && value < 1073741824)
            {
                AddHistoryRecord();
                board[row, column] *= 2;
                TilePromoted?.Invoke(this, new TilePromotedEventArgs(row, column, value));
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove a specified tile.
        /// </summary>
        /// <param name="row">The row of the tile to remove.</param>
        /// <param name="column">The column of the tile to remove.</param>
        /// <returns><see langword="true"/> if the tile is removed, <see langword="false"/> if no tile exists at the specified location.</returns>
        protected bool RemoveTile(int row, int column)
        {
            int value = board[row, column];
            if (value == 0) { return false; }
            else
            {
                AddHistoryRecord();
                board[row, column] = 0;
                TileRemoved?.Invoke(this, new TileRemovedEventArgs(row, column, value));
                return true;
            }
        }
        /// <summary>
        /// Determine if the current board state is equal to the previous.
        /// </summary>
        /// <returns><see langword="true"/> if all the tiles are same, <see langword="false"/> if otherwise.</returns>
        bool StatesEqual()
        {
            int[,] tempBrd = history[history.Count - 1].board;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] != tempBrd[i, j]) { return false; }
                }
            }
            return true;
        }
        /// <summary>
        /// Undo the previous move.
        /// </summary>
        /// <remarks>If any tile moved, the method will also fire the <see cref="TilesMoved"/> event.</remarks>
        public virtual void Undo()
        {
            if (history.Count == 0) { return; }
            board = history[history.Count - 1].board;
            UpdateFreeCells();
            Score = history[history.Count - 1].score;
            RemoveHistoryRecord();
            TilesMoved?.Invoke(this, new TilesMovedEventArgs(new MoveResult()));
        }
        /// <summary>
        /// Specified the direction of movement.
        /// </summary>
        public enum MoveDirection
        {
            /// <summary>
            /// Specify the board to move up.
            /// </summary>
            Up = 0,
            /// <summary>
            /// Specify the board to move down.
            /// </summary>
            Down = 1,
            /// <summary>
            /// Specify the board to move left.
            /// </summary>
            Left = 2,
            /// <summary>
            /// Specify the board to move right.
            /// </summary>
            Right = 3
        }
        /// <summary>
        /// Provides information to the <see cref="GameFailed"/> event.
        /// </summary>
        public class GameFailedEventArgs : EventArgs
        {
            /// <summary>
            /// The score of the board.
            /// </summary>
            public int Score { get; }
            /// <summary>
            /// Initializes an instance of <see cref="GameFailedEventArgs"/> with specified score.
            /// </summary>
            /// <param name="score">The socre of the board.</param>
            public GameFailedEventArgs(int score)
            {
                Score = score;
            }
        }
        /// <summary>
        /// Details the movements of tiles in a move operation.
        /// </summary>
        public class MoveResult : IEnumerable<MoveResult.Move>
        {
            /// <summary>
            /// Containing information of a move.
            /// </summary>
            public class Move
            {
                /// <summary>
                /// Gets the original location of tile.
                /// </summary>
                /// <value>
                /// A tuple containing the row and column of the original location.
                /// </value>
                public (int row, int column) Original { get; internal set; }
                /// <summary>
                /// Gets the destination of tile.
                /// </summary>
                /// <value>
                /// A tuple containing the row and column of the destination.
                /// </value>
                public (int row, int column) Destination { get; internal set; }
                /// <summary>
                /// Initialize a <see cref="Move"/> object with specified original location and destination.
                /// </summary>
                /// <param name="original">The original location.</param>
                /// <param name="destination">The destination.</param>
                internal Move((int row, int column) original, (int row, int column) destination)
                {
                    Original = original;
                    Destination = destination;
                }
                /// <summary>
                /// Determines if a specified move connects the current move at the end.
                /// </summary>
                /// <param name="inMove">A <see cref="Move"/> object specifying the input move.</param>
                /// <returns><see langword="true"/> if connected at the end, <see langword="false"/> otherwise.</returns>
                internal bool IsConnecting(Move inMove)
                {
                    if (inMove.Original.row == this.Destination.row && inMove.Original.column == this.Destination.column)
                    {
                        return true;
                    }
                    return false;
                }
                /// <summary>
                /// Joins a specified move to the current one if they are connecting.
                /// </summary>
                /// <param name="inMove">A <see cref="Move"/> object specifying the input move.</param>
                /// <returns><see langword="true"/> if connected, <see langword="false"/> otherwise.</returns>
                internal bool JoinConnecting(Move inMove)
                {
                    if(IsConnecting(inMove))
                    {
                        Destination = inMove.Destination;
                        return true;
                    }
                    return false;
                }
            }
            /// <summary>
            /// Stores all moves in the current movement.
            /// </summary>
            readonly List<Move> moves = new List<Move>();
            /// <summary>
            /// Gets the count of moves in the current movement.
            /// </summary>
            public int Count
            {
                get => moves.Count;
            }
            /// <summary>
            /// Gets a <see cref="Move"/> object in the movement by its index.
            /// </summary>
            /// <param name="index">The index of the requested move.</param>
            /// <returns>The requested <see cref="Move"/> object.</returns>
            public Move this[int index]
            {
                get => moves[index];
            }
            /// <summary>
            /// Add a <see cref="Move"/> object to the movement.
            /// </summary>
            /// <param name="move">The <see cref="Move"/> object to add.</param>
            internal void Add(Move move)
            {
                if(move.Original.row == move.Destination.row && move.Original.column == move.Destination.column) { return; }
                for(int i = 0; i < Count; i++)
                {
                    if (this[i].JoinConnecting(move)) { return; }
                }
                moves.Add(move);
            }
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<Move> GetEnumerator()
            {
                foreach(Move move in moves)
                {
                    yield return move;
                }
            }
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            /// <remarks>
            /// This is the non-generic version of <see cref="GetEnumerator"/>. Use the generic version instead.
            /// </remarks>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        /// <summary>
        /// Provides information to the <see cref="ScoreChanged"/> event.
        /// </summary>
        public class ScoreChangedEventArgs : EventArgs
        {
            /// <summary>
            /// The score of the board.
            /// </summary>
            public int Score { get; }
            /// <summary>
            /// Initializes an instance of <see cref="ScoreChangedEventArgs"/> with specified score.
            /// </summary>
            /// <param name="score">The socre of the board.</param>
            public ScoreChangedEventArgs(int score)
            {
                Score = score;
            }
        }
        /// <summary>
        /// Provides information for <see cref="TileAdded"/> event.
        /// </summary>
        public class TileAddedEventArgs : EventArgs
        {
            /// <summary>
            /// The location of the new tile.
            /// </summary>
            public (int row, int column) Location { get; }
            /// <summary>
            /// The value of the new tile.
            /// </summary>
            public int Value { get; }   
            /// <summary>
            /// Specify whether the tile addition is initiated by a board movement.
            /// </summary>
            public bool MoveInitiated { get; }
            /// <summary>
            /// Initialize an instance of <see cref="TileAddedEventArgs"/> with specified tile location and value, 
            /// and specification of whether the tile addition is initiated by a board movement.
            /// </summary>
            /// <param name="row">The row of the new tile.</param>
            /// <param name="column">The column of the new tile.</param>
            /// <param name="value">The value of the new tile.</param>
            /// <param name="moveInitiated">Whether the tile is added because of a board movement.</param>
            internal TileAddedEventArgs(int row, int column, int value, bool moveInitiated)
            {
                Location = (row, column);
                MoveInitiated = moveInitiated;
                Value = value;
            }
            /// <summary>
            /// Initialize an instance of <see cref="TileAddedEventArgs"/> with specified tile location and value, 
            /// and specification of whether the tile addition is initiated by a board movement.
            /// </summary>
            /// <param name="row">The row of the new tile.</param>
            /// <param name="column">The column of the new tile.</param>
            /// <param name="value">The value of the new tile.</param>
            public TileAddedEventArgs(int row, int column, int value) : this(row, column, value, false) { }
        }
        /// <summary>
        /// Provides information for <see cref="TilePromoted"/> event.
        /// </summary>
        public class TilePromotedEventArgs : EventArgs
        {
            /// <summary>
            /// Initialize an instance of <see cref="TilePromotedEventArgs"/> with specified tile location, old value, and new value.
            /// </summary>
            /// <param name="location">The tuple containing the row and column of the promoted tile.</param>
            /// <param name="oldValue">The value of the tile before promotion.</param>
            /// <param name="newValue">The value of the tile after promotion.</param>
            public TilePromotedEventArgs((int row, int column) location, int oldValue, int newValue)
            {
                Location = location;
                OldValue = oldValue;
                NewValue = newValue;
            }
            /// <summary>
            /// Initialize an instance of <see cref="TilePromotedEventArgs"/> with specified tile location, old value, and new value.
            /// </summary>
            /// <param name="row">The row of the promoted tile.</param>
            /// <param name="column">The column of the promoted tile.</param>
            /// <param name="oldValue">The value of the tile before promotion.</param>
            /// <param name="newValue">The value of the tile after promotion.</param>
            public TilePromotedEventArgs(int row, int column, int oldValue, int newValue) : this((row, column), oldValue, newValue) { }

            /// <summary>
            /// Initialize an instance of <see cref="TilePromotedEventArgs"/> with specified tile location and old value, 
            /// and defaults the new value to the old value times 2.
            /// </summary>
            /// <param name="row">The row of the promoted tile.</param>
            /// <param name="column">The column of the promoted tile.</param>
            /// <param name="oldValue">The value of the tile before promotion.</param>
            public TilePromotedEventArgs(int row, int column, int oldValue) : this(row, column, oldValue, oldValue * 2) { }
            /// <summary>
            /// The location of the promoted tile.
            /// </summary>
            public (int row, int column) Location { get; }
            /// <summary>
            /// The original value of the tile.
            /// </summary>
            public int OldValue { get; private set; }
            /// <summary>
            /// The new value of the tile.
            /// </summary>
            public int NewValue { get; private set; }
        }
        /// <summary>
        /// Provides information for <see cref="TileRemoved"/> event.
        /// </summary>
        public class TileRemovedEventArgs : EventArgs
        {
            /// <summary>
            /// Initialize an instance of <see cref="TileRemovedEventArgs"/> with specified tile location and value.
            /// </summary>
            /// <param name="location">The tuple containing the row and column of the removed tile.</param>
            /// <param name="value">The value of the removed tile.</param>
            public TileRemovedEventArgs((int row, int column) location, int value)
            {
                Location = location;
                Value = value;
            }
            /// <summary>
            /// Initialize an instance of <see cref="TileRemovedEventArgs"/> with specified tile location and value.
            /// </summary>
            /// <param name="row">The row of the removed tile.</param>
            /// <param name="column">The column of the removed tile.</param>
            /// <param name="value">The value of the removed tile.</param>
            public TileRemovedEventArgs(int row, int column, int value) : this((row, column), value) { }
            /// <summary>
            /// The location of the removed tile.
            /// </summary>
            public (int row, int column) Location { get; }
            /// <summary>
            /// The value of the removed tile.
            /// </summary>
            public int Value { get; }
        }
        /// <summary>
        /// Provides date to the <see cref="TilesMoved"/> event.
        /// </summary>
        public class TilesMovedEventArgs : EventArgs
        {
            /// <summary>
            /// A <see cref="MoveResult"/> object containing the details of the movement.
            /// </summary>
            public MoveResult Moves { get; private set; }
            /// <summary>
            /// Initializes an instance of <see cref="TilesMovedEventArgs"/> with specified <see cref="MoveResult"/> object.
            /// </summary>
            /// <param name="moves">A <see cref="MoveResult"/> object.</param>
            public TilesMovedEventArgs(MoveResult moves)
            {
                Moves = moves;
            }
        }
    }

    /// <summary>
    /// A 2048 board that performs a specified action at a specified interval.
    /// </summary>
    [Serializable] 
    public abstract class TimedBoard : Board
    {
        /// <summary>
        /// The times of action performance.
        /// </summary>
        int performCount = 0;
        /// <summary>
        /// Stores the interval of the timer.
        /// </summary>
        double interval = 0;
        /// <summary>
        /// Internal timer controlling the performance of action.
        /// </summary>
        System.Timers.Timer actionTimer = new System.Timers.Timer();

        /// <summary>
        /// Initializes a <see cref="TimedBoard"/> object with 4 * 4 size, interval of 1000 millisecond, and default action of doing nothing.
        /// </summary>
        protected TimedBoard() : this(4) { }
        /// <summary>
        /// Initializes a <see cref="TimedBoard"/> with specified size, interval of 1000 millisecond, and default action of doing nothing.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        protected TimedBoard(int size) : this(size, 1000) { }
        /// <summary>
        /// Initializes a <see cref="TimedBoard"/> with specified size and interval, and default action of doing nothing.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <param name="interval">The interval at which action will be performed.</param>
        protected TimedBoard(int size, double interval) : base(size)
        {
            Interval = interval;
            actionTimer.Elapsed += ActionTimer_Elapsed;
            TimerElapsed += TimerElapsedHandler;
            actionTimer.Start();
        }

        /// <summary>
        /// Initializes a clone of specified <see cref="TimedBoard"/> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        protected TimedBoard(TimedBoard board) : base(board)
        {
            Interval = board.Interval;
            performCount = board.performCount;
            actionTimer.Elapsed += ActionTimer_Elapsed;
            TimerElapsed += TimerElapsedHandler;
            actionTimer.Start();
        }
        /// <summary>
        /// Initializes a <see cref="TimedBoard"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        protected TimedBoard(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            performCount = (int)info.GetValue("performCount", typeof(int));
            Interval = (double)info.GetValue("interval", typeof(double));
            actionTimer.Elapsed += ActionTimer_Elapsed;
            TimerElapsed += TimerElapsedHandler;
            actionTimer.Enabled = (bool)info.GetValue("timerEnabled", typeof(bool));
        }

        /// <summary>
        /// Occurs when action is performed.
        /// </summary>
        public event EventHandler<TimerElapsedEventArgs> TimerElapsed;
        /// <summary>
        /// Gets the interval at which action will be performed.
        /// </summary>
        public double Interval
        {
            get => interval;
            private set
            {
                actionTimer.Interval = value;
                interval = value;
            }
        }
        /// <summary>
        /// Gets or sets the state of the internal timer. 
        /// </summary>
        protected bool TimerEnabled
        {
            get => actionTimer.Enabled;
            set => actionTimer.Enabled = value;
        }
        /// <summary>
        /// Handles the timer event.
        /// </summary>
        /// <param name="sender">The internal timer.</param>
        /// <param name="e">An <see cref="TimerElapsedEventArgs"/> containing information about the event.</param>
        /// <remarks>Override this method to specify action.</remarks>
        protected virtual void TimerElapsedHandler(object sender, TimerElapsedEventArgs e) { }

        /// <summary>
        /// Handling the Elapsed event of the timer by performing action.
        /// </summary>
        /// <param name="sender">The timer.</param>
        /// <param name="e">A <see cref="System.Timers.ElapsedEventArgs"/> object containing information of the event.</param>
        private void ActionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            performCount++;
            TimerElapsed?.Invoke(this, new TimerElapsedEventArgs(performCount, performCount * Interval));
        }
        /// <summary>
        /// Populate a <see cref="SerializationInfo"/> object with data needed for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to populate.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("performCount", performCount, typeof(int));
            info.AddValue("interval", interval, typeof(double));
            info.AddValue("timerEnabled", TimerEnabled, typeof(bool));
        }
        /// <summary>
        /// Provides data for <see cref="TimerElapsed"/> event.
        /// </summary>
        public class TimerElapsedEventArgs : EventArgs
        {
            /// <summary>
            /// The times the action has been performed.
            /// </summary>
            public int PerformCount { get; }
            /// <summary>
            /// Total running time of the timer.
            /// </summary>
            public double TotalTime { get; }
            /// <summary>
            /// Initializes an instance of <see cref="TimerElapsedEventArgs"/> with specified Perform Count and total running time.
            /// </summary>
            /// <param name="perfCount">The times the action has been performed.</param>
            /// <param name="totalTime">Total running time of the timer.</param>
            public TimerElapsedEventArgs(int perfCount, double totalTime)
            {
                PerformCount = perfCount;
                TotalTime = totalTime;
            }
        }
    }

    /// <summary>
    /// A 2048 board that adds a tile to the board at a specified interval.
    /// </summary>
    [Serializable]
    public sealed class TimedAddTileBoard : TimedBoard
    {
        /// <summary>
        /// Initializes a <see cref="TimedAddTileBoard"/> with 4 * 4 size and adds a tile to the board every 30 second.
        /// </summary>
        public TimedAddTileBoard() : this(4) { }
        /// <summary>
        /// Initializes a <see cref="TimedAddTileBoard"/> with specified size and adds a tile to the board every 15 second.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        public TimedAddTileBoard(int size) : this(size, 15000) { }
        /// <summary>
        /// Initializes a <see cref="TimedAddTileBoard"/> with specified size and adds a tile at a specified interval.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <param name="interval">The interval of adding tile in millisecond.</param>
        public TimedAddTileBoard(int size, double interval) : base(size, interval) { }
        /// <summary>
        /// Initializes a clone of specified <see cref="Board"/> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        public TimedAddTileBoard(TimedAddTileBoard board) : base(board) { }
        /// <summary>
        /// Initializes a <see cref="TimedAddTileBoard"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        TimedAddTileBoard(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// Handles the timer event.
        /// </summary>
        /// <param name="sender">The internal timer.</param>
        /// <param name="e">An <see cref="TimedBoard.TimerElapsedEventArgs"/> containing information about the event.</param>
        protected override void TimerElapsedHandler(object sender, TimerElapsedEventArgs e)
        {
            GenerateTile();
        }
        /// <summary>
        /// Returns a clone of current object.
        /// </summary>
        /// <returns>A clone of current object.</returns>
        public override Board Copy()
        {
            return new TimedAddTileBoard(this);
        }
        /// <summary>
        /// Populate a <see cref="SerializationInfo"/> object with data needed for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to populate.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
    /// <summary>
    /// A 2048 board that reduces specified score at a specified interval.
    /// </summary>
    [Serializable]
    public sealed class TimedReduceScoreBoard : TimedBoard
    {
        /// <summary>
        /// Initializes a <see cref="TimedReduceScoreBoard"/> with 4 * 4 size and reduces 4 points every 5 second.
        /// </summary>
        public TimedReduceScoreBoard() : this(4) { }
        /// <summary>
        /// Initializes a <see cref="TimedReduceScoreBoard"/> with specified size and reduces 4 points every 5 second.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        public TimedReduceScoreBoard(int size) : this(size, 5000) { }
        /// <summary>
        /// Initializes a <see cref="TimedReduceScoreBoard"/> with specified size and reduces 4 points at a specified interval.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <param name="interval">The interval of reducing points in millisecond.</param>
        public TimedReduceScoreBoard(int size, double interval) : this(size, interval, 4) { }
        /// <summary>
        /// Initializes a <see cref="TimedReduceScoreBoard"/> with specified size and reduces specified points at a specified interval.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <param name="interval">The interval of reducing points.</param>
        /// <param name="reduceScore">The point to reduce.</param>
        /// <remarks>
        /// The <paramref name="reduceScore"/> parameter should be positive to reduce score. 
        /// A positive <paramref name="reduceScore"/> parameter will credit score at the specified interval.
        /// </remarks>
        public TimedReduceScoreBoard(int size, double interval, int reduceScore) : base(size, interval)
        {
            ReduceScore = reduceScore;
        }
        /// <summary>
        /// Initializes a clone of specified <see cref="Board"/> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        public TimedReduceScoreBoard(TimedReduceScoreBoard board) : base(board)
        {
            ReduceScore = board.ReduceScore;
        }
        /// <summary>
        /// Initializes a <see cref="TimedReduceScoreBoard"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        TimedReduceScoreBoard(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ReduceScore = (int)info.GetValue("reduceScore", typeof(int));
        }
        /// <summary>
        /// The score to reduce at a specific interval.
        /// </summary>
        public int ReduceScore { get; }
        /// <summary>
        /// Handles the timer event.
        /// </summary>
        /// <param name="sender">The internal timer.</param>
        /// <param name="e">An <see cref="TimedBoard.TimerElapsedEventArgs"/> containing information about the event.</param>
        protected override void TimerElapsedHandler(object sender, TimerElapsedEventArgs e)
        {
            CreditScore(-ReduceScore); 
        }
        /// <summary>
        /// Returns a clone of current object.
        /// </summary>
        /// <returns>A clone of current object.</returns>
        public override Board Copy()
        {
            return new TimedReduceScoreBoard(this);
        }
        /// <summary>
        /// Populate a <see cref="SerializationInfo"/> object with data needed for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to populate.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("reduceScore", ReduceScore, typeof(int));
        }
    }
    /// <summary>
    /// A 2048 board that fails after a specified time.
    /// </summary>
    [Serializable]
    public sealed class TimedDeathBoard : TimedBoard
    {
        /// <summary>
        /// Initializes a <see cref="TimedDeathBoard"/> with 4 * 4 size and fails after specified time.
        /// </summary>
        /// <param name="time">The time before the game fails, in second.</param>
        public TimedDeathBoard(long time) : this(time, 4) { }
        /// <summary>
        /// Initializes a <see cref="TimedDeathBoard"/> with specified size that fails after specified time.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <param name="time">The time before the game fails in second.</param>
        public TimedDeathBoard(long time, int size) : base(size, 1000)
        {
            RemainingTime = time;
        }
        /// <summary>
        /// Initializes a clone of specified <see cref="Board"/> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        public TimedDeathBoard(TimedDeathBoard board) : base(board)
        {
            RemainingTime = board.RemainingTime;
        }
        /// <summary>
        /// Initializes a <see cref="Board"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        TimedDeathBoard(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            RemainingTime = (long)info.GetValue("remainingTime", typeof(long));
        }
        /// <summary>
        /// The remaining time before the game fails.
        /// </summary>
        public long RemainingTime { get; private set; }
        /// <summary>
        /// Returns a clone of current object.
        /// </summary>
        /// <returns>A clone of current object.</returns>
        public override Board Copy()
        {
            return new TimedDeathBoard(this);
        }
        /// <summary>
        /// Handles the timer event.
        /// </summary>
        /// <param name="sender">The internal timer.</param>
        /// <param name="e">An <see cref="TimedBoard.TimerElapsedEventArgs"/> containing information about the event.</param>
        protected override void TimerElapsedHandler(object sender, TimerElapsedEventArgs e)
        {
            RemainingTime--;
            if(RemainingTime <= 0)
            {
                FailGame(new TimedGameFailedEventArgs(TimedGameFailedEventArgs.FailedReason.TimesUp, Score));
                TimerEnabled = false;
            }
        }
        /// <summary>
        /// Populate a <see cref="SerializationInfo"/> object with data needed for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to populate.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("remainingTime", RemainingTime, typeof(long));
        }

        /// <summary>
        /// Provide additional information to <see cref="Board.GameFailedEventArgs"/>.
        /// </summary>
        public class TimedGameFailedEventArgs : GameFailedEventArgs
        {
            /// <summary>
            /// Specifying the reason of failure.
            /// </summary>
            public enum FailedReason
            {
                /// <summary>
                /// Specifying that the game is failed because the time is up.
                /// </summary>
                TimesUp,
                /// <summary>
                /// Specifying that the game is failed because the board is full and no further movement can be made.
                /// </summary>
                Normal
            }
            /// <summary>
            /// The reason the game failed.
            /// </summary>
            public FailedReason Reason { get; private set; }
            /// <summary>
            /// Initializes a <see cref="TimedGameFailedEventArgs"/> object with specified failure reason and score.
            /// </summary>
            /// <param name="reason">A <see cref="FailedReason"/> value specifying the reason the game failed.</param>
            /// <param name="score">The score of the game.</param>
            public TimedGameFailedEventArgs(FailedReason reason, int score) : base(score)
            {
                Reason = reason;
            }
        }
    }

    /// <summary>
    /// A 2048 board with immitigatable obstacles.
    /// </summary>
    [Serializable]
    public sealed class ObstacledBoard : Board
    {
        /// <summary>
        /// Initializes a <see cref="ObstacledBoard"/> with specified size and amount of obstacles.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <param name="obstacles">The amount of obstacles.</param>
        public ObstacledBoard(int size, int obstacles) : base(size)
        {
            ObstacleCount = obstacles;
            for(int i = 0; i < ObstacleCount; i++)
            {
                AddTile(-1);
            }
        }
        /// <summary>
        /// Initializes a <see cref="ObstacledBoard"/> with specified size.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        /// <remarks>
        /// The amount of obstacles is determined by expression <c>(int)(size / 2) - 1</c>. 
        /// </remarks>
        public ObstacledBoard(int size) : this(size, size / 2 - 1) { }
        /// <summary>
        /// Initializes a <see cref="ObstacledBoard"/> with 4 * 4 size and 1 obstacle.
        /// </summary>
        public ObstacledBoard() : this(4) { }
        /// <summary>
        /// Initializes a clone of specified <see cref="Board"/> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        public ObstacledBoard(ObstacledBoard board) : base(board)
        {
            ObstacleCount = board.ObstacleCount;
        }
        /// <summary>
        /// Initializes a <see cref="ObstacledBoard"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        ObstacledBoard(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ObstacleCount = (int)info.GetValue("obstacleCount", typeof(int));
        }
        /// <summary>
        /// Gets the count of obstacles on board.
        /// </summary>
        public int ObstacleCount { get; }
        /// <summary>
        /// Returns a clone of current object.
        /// </summary>
        /// <returns>A clone of current object.</returns>
        public override Board Copy()
        {
            return new ObstacledBoard(this);
        }
        /// <summary>
        /// Populate a <see cref="SerializationInfo"/> object with data needed for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to populate.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("obstacleCount", ObstacleCount, typeof(int));
        }
    }

    /// <summary>
    /// A 2048 board with items enabled.
    /// </summary>
    [Serializable]
    public sealed class ItemBoard : Board
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemBoard"/> with specified size.
        /// </summary>
        /// <param name="size">The size of the board, represented by the number of cells in one dimension.</param>
        public ItemBoard(int size) : base(size) { }
        /// <summary>
        /// Initializes a <see cref="ItemBoard"/> with 4 * 4 size.
        /// </summary>
        public ItemBoard() : this(4) { }

        /// <summary>
        /// Initializes a clone of specified <see cref="ItemBoard" /> object.
        /// </summary>
        /// <param name="board">The object to clone.</param>
        public ItemBoard(ItemBoard board) : base(board) { }

        /// <summary>
        /// Initializes a <see cref="ObstacledBoard"/> object from data provided in a <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to get data from.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) of this serialization.</param>
        ItemBoard(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Occurs when a tile is promoted by calling <see cref="PromoteTile(int, int)"/> method.
        /// </summary>
        public new event EventHandler<TilePromotedEventArgs> TilePromoted
        {
            add => base.TilePromoted += value;
            remove => base.TilePromoted -= value;
        }
        /// <summary>
        /// Occurs when a tile is removed by calling <see cref="RemoveTile(int, int)"/> method.
        /// </summary>
        public new event EventHandler<TileRemovedEventArgs> TileRemoved
        {
            add => base.TileRemoved += value;
            remove => base.TileRemoved -= value;
        }

        /// <summary>
        /// Generate a tile with specified value in specified cell.
        /// </summary>
        /// <param name="value">The desired value.</param>
        /// <param name="row">The row of the new tile.</param>
        /// <param name="column">The column of the new tile.</param>
        /// <returns>A tuple containing the row and column of the new tile, or (-1, -1) if the specified cell is occupied.</returns>
        /// <remarks>
        /// If a tile was successfully added, the method will fire <see cref="TileAdded"/> event.
        /// </remarks>
        public new (int row, int column) AddTile(int value, int row, int column) { return base.AddTile(value, row, column); }

        /// <summary>
        /// Returns a clone of current object.
        /// </summary>
        /// <returns>A clone of current object.</returns>
        public override Board Copy()
        {
            return new ItemBoard(this);
        }
        /// <summary>
        /// Promote a specified tile by one level.
        /// </summary>
        /// <param name="row">The row of the tile to promote.</param>
        /// <param name="column">The column of the tile to promote.</param>
        /// <returns><see langword="true"/> if the specified tile is removed, 
        /// <see langword="false"/> if no tile exists at the specified location or the tile is too large to promote.</returns>
        public new bool PromoteTile(int row, int column) { return base.PromoteTile(row, column); }
        /// <summary>
        /// Remove a specified tile.
        /// </summary>
        /// <param name="row">The row of the tile to remove.</param>
        /// <param name="column">The column of the tile to remove.</param>
        /// <returns><see langword="true"/> if the tile is removed, <see langword="false"/> if no tile exists at the specified location.</returns>
        public new bool RemoveTile(int row, int column) { return base.RemoveTile(row, column); }

        /// <summary>
        /// Undo the previous move.
        /// </summary>
        /// <remarks>Undo is not supported in this type of board. This method will do nothing.</remarks>
        public override void Undo()
        {
            // Not supported in this scenario.
        }
    }
}
