using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login2048.Model
{
    public class Highscore
    {
        public long Id { get; set; }
        public long Score { get; set; }
        public string Username { get; set; }
        public int Mode { get; set; }
        public int Size { get; set; }
        public long Time { get; set; }
    }

    public enum GameModes
    {
        Normal = 0, TimedAddTile = 1, TimedReduceScore = 2, TimedTenMin = 3, Obstacles = 4, Tools = 5
    }
}
