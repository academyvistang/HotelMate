using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entitities;

namespace HotelMateWebV1.Models
{
    public class CardOwner
    {
        public User Owner { get; set; }
        public string PlayingNow { get; set; }
        public List<CardModel> CardModelsPlayingStack { get; set; }
        public List<CardModel> CardModelsPlayedStack { get; set; }

        public string WhoIsPlayingMessage { get; set; }

        public string CanClickFinish { get; set; }

        public string CanClickFinishText { get; set; }

        public bool CanShowFlashMessage { get; set; }

        public string CanShowFlashMessageMessage { get; set; }

        public bool GameIsOver { get; set; }

        public string TheWinnerIs { get; set; }

        public string CanClickContestText { get; set; }

        public string CanClickContest { get; set; }

        public string GameStake { get; set; }
    }

    public class CardModel
    {
        public Card Card { get;set; }       
        public string ActionUrl { get; set; }
        public bool IsStarCard { get; set; }

        public string Status { get; set; }
    }
}