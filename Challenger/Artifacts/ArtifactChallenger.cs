using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Challenger
{
    public class ArtifactChallenger : Artifact, IModArtifact
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
                    pools = [ArtifactPool.EventOnly],
                    unremovable = true,
                },
                Sprite = PMod.sprites[PSpr.Artifacts_Challenger].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Challenger", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Challenger", "description"]).Localize
            });
        }

        public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
        {
            if (handCount % 2 == 1 && handPosition == handCount / 2)
            {
                // combat.Queue(new AStatus() { targetPlayer = true, status = Status.heat, statusAmount = 1 });
            }

            foreach (PartChallengerFist p in state.ship.parts.Where(x => x is PartChallengerFist))
            {
                if (handCount % 2 == 1 && handPosition == handCount / 2)
                {
                    p.active = true;
                }
                else
                {
                    if (handPosition < handCount / 2)
                    {
                        p.active = !p.flip;
                    }
                    else
                    {
                        p.active = p.flip;
                    }
                }
            }
            //combat.Queue(new AChallengerResetPartsActive());
        }

    }
}
