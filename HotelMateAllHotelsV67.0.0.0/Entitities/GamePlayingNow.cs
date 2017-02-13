using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities.Core;

namespace Entitities
{
    public class GamePlayingNow : EntityBase
    {
        public virtual Game21 Game { get; set; }
        public int ValueNum { get; set; }
        public int GameStage { get; set; }
        public bool Contested { get; set; }
    }
}
