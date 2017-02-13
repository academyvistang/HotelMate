using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Entitities.DbContextFolder
{   

    public interface IAgbo21Context
    {
        IDbSet<Card> Cards { get; set; }
        IDbSet<Game21> Games { get; set; }
        IDbSet<GameCard> GameCards { get; set; }
        IDbSet<GamePlayingNow> GamePlayingNows { get; set; }
        IDbSet<GameUser> GameUsers { get; set; }
        IDbSet<Message> Messages { get; set; }
        IDbSet<Rank> Ranks { get; set; }
        IDbSet<Suit> Suits { get; set; }
        IDbSet<User> Users { get; set; }
        IDbSet<UserAccount> UserAccounts { get; set; }
        int SaveChanges();
    }
}
