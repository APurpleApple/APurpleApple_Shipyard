using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Squadron.CardActions
{
    internal class APushSquadronParts : CardAction
    {
        public bool pushLeft = false;
        public int wallX = 0;

        public override void Begin(G g, State s, Combat c)
        {
            List<Part> parts = new();
            List<int> partXs = new();

            for (int i = 0; i < s.ship.parts.Count; i++)
            {
                if (s.ship.parts[i] is PartSquadronUnit)
                {
                    partXs.Add(i + s.ship.x);
                    parts.Add(s.ship.parts[i]);
                }
            }

            if (pushLeft)
            {
                for (int i = 0; i < partXs.Count; i++)
                {
                    partXs[i] = int.Min(partXs[i], wallX - (partXs.Count - (i+1)));
                }
            }
            else
            {
                for (int i = partXs.Count - 1; i >= 0; i--)
                {
                    partXs[i] = int.Max(partXs[i], wallX + i);
                }
            }

            s.ship.x = partXs[0];
            List<Part> finalLayout = new();

            int x = s.ship.x;
            for (int i = 0; i < parts.Count; i++)
            {
                while (x < partXs[i])
                {
                    finalLayout.Add(new Part() { type = PType.empty });
                    x++;
                }

                finalLayout.Add(parts[i]);
                x++;
            }


            s.ship.parts = finalLayout;
        }
    }
}
