using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.DbContextFolder;
using System.Data.Entity;

namespace Entitities
{
    public class Agbo21Context : DbContext, IAgbo21Context
    {
        public IDbSet<Card> Cards { get; set; }
        public IDbSet<Game21> Games { get; set; }
        public IDbSet<GameCard> GameCards { get; set; }
        public IDbSet<GamePlayingNow> GamePlayingNows { get; set; }
        public IDbSet<GameUser> GameUsers { get; set; }
        public IDbSet<Message> Messages { get; set; }
        public IDbSet<Rank> Ranks { get; set; }
        public IDbSet<Suit> Suits { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<UserAccount> UserAccounts { get; set; }
       
    }
}
