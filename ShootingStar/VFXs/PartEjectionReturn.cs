using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    public class PartEjectionReturn : FX
    {
        public Part? part;
        public int worldX;

        public override void Update(G g)
        {
            base.Update(g);
            if (age >= 1)
            {
                //age = 0.99999;
            }
        }

        public override void Render(G g, Vec v)
        {
            if (part == null) return;
            Spr? spr = DB.parts.GetOrNull(part.skin ?? part.type.Key());

            Draw.Sprite(spr, v.x + worldX + 7, v.y + Ease.InSin(1-age) * 300 + 32 + 78, originPx: new Vec(8, 32)) ;
        }
    }
}
