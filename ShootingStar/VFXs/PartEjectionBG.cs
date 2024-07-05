using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    public class PartEjectionBG : FX
    {
        public Part? part;
        public int worldX;

        public override void Update(G g)
        {
            if (age == 0.0)
            {
                Start(g);
            }

            age += g.dt * .25;
        }

        public override void Render(G g, Vec v)
        {
            if (part == null) return;
            Spr? spr = DB.parts.GetOrNull(part.skin ?? part.type.Key());
            double scale = (1-Math.Sin(age * Math.PI)) *.4 + .2;
            Draw.Sprite(spr, v.x + worldX + 8, v.y + age * (G.screenSize.y+150) - 100, originPx: new Vec(8, 32), rotation: age * 20, scale: new Vec(scale,scale)) ;
        }
    }
}
