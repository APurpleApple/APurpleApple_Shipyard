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

        public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
        {
            if (!(handCount % 2 == 1 && handPosition == handCount / 2))
            {
                bool currentIsRight = !(handPosition < handCount / 2);

                if (!shouldBeRight.HasValue)
                {
                    shouldBeRight = currentIsRight;
                }

                if (currentIsRight == shouldBeRight)
                {
                    shouldBeRight = !shouldBeRight;
                    highscore++;
                    this.Pulse();
                }
                else
                {
                    shouldBeRight = null;
                    highscore = 0;
                    Pulse();
                }
            }
        }


        public override Spr GetSprite()
        {
            return shouldBeRight.HasValue ? shouldBeRight.Value ? PMod.sprites["Artifact_HighScoreRight"].Sprite : PMod.sprites["Artifact_HighScoreLeft"].Sprite : PMod.sprites["Artifact_HighScore"].Sprite;
        }
    }
}
