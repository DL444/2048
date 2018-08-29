//#define MDEBUG
using Lib2048;
using System;

namespace Test2048
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "2048";
            ConsoleKeyInfo k = new ConsoleKeyInfo();
            Console.WriteLine("Console 2048 by David Lee");
            Console.WriteLine();
            Console.WriteLine("Arrow Key: Move tiles");
            Console.WriteLine("        U: Undo");
            Console.WriteLine("      Esc: Suicide");
            Console.WriteLine();
            int size = 0;
            while(size < 3)
            {
                Console.Write("Enter board size (no less than 3): ");
                try
                {
                    size = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.Clear();
                    continue;
                }
                Console.Clear();
            }

            Board board = new Board(size);
            bool failed = false;

            board.GameFailed += (object sender, Board.GameFailedEventArgs e) => { failed = true; };

            Action beep = new Action(() =>
            {
                Console.Beep(1000, 200);
            });

            Display(board);
            while(!failed)
            {
                Board.MoveResult result = new Board.MoveResult();
                k = Console.ReadKey();
                switch(k.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        result = board.Move(Board.MoveDirection.Up);
                        beep.BeginInvoke(null, null);
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        result = board.Move(Board.MoveDirection.Down);
                        beep.BeginInvoke(null, null);
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        result = board.Move(Board.MoveDirection.Left);
                        beep.BeginInvoke(null, null);
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        result = board.Move(Board.MoveDirection.Right);
                        beep.BeginInvoke(null, null);
                        break;
                    case ConsoleKey.U:
                        board.Undo();
                        beep.BeginInvoke(null, null);
                        break;
                    case ConsoleKey.Escape:
                        failed = true;
                        beep.BeginInvoke(null, null);
                        break;
                }
                Console.Clear();
                Display(board);
#if MDEBUG
                DisplayResult(result);
#endif
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"You are dead! Score = {board.Score}");
        }


        static ConsoleColor[] foreground = new ConsoleColor[]
        {
            ConsoleColor.Gray,
            ConsoleColor.Blue, ConsoleColor.DarkCyan, ConsoleColor.Cyan,
            ConsoleColor.DarkGreen, ConsoleColor.Green, ConsoleColor.DarkYellow, ConsoleColor.Yellow,
            ConsoleColor.DarkMagenta, ConsoleColor.Magenta, ConsoleColor.DarkRed, ConsoleColor.Red
        };
        //static ConsoleColor[] background = new ConsoleColor[]
        //{
        //    ConsoleColor.Black,
        //    ConsoleColor.Red, ConsoleColor.DarkRed, ConsoleColor.Magenta, ConsoleColor.DarkMagenta,
        //    ConsoleColor.Yellow, ConsoleColor.DarkYellow, ConsoleColor.Green, ConsoleColor.DarkGreen,
        //    ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue
        //};

        static void Display(Board brd)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"Score: {brd.Score}");
            Console.WriteLine();
            for(int i = 0; i < brd.Size; i++)
            {
                for(int j = 0; j < brd.Size; j++)
                {
                    int colorCode = brd[i, j] == 0 ? 0 : (int)Math.Log(brd[i, j], 2);
                    colorCode = colorCode > 11 ? 11 : colorCode;
                    Console.ForegroundColor = foreground[colorCode];
                    //Console.BackgroundColor = background[colorCode];
                    Console.Write($"{brd[i, j], 4}");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        static void DisplayResult(Board.MoveResult result)
        {
            foreach(Board.MoveResult.Move move in result)
            {
                Console.WriteLine($"Move ({move.Original.row + 1}, {move.Original.column + 1}) to ({move.Destination.row + 1}, {move.Destination.column + 1}).");
            }
        }
    }
}
