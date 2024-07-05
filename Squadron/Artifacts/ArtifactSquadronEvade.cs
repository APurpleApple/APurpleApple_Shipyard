using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Squadron
{
    internal class ArtifactSquadronEvade : Artifact, IModArtifact
    {
        public static void Register(IModHelper helper)
        {
            Type type = MethodBase.GetCurrentMethod()!.DeclaringType!;
            helper.Content.Artifacts.RegisterArtifact(type.Name, new()
            {
                ArtifactType = type,
                Meta = new()
                {
                    owner = Deck.colorless,
                    unremovable = true,
                    pools = [ArtifactPool.Common]
                },
                Sprite = PMod.sprites[PSpr.Artifacts_EvasiveAction].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "EvasiveAction", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "EvasiveAction", "description"]).Localize
            });
        }

        public override void OnTurnStart(State state, Combat combat)
        {
            if (state.ship.Get(Status.evade) == 0)
            {
                combat.Queue(new AStatus() { status = Status.evade, targetPlayer = true, statusAmount = 1 });
            }
        }
    }
}
