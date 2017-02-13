using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entitities;

namespace HotelMateWebV1.Models
{
    public class GameViewModel
    {
        public IEnumerable<Game21> LiveGames { get; set; }

        public User User { get; set; }      

        public string CurrentUserName { get; set; }      

        public int GameId { get; set; }

        public string GlobalUrlAction { get; set; }

        public decimal CurrentUserBalance { get; set; }

        public int NumberOfPlayers { get; set; }

        public int GameStake { get; set; }

        public List<GamePlayingNow> CurrentGames { get; set; }

        public bool UserBalanceFailure { get; set; }
    }
}
