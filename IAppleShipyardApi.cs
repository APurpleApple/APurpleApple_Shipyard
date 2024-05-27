using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard;
public interface IAppleShipyardApi
{
    void RegisterActionLooksForPartType(Type actionType, PType partType);
}
