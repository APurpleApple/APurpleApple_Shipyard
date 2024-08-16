using APurpleApple.Shipyard.ExternalAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HarmonyLib.Code;

namespace APurpleApple.Shipyard.Squadron
{
    public class PartSquadronUnit : Part, ICustomPart
    {
        public Deck? pilot = null;
        public double yLerped = 0;
        public bool IsTemporary { get; set; } = false;
        public void Render(Ship ship, int localX, G g, Vec v, Vec worldPos)
        {
            xLerped = Mutil.MoveTowards(xLerped ?? ((double)localX), localX, g.dt * 10);

            int j = ship.parts.Where(p => p is PartSquadronUnit).TakeWhile(p => p != this).Count();

            if (g.state.IsOutsideRun())
            {
                Deck[] pilots = g.state.runConfig.selectedChars.ToArray();
                pilot = pilots.Length > j ? pilots[j] : null;
            }

            double yOffset = j % 2 == 0 ? 9 : 0;

            bool isMissing = false;

            if (pilot.HasValue)
            {
                Status missingStatus;
                if (StatusMeta.deckToMissingStatus.TryGetValue(pilot.Value, out missingStatus))
                {
                    if (ship.Get(missingStatus) > 0)
                    {
                        isMissing = true;
                    }
                }
            }

            yLerped = Mutil.MoveTowards(yLerped, yOffset, g.dt * Math.Abs(yLerped - yOffset) * 5);
            xLerped = Mutil.MoveTowards(xLerped ?? ((double)localX), localX, g.dt * Math.Max(Math.Abs((xLerped ?? (double)localX) - localX) - 1, 0) * 10);
            j++;

            Vec partPos = worldPos + new Vec((xLerped ?? ((double)localX)) * 16.0, yLerped + -32.0 + (ship.isPlayerShip ? offset.y : (1.0 + (0.0 - offset.y))));
            partPos += v;
            partPos += new Vec(-5.0, 3.0);

            Draw.Sprite(PMod.sprites[isMissing ? PSpr.Parts_squadron_fighter_broken : PSpr.Parts_squadron_fighter].Sprite, partPos.x, partPos.y, color: isMissing ? new Color(.75, .75, .75, 1) : Colors.white);

            if (pilot.HasValue)
            {
                Draw.Sprite(PMod.sprites[PSpr.Parts_squadron_color_decal].Sprite, partPos.x, partPos.y, color: DB.decks[pilot.Value].color);
            }
        }

        public bool DoVanillaRender(Ship ship, int localX, G g) => false;

        public void RenderUI(Ship ship, G g, Combat? combat, int localX, string keyPrefix, bool isPreview, Vec v) 
        {
            if (pilot.HasValue)
            {
                Vec partPos = v;
                partPos += new Vec(-5.0, 1.0);

                if (g.state.route is Combat c)
                {
                    Draw.Text(Character.GetDisplayName(pilot.Value, g.state), partPos.x + 13, partPos.y + 10, color: DB.decks[pilot.Value].color, outline: Colors.black, align: daisyowl.text.TAlign.Center);
                }

                if (pilot == SquadronPatches.GetLeader(g.state))
                {
                    Draw.Sprite(PMod.sprites[PSpr.Icons_crown].Sprite, partPos.x, partPos.y, color: Colors.white.fadeAlpha(Math.Abs(Math.Sin(g.time * 2))));
                }
            }
            
        }
    }
}
