using System;
using System.Linq;
using Prepaid2048;

namespace CardGen2048
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            int value = 0;
            if (args.Length < 1)
            {
                Console.WriteLine("Specify value.");
                return;
            }
            if (args.Length == 1)
            {
                count = 1;
            }
            else
            {
                try
                {
                    count = int.Parse(args[1]);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid count parameter.");
                    return;
                }
            }

            try
            {
                value = int.Parse(args[0]);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid value parameter.");
                return;
            }

            if (value == 10 || value == 20 || value == 5 || value == 50 || value == 100)
            {
                for (int i = 0; i < count; i++)
                {
                    Console.WriteLine(PrepaidCardManager.GenerateCardKey(value));
                }
            }
            else
            {
                Console.WriteLine("Invalid value parameter.");
            }
        }
    }
}
