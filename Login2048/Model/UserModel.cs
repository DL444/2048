using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login2048.Model
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int Coins { get; set; }
    }
}
