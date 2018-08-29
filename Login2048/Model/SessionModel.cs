using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login2048.Model
{
    public class Session
    {
        public long Id { get; set; }
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public long LastRefresh { get; set; }
    }
}
