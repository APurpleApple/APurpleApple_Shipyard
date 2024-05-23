using APurpleApple.Shipyard.Artifacts;
using APurpleApple.Shipyard.VFXs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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


            foreach (var item in artifact.ejectedParts.Values)
            {
                if (item.key == null) continue;
                artifact.turnsBeforeComeback[item.key]--;
                if (artifact.turnsBeforeComeback[item.key] == 0)
                {
                    c.Queue(new AEjectedPartReturn() { part = item, index = artifact.originalPlace[item.key], timer = 0 });
                    c.fx.Add(new PartEjectionReturn() { part = item, worldX = (artifact.originalPlace[item.key] + s.ship.x) * 16 });

                    artifact.turnsBeforeComeback.Remove(item.key);
                    artifact.ejectedParts.Remove(item.key);
                    artifact.originalPlace.Remove(item.key);
                }
            }
        }
    }
}
