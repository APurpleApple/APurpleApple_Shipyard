using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    public class ArtifactAsteroid : Artifact, IModArtifact
    {
        public int fistMovement = 0;

        public Dictionary<string, Part> ejectedParts = new();
        public Dictionary<string, int> turnsBeforeComeback = new();
        public Dictionary<string, int> originalPlace = new();

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
                Sprite = PMod.sprites[PSpr.Artifacts_AsteroidArtifact].Sprite,
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
            foreach (var item in ejectedParts.Values)
            {
                if (item.key == null) continue;
                state.ship.parts[originalPlace[item.key]] = item;

                turnsBeforeComeback.Remove(item.key);
                ejectedParts.Remove(item.key);
                originalPlace.Remove(item.key);
            }

            ejectedParts.Clear();
        }
    }
}
