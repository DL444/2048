using System;
using ThemeSerializer2048;
using static LoginClient2048.LoginClient;

namespace TestConsole2048
{
    class Program
    {
        static void Main(string[] args)
        {
            AddHighscore("DL444", "YeoQzSOWJpFPlbhhCfdAsaKQ", 1, 4, 1024).GetAwaiter().GetResult();
            Console.WriteLine(GetHighscore(1, 4).GetAwaiter().GetResult()); 
        }
    }
}
