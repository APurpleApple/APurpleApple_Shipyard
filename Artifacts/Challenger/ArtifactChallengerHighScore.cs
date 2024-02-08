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
    public class ArtifactChallengerHighScore : Artifact, IModArtifact
    {
        public int highscore = 0;
        public bool? shouldBeRight;
        public bool isMiddle = false;
        public bool currentIsRight = false;

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
                },
                Sprite = PMod.sprites["Artifact_HighScore"].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "HighScore", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "HighScore", "description"]).Localize
            });
        }

        public override int? GetDisplayNumber(State s)
        {
            return highscore;
        }

        public override void OnCombatEnd(State state)
        {
            shouldBeRight = null;
        }

        public override void OnPlayerAttack(State state, Combat combat)
        {
            if (shouldBeRight == null)
            {
                shouldBeRight = !currentIsRight;
            }
            else if(!isMiddle)
            {
                if (currentIsRight == shouldBeRight)
                {
                    highscore++;
                    shouldBeRight = !shouldBeRight;
                }
                else
                {
                    Pulse();
                    highscore = 0;
                    shouldBeRight = null;
                }
            }
            else
            {
                highscore++;
            }
        }

        public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
        {
            isMiddle = handCount % 2 == 1 && handPosition == handCount / 2;

            if (!isMiddle)
            {
                currentIsRight = !(handPosition < handCount / 2);
            }
        }
    }
}
