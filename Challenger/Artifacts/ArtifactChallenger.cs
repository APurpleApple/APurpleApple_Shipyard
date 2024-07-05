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
        public int fistMovement = 0;

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

            foreach (Part p in state.ship.parts)
            {
                if (p.key == "ChallengerFist")
                {
                    if (handCount % 2 == 1 && handPosition == handCount / 2)
                    {
                        fistMovement = 0;
                        p.active = true;
                    }
                    else
                    {
                        if (handPosition < handCount / 2)
                        {
                            fistMovement = 1;
                            p.active = !p.flip;
                        }
                        else
                        {
                            fistMovement = -1;
                            p.active = p.flip;
                        }
                    }
                }
            }

            combat.Queue(new AChallengerResetPartsActive());
        }

        public override void OnEnemyGetHit(State state, Combat combat, Part? part)
        {
            if (fistMovement != 0)
            {
                if (combat.currentCardAction == null) return;
                if (combat.currentCardAction is not AAttack attack) return;
                if (attack.fromDroneX.HasValue) return;
                attack.timer = 0.0;
                combat.QueueImmediate(new AMove() { dir = fistMovement, targetPlayer = false });
                for (int i = 0; i < combat.cardActions.Count; i++)
                {
                    if (combat.cardActions[i] is AStunPart a)
                    {
                        combat.cardActions.Remove(a);
                        a.timer = 0.0;
                        combat.cardActions.Insert(0, a);
                        break;
                    }
                }
            }
        }
    }
}
