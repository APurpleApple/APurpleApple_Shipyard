using APurpleApple.Shipyard.Artifacts;
using APurpleApple.Shipyard.VFXs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.CardActions
{
    internal class AEjectedPartReturnManager : CardAction
    {
        public override void Begin(G g, State s, Combat c)
        {
            ArtifactAsteroid? artifact = s.artifacts.Find((x) => x is ArtifactAsteroid) as ArtifactAsteroid;
            if (artifact == null) return;
            c.Queue(new CardAction() { timer = 1 });

            int index = 0;
            for (var i = 0; i < s.ship.parts.Count; i++)
            {
                if (s.ship.parts[i].key == "AsteroidScaffolding" && artifact.ejectedParts.Count > index)
                {
                    c.Queue(new AEjectedPartReturn() { part = artifact.ejectedParts[index], index = i, timer = 0 });
                    c.fx.Add(new PartEjectionReturn() { part = artifact.ejectedParts[index], worldX = (i + s.ship.x) * 16 });
                    index++;
                }
            }


            artifact.ejectedParts.Clear();
        }
    }
}
