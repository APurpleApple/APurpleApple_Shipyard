using APurpleApple.Shipyard.CardActions;
using APurpleApple.Shipyard.Interfaces;
using APurpleApple.Shipyard.Parts;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Artifacts.IronExpress
{
    public class ArtifactIronExpressV2 : Artifact, IModArtifact, IIronExpressHook
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
                    pools = [ArtifactPool.Boss],
                    unremovable = false,
                },
                Sprite = PMod.sprites["Artifact_IronExpressV2"].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "IronExpressV2", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "IronExpressV2", "description"]).Localize
            });
        }

        public void OnIronExpressRotate(Combat c, State s, PartRailCannon cannon)
        {
            if (!cannon.isCannon)
            {
                c.QueueImmediate(new AStatus() { targetPlayer = true, status = Status.overdrive, statusAmount = 1, artifactPulse = this.Key() });
            }
        }

        public void OnIronExpressSlide(Combat c, State s, PartRailCannon cannon)
        {
        }
    }
}
