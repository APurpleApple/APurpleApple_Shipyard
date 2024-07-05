using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.IronExpress
{
    public class ArtifactIronExpress : Artifact, IModArtifact
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
                Sprite = PMod.sprites[PSpr.Artifacts_IronExpress].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "IronExpress", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "IronExpress", "description"]).Localize
            });
        }

        public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
        {
            if (handPosition == 0 && handCount > 1)
            {
                MoveCannon(state, -1);
            }

            if (handCount % 2 == 1 && handPosition == handCount / 2)
            {
                if (state.route is Combat c)
                {
                    c.Queue(new AIronExpressCannonRotate());

                    PartRailCannon? cannon = state.ship.parts.First(p => p is PartRailCannon) as PartRailCannon;
                    if (cannon != null && cannon.isCannon)
                    {
                        c.Queue(new AStatus() { targetPlayer = true, status = SStatus.shield, statusAmount = 2});
                    }
                }
            }

            if (handPosition == handCount - 1 && handCount > 1)
            {
                MoveCannon(state, 1);
            }
        }

        public void MoveCannon(State s, int direction)
        {
            if (s.route is Combat c)
            {
                c.Queue(new AIronExpressCannonSlide() { direction = direction });
            }
        }
    }
}
