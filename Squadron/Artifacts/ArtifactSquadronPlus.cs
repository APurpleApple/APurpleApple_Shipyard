using FSPRO;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Squadron
{
    internal class ArtifactSquadronPlus : Artifact, IModArtifact
    {
        public Deck? activePilot = null;

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
                    pools = [ArtifactPool.Boss]
                },
                Sprite = PMod.sprites[PSpr.Artifacts_Squadron4th].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Squadron4th", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Squadron4th", "description"]).Localize
            });
        }

        private void AddStartersForCharacter(Deck d, State s)
        {
            StarterDeck? starterDeck;
            if (!StarterDeck.starterSets.TryGetValue(d, out starterDeck))
                return;
            foreach (Card card in Enumerable.Select<Card, Card>((IEnumerable<Card>)starterDeck.cards, (Func<Card, Card>)(c => c.CopyWithNewId())))
                s.SendCardToDeck(card);
            foreach (Artifact r in Enumerable.Select<Artifact, Artifact>((IEnumerable<Artifact>)starterDeck.artifacts, (Func<Artifact, Artifact>)(r => Mutil.DeepCopy<Artifact>(r))))
                s.SendArtifactToChar(r);
        }

        public override void OnReceiveArtifact(State state)
        {
            
            state.ship.baseEnergy += 1;

            state.ship.parts.Add(new Part() { type = PType.empty });
            Rand rng = new Rand(state.rngCurrentEvent.seed + 40455781);
            List<Deck> list = (from dt in state.storyVars.GetUnlockedChars()
                               where !state.characters.Any((Character ch) => ch.deckType == dt)
                               select dt).ToList();

            Deck foundCharacter = list.Random(rng);

            state.ship.parts.Add(new PartSquadronUnit()
            {
                type = PType.special,
                skin = "",
                damageModifier = PDamMod.none,
                key = "SquadronUnit",
                pilot = foundCharacter
            });

            state.GetCurrentQueue().Add(new AAddCharacter() { deck = foundCharacter, addTheirStarterCardsAndArtifacts = true, canGoPastTheCharacterLimit = true });

            //AddStartersForCharacter(foundCharacter, state);
        }
    }
}
