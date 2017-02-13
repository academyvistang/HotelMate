using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class Game21 : EntityBase
    {
        public virtual ICollection<GameUser> GameUsers { get; set; }
        public virtual ICollection<GameCard> GameCards { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
        public string Status { get; set; }
    }
}
