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

        public override void OnCombatStart(State state, Combat combat)
        {
            if( highscore >= 5)
            {
                combat.Queue(new AStatus() { status = Status.evade, statusAmount = 2, targetPlayer = true, artifactPulse = this.Key() });
            }
            if (highscore >= 50)
            {
                combat.Queue(new AHeal() { artifactPulse = this.Key(), healAmount = 1, targetPlayer = true });
            }
            if (highscore >= 999)
            {
                combat.Queue(new AStatus() { status = Status.powerdrive, statusAmount = 999, targetPlayer = true, artifactPulse = this.Key() });
            }
        }

        public override int ModifyCardRewardCount(State state, bool isEvent, bool inCombat)
        {
            return highscore >= 10 ? 1 : 0;
        }

        public override void OnTurnStart(State state, Combat combat)
        {
            if (highscore >= 20)
            {
                combat.energy++;
                Pulse();
            }
        }

        public override int ModifyBaseDamage(int baseDamage, Card? card, State state, Combat? combat, bool fromPlayer)
        {
            int mod = 0;
            if (highscore >= 35) mod++;
            if (highscore >= 100) mod++;
            if (highscore >= 250) mod++;
            if (highscore >= 500) mod++;
            return mod;
        }

        public override void OnCombatEnd(State state)
        {
            shouldBeRight = null;
        }

        public override List<Tooltip>? GetExtraTooltips()
        {
            List<Tooltip> tooltips = new List<Tooltip>();
             
            if (highscore >= 5)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips","Highscore","5"])));
            }
            if (highscore >= 10)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "10"])));
            }
            if (highscore >= 20)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "20"])));
            }
            if (highscore >= 35)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "35"])));
            }
            if (highscore >= 50)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "50"])));
            }
            if (highscore >= 100)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "100"])));
            }
            if (highscore >= 250)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "250"])));
            }
            if (highscore >= 500)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "500"])));
            }
            if (highscore >= 999)
            {
                tooltips.Add(new TTText(PMod.Instance.Localizations.Localize(["tooltips", "Highscore", "999"])));
            }

            return tooltips;
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
