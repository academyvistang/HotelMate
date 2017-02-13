using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

using System.ComponentModel.DataAnnotations;

namespace Entitities
{
    public class User : EntityBase
    {
        public string UserName { get; set; }
        public string PrevUserName { get; set; }
        public bool CurrentUser { get; set; }
        public bool playingNow  { get; set; }
        public int playingSeq  { get; set; }


        public string Password { get; set; }
        public string Email { get; set; }
        public decimal CurrentStake { get; set; }
        public bool IsComputer { get; set; }
        public bool Male { get; set; }

        public string WinningMessage { get; set; }
        public string LoosingMessage { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; }
        public string GameName { get; set; }
        public string UserPictureName { get; set; }

        public DateTime LastLoggedInDate { get; set; }
        public decimal UserBalance { get; set; }
        public decimal RealMoneyBalance { get; set; }
        public decimal PlayBalance  { get; set; }

        public bool IsGameController { get; set; }

        public bool ForfitChanceToPlay { get; set; }

        public int ShowFinish { get; set; }

        //[NotMapped]
        public  IList<Card> UsersStack { get; set; }

        //[Ignore]
        public  IList<Card> UsersPlayingStack { get; set; }         

        
    }
}