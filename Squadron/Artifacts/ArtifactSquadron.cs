using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Squadron
{
    internal class ArtifactSquadron : Artifact, IModArtifact
    {
        public Deck? leader = null;

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
                    pools = [ArtifactPool.EventOnly]
                },
                Sprite = PMod.sprites[PSpr.Artifacts_Squadron].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Squadron", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Squadron", "description"]).Localize
            });
        }

        public override void OnReceiveArtifact(State state)
        {
            int i = 0;
            foreach (var item in state.ship.parts)
            {
                if (item is PartSquadronUnit unit && i < state.characters.Count)
                {
                    unit.pilot = state.characters[i].deckType;
                    i++;
                }
            }
        }

        public override void OnCombatEnd(State state)
        {
            List<PartSquadronUnit> units = new List<PartSquadronUnit>();

            foreach (var item in state.ship.parts)
            {
                if (item is PartSquadronUnit unit)
                {
                    units.Add(unit);
                }
            }

            state.ship.parts = new List<Part>();

            for (var i = 0; i < units.Count; i++)
            {
                if (i != 0)
                {
                    state.ship.parts.Add(new Part() { type = PType.empty });
                }
                state.ship.parts.Add(units[i]);
            }
        }

        public override void OnPlayerTakeNormalDamage(State state, Combat combat, int rawAmount, Part? part)
        {
            if (part == null) return;
            if (part is not PartSquadronUnit unit) return;
            if (!unit.pilot.HasValue) return;
            SquadronPatches.SetLeader(state, unit.pilot);

            Status missingStatus;
            if (!StatusMeta.deckToMissingStatus.TryGetValue(unit.pilot.Value, out missingStatus)) return;
            if (state.ship.Get(missingStatus) > 0) return;
            
            combat.Queue(new AStatus() { status= missingStatus, statusAmount = 1, targetPlayer = true });
        }

        
        public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
        {
            foreach (Part part in state.ship.parts)
            {
                if (part is PartSquadronUnit unit)
                {
                    if (unit.pilot == deck)
                    {
                        leader = deck;
                        break;
                    }
                }
            }
        }
    }
}
