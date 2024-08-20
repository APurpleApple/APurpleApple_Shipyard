using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APurpleApple.Shipyard.ExternalAPIs;
using HarmonyLib;

namespace APurpleApple.Shipyard.Challenger
{
    public class PartChallengerFist : Part, ICustomPart
    {
        public int xTarget = 0;
        public int yTarget = 0;

        public bool IsTemporary { get; set; } = false;
        public bool DoVanillaRender(Ship ship, int localX, G g) => !active;

        public void Render(Ship ship, int localX, G g, Vec v, Vec worldPos)
        {
            if (!active) return;
            Vec partPos = worldPos + new Vec((xLerped ?? ((double)localX)) * 16.0, -32.0 + (ship.isPlayerShip ? offset.y : (1.0 + (0.0 - offset.y))));
            partPos += v;
            partPos += new Vec(-1.0, -1.0);

            double x = partPos.x;
            double y = partPos.y;
            bool flipX = flip;
            bool flipY = !ship.isPlayerShip;
            Color? color = new Color(1.0, 1.0, 1.0, 1.0);

            double u = double.Lerp(y + 27, yTarget + 30, pulse);
            double d = y + (double)(ship.isPlayerShip ? 6 : (-6)) * pulse + 24;
            double distance = Math.Abs(u - d);
            double curveOffset = GetCurveOffset(pulse, flipX);

            double fistX = double.Lerp(x, xTarget * 16.0 + v.x, pulse);

            double segments = 4;
            double segmentsLength = pulse / segments;
            for (int j = 0; j < segments; j++)
            {
                Vec segmentStart = new Vec(double.Lerp(x, xTarget * 16.0 + v.x, segmentsLength * j) + GetCurveOffset(segmentsLength * j, flipX), d - distance * j / segments);
                Vec segmentEnd = new Vec(double.Lerp(x, xTarget * 16.0 + v.x, segmentsLength * (j + 1)) + GetCurveOffset(segmentsLength * (j + 1), flipX), d - distance * (j + 1) / segments);
                Vec diff = (segmentEnd - segmentStart).normalized();
                double dist = (segmentEnd - segmentStart).len();
                double angle = Math.Atan2(-diff.y, -diff.x) - Math.PI * .5;
                Draw.Sprite(PMod.sprites[PSpr.Parts_fist_chain_segment].Sprite, segmentStart.x + 8.5, segmentStart.y, flipX, flipY, angle, new Vec(4.5, 7), null, new Vec(1, dist / 7), null, color);
            }
            Vec fistVec = new Vec(GetCurveOffset(pulse, flipX) - GetCurveOffset(pulse - 0.01, flipX), distance * pulse - distance * (pulse - 0.01)).normalized();
            double fistAngle = pulse == 0 ? 0 : Math.Clamp(Math.Atan2(fistVec.y, -fistVec.x), -.2, .2) * (flipX ? -1 : 1);
            Draw.Sprite(PMod.sprites[PSpr.Parts_fist_fist].Sprite, fistX + curveOffset + 8.5, u, flipX, flipY, fistAngle, new Vec(8.5, 27), null, null, null, color);
            Draw.Sprite(PMod.sprites[PSpr.Parts_fist_cannon].Sprite, x, d - 24, flipX, flipY, 0, null, null, null, null, color);
        }

        private static double GetCurveOffset(double pulse, bool flip)
        {
            return (flip ? 10 : -10) * Math.Sin(pulse * Math.PI * 1.3);
        }
    }
}
