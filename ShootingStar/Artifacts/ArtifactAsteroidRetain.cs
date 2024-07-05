using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    public class ArtifactAsteroidRetain : Artifact, IModArtifact
    {
        public bool active = true;
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
                    unremovable = true,
                },
                Sprite = PMod.sprites[PSpr.Artifacts_AsteroidRetain].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "MagnetizedParts", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "MagnetizedParts", "description"]).Localize
            });
        }

        public override void OnTurnStart(State state, Combat combat)
        {
            active = true;
        }

        public override void OnDrawCard(State state, Combat combat, int count)
        {
            if( !active ) return;

            for (int i = 0; i < combat.hand.Count; i++)
            {
                if (combat.hand[i].GetActions(state, combat).Any((a)=>a is AAsteroidEjectPart))
                {
                    if (!combat.hand[i].retainOverride.HasValue)
                    {
                        Pulse();
                        combat.hand[i].retainOverride = true;
                        active = false;
                        break;
                    }
                }
            }
        }

        public override void OnCombatEnd(State state)
        {
            foreach (var card in state.deck)
            {
                if (card.retainOverride.HasValue && !card.retainOverrideIsPermanent)
                {
                    if (card.GetActions(state, DB.fakeCombat).Any((a) => a is AAsteroidEjectPart) && card.retainOverride.Value)
                    {
                        card.retainOverride = null;
                    }
                }
            }
        }

        public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
        {
            if (card.retainOverride.HasValue && !card.retainOverrideIsPermanent)
            {
                if (card.GetActions(state, combat).Any((a) => a is AAsteroidEjectPart) && card.retainOverride.Value)
                {
                    card.retainOverride = null;
                }
            }
            
        }
    }
}
