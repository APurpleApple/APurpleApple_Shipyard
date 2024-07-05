using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    public class PartEjection : FX
    {
        public Part? part;
        public int worldX;
        public bool spins = true; 

        public override void Render(G g, Vec v)
        {
            if (part == null) return;
            Spr? spr = DB.parts.GetOrNull(part.skin ?? part.type.Key());

            Draw.Sprite(spr, v.x + worldX + 8, v.y - age * 300 + 32 + 60, originPx: new Vec(8, 32), rotation: spins ? Ease.InSin(age) * 20 : Ease.InSin(age) * 4) ;
        }
    }
}
