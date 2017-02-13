using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class GameUser : EntityBase
    {        
        public virtual Game21 Game21 {get;set;}
        public virtual User User { get; set; }
        public int OrderSequence { get; set; }
        public int Finished { get; set; }
        public bool SilentMode { get; set; }        
    }
}
