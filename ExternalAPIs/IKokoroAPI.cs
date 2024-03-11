using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ExternalAPIs
{
    public interface IKokoroApi
    {
        bool IsEvadePossible(State state, Combat combat, int direction, EvadeHookContext context);
    }

    public enum EvadeHookContext
    {
        Rendering, Action
    }
}
