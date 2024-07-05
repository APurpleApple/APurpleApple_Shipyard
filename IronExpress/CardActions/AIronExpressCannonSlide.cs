using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.IronExpress
{
    public class AIronExpressCannonSlide : CardAction
    {
        public int direction = 0;

        public override void Begin(G g, State s, Combat c)
        {
            int cannonIndex = s.ship.parts.FindIndex((p) => p is PartRailCannon);

            int newLoc = cannonIndex + direction;
            PartRailCannon cannon = (PartRailCannon)s.ship.parts[cannonIndex];


            if (s.EnumerateAllArtifacts().Any((x)=>x is ArtifactIronExpressMobius))
            {
                if (newLoc < 0 || newLoc > s.ship.parts.Count - 1)
                {
                    newLoc = (newLoc + s.ship.parts.Count) % s.ship.parts.Count;
                    cannon.xLerped = newLoc;
                }
            }

            if (direction != 0 && newLoc >= 0 && newLoc <= s.ship.parts.Count - 1)
            {
                if (cannon.overlapedPart != null)
                {
                    s.ship.parts[cannonIndex] = cannon.overlapedPart;
                    cannon.overlapedPart = s.ship.parts[newLoc];
                    s.ship.parts[newLoc] = cannon;
                }

                foreach (IIronExpressHook a in s.EnumerateAllArtifacts().Where((a) => a is IIronExpressHook))
                {
                    a.OnIronExpressSlide(c, s, cannon);
                }
            }
        }
    }
}
