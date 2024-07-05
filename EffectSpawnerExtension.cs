using APurpleApple.Shipyard.Ouranos;
using FMOD;
using FSPRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard
{
    internal class EffectSpawnerExtension
    {
        public static void RailgunBeam(Combat combat, int x, int width, Color beamColor)
        {
            width = Math.Min(15, width);
            Rect rect = Rect.FromPoints(FxPositions.Cannon(x, true), FxPositions.Miss(x, false));
            combat.fx.Add(new RailgunBeam
            {
                cannonBeam = beamColor,
                width = width,
                rect = rect
            });
            Audio.Play("");
        }
    }
}
