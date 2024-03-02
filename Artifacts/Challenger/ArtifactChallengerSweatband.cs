using APurpleApple.Shipyard.CardActions;
using APurpleApple.Shipyard.Parts;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Artifacts.Challenger
{
    public class ArtifactChallengerSweatband : Artifact, IModArtifact
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
                    pools = [ArtifactPool.Common],
                },
                Sprite = PMod.sprites["Artifact_Sweatband"].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Sweatband", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Sweatband", "description"]).Localize
            });
        }

        public override void OnPlayerLoseHull(State state, Combat combat, int amount)
        {
            combat.Queue(new AStatus() { targetPlayer = true, status = Status.autododgeRight, artifactPulse = this.Key(), statusAmount = 1 });
        }

        public override List<Tooltip>? GetExtraTooltips()
        {
            List<Tooltip> tooltips = new List<Tooltip>();

            tooltips.Add(new TTGlossary($"status.autododgeRight"));

            return tooltips;
        }
    }
}
