using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class Card :EntityBase
    {
        public string FileLocation { get; set; }
        public string BlankLocation { get; set; }
        public int CardNumberValue { get; set; }
        public bool IsABlank { get; set; }

        public virtual Rank Rank { get; set; }
        public virtual Suit Suit { get; set; }
        public bool ShowNumberedSide { get; set; }
    }
}
