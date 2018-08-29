using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login2048.Model
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Highscore> Highscores { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Card> RedeemedCards { get; set; }
        public DbSet<Theme> Themes { get; set; }
    }
}
