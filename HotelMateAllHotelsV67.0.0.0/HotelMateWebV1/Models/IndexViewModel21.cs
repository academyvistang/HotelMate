using Entitities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Models
{

    public class IndexViewModel21
    {
        public IEnumerable<Card> AllCards { get; set; }

        public IEnumerable<Game21> LiveGames { get; set; }

        public User User { get; set; }

        public User Computer { get; set; }

        public Card[] AgboFloor { get; set; }

        public User User1 { get; set; }

        public User User2 { get; set; }

        public string CurrentUserName { get; set; }

        public List<GameUser> GameUsers { get; set; }

        public List<CardOwner> CardOwners { get; set; }

        public int GameId { get; set; }

        public string GlobalUrlAction { get; set; }

        public decimal CurrentUserBalance { get; set; }

        public int LiveGamesCount { get; set; }

        public bool DontShowDiscussionDiv { get; set; }
    }
}