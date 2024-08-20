using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Ouranos
{
    internal class ABeamAttack : AAttack
    {
        public List<AAttack> storedattacks = new List<AAttack>();

        public override void Begin(G g, State s, Combat c)
        {
            foreach (var item in storedattacks)
            {
                item.multiCannonVolley = true;
                item.fromX = fromX;
                item.Begin(g,s,c);
                Console.WriteLine($"shot {item.damage}");
            }
            EffectSpawnerExtension.RailgunBeam(c, s.ship.x + fromX!.Value, damage, new Color("ff8866")) ;
        }
    }
}
