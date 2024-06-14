using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard
{
    public static class GExt
    {
        public static Rect Peek(this G g, Rect? rect = null)
        {
            Box? box = ((g.uiStack.Count > 0) ? g.uiStack.Peek() : null);
            return rect.GetValueOrDefault() + box?.rect ?? default(Rect);
        }
    }
}
