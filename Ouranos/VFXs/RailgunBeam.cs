using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Ouranos
{
    internal class RailgunBeam : FX
    {
        public Rect rect;

        public double width = 2;

        public Color cannonBeam = new Color("ff8866");

        public static Color cannonBeamCore = new Color("ffffff");

        public override void Render(G g, Vec v)
        {
            double num = 0.25;
            if (age < num)
            {
                double percent = width * (1.0 - age / num);
                Draw.Rect(v.x + rect.x - (percent + 1.0), v.y + rect.y, rect.w + 2.0 * (percent + 1.0), rect.h, cannonBeam, BlendMode.Screen);
                Draw.Rect(v.x + rect.x - percent, v.y + rect.y, rect.w + percent * 2.0, rect.h, cannonBeamCore);
            }
        }
    }
}
