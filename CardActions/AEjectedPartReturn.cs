using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.CardActions
{
    internal class AEjectedPartReturn : CardAction
    {
        public Part? part;
        public int index = 0;
        public override void Begin(G g, State s, Combat c)
        {
            if (part == null) return;
            s.ship.parts[index] = part;
            part.xLerped = index;
            part.hilight = false;
        }
    }
}
