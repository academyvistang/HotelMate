using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class GameCard : EntityBase
    {

        public virtual Game21 Game { get; set; }
        public virtual User User  { get; set; }
        public virtual Card Card { get; set; }

        public string Status  { get; set; }  
        public int OrderSeq  { get; set; }

    
        public bool ShowNumberedSide  { get; set; }
        public bool IsABlank  { get; set; }
        public bool TradingFloorCard  { get; set; }
    }
}
