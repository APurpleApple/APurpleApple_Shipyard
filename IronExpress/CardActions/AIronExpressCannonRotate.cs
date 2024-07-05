using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.IronExpress
{
    public class AIronExpressCannonRotate : CardAction
    {
        public bool? isCannon;

        public override void Begin(G g, State s, Combat c)
        {
            int cannonIndex = s.ship.parts.FindIndex((p) => p is PartRailCannon);
            PartRailCannon cannon = (PartRailCannon)s.ship.parts[cannonIndex];

            if (!isCannon.HasValue)
            {
                isCannon = !cannon.isCannon;
            }

            if (cannon.isCannon != isCannon )
            {
                cannon.isCannon = isCannon.Value;
                if (isCannon.Value)
                {
                    cannon.damageModifier = PDamMod.weak;
                    cannon.type = PType.cannon;
                    cannon.active = true;
                }
                else
                {
                    cannon.damageModifier = PDamMod.armor;
                    cannon.type = PType.special;
                    cannon.active = false;
                }

                foreach (IIronExpressHook a in s.EnumerateAllArtifacts().Where((a) => a is IIronExpressHook))
                {
                    a.OnIronExpressRotate(c, s, cannon);
                }
            }
        }
    }
}
