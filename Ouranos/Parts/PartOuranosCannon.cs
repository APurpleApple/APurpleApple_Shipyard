using APurpleApple.Shipyard.ExternalAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Ouranos.Parts
{
    internal class PartOuranosCannon : Part, ICustomPart
    {
        public Spr? sprite = null;
        public bool IsTemporary { get; set; } = false;
        public int RenderDepth { get; set; } = -1;

        public bool DoVanillaRender(Ship ship, int localX, G g) => false;
        public void Render(Ship ship, int localX, G g, Vec v, Vec worldPos)
        {
            xLerped = Mutil.MoveTowards(xLerped ?? ((double)localX), localX, g.dt * 10);
            Vec partPos = worldPos + new Vec((xLerped ?? ((double)localX)) * 16.0, -32.0 + (ship.isPlayerShip ? offset.y : (1.0 + (0.0 - offset.y))));
            partPos += v;
            partPos += new Vec(-1.0, -1.0);

            Draw.Sprite(sprite ?? DB.parts.GetOrNull(skin ?? ""), partPos.x+8.5, partPos.y, flip, originRel: new Vec(.5,0));
        }
    }
}
