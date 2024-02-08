using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.CardActions
{
    public class AChallengerResetPartsActive : CardAction
    {
        public override void Begin(G g, State s, Combat c)
        {
            timer = 0;
            canRunAfterKill = true;

            foreach (Part p in s.ship.parts)
            {
                if (p.key == "ChallengerFist")
                {
                    p.active = true;
                }
            }
        }
    }
}
