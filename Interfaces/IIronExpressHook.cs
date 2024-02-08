using APurpleApple.Shipyard.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Interfaces
{
    public interface IIronExpressHook
    {
        public void OnIronExpressRotate(Combat c, State s, PartRailCannon cannon);
        public void OnIronExpressSlide(Combat c, State s, PartRailCannon cannon);
    }
}
