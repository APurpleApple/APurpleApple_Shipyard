using APurpleApple.Shipyard.CardActions;
using APurpleApple.Shipyard.Parts;
using APurpleApple.Shipyard.VFXs;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Artifacts
{
    public class ArtifactAsteroid : Artifact, IModArtifact
    {
        public int fistMovement = 0;

        public List<Part> ejectedParts = new List<Part>();
        public List<int> turnBeforeComeback = new List<int>();

        public static void Register(IModHelper helper)
        {
            Type type = MethodBase.GetCurrentMethod()!.DeclaringType!;
            helper.Content.Artifacts.RegisterArtifact(type.Name, new()
            {
                ArtifactType = type,
                Meta = new()
                {
                    owner = Deck.colorless,
                    pools = [ArtifactPool.EventOnly],
                    unremovable = true,
                },
                Sprite = PMod.sprites["Asteroid_Artifact"].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Asteroid", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Asteroid", "description"]).Localize
            });
        }

        public override void OnTurnStart(State state, Combat combat)
        {
            if (ejectedParts.Count == 0) return;
            combat.Queue(new AEjectedPartReturnManager() { timer = 0 });
        }

        public override void OnCombatEnd(State state)
        {
            if (ejectedParts.Count == 0) return;
            int index = 0;
            for (var i = 0; i < state.ship.parts.Count; i++)
            {
                if (state.ship.parts[i].key == "AsteroidScaffolding" && ejectedParts.Count > index)
                {
                    state.ship.parts[i] = ejectedParts[index];
                    ejectedParts[index].xLerped = i;
                    index++;
                }
            }
            ejectedParts.Clear();
        }
    }
}
