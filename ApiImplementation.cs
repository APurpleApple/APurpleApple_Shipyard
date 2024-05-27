using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard
{
    public sealed class ApiImplementation : IAppleShipyardApi
    {
        public void RegisterActionLooksForPartType(Type actionType, PType partType)
        {
            PMod.cardActionLooksForType.Add(new Tuple<Type, PType>(actionType, partType));
        }
    }
}
