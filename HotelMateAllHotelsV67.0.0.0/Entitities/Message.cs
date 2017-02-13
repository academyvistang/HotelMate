using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class Message : EntityBase
    {
        public virtual Game21 Game { get; set; }
        public string TextMessage { get; set; }
        public int OrderSeq { get; set; }
    }
}
