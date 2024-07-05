using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.IronExpress
{
    public class PartRailCannon : Part
    {
        public Part? overlapedPart = null;
        public double rotLerped = 0.0;
        public bool isCannon = true;
    }
}
