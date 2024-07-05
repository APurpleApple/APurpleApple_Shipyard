using APurpleApple.Shipyard.Squadron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard
{
    public sealed class ApiImplementation : IAppleShipyardApi
    {
        public void DisableArtifactForShip(string ship, Type artifactType)
        {
            PMod.shipyardEntries[ship].DisabledArtifacts.Add(artifactType);
        }

        public void DisableEventForShip(string ship, string eventKey)
        {
            PMod.shipyardEntries[ship].DisabledEvents.Add(eventKey);
        }

        public void RegisterActionLooksForPartType(Type actionType, PType partType)
        {
            SquadronEntry.cardActionLooksForType.Add(new Tuple<Type, PType>(actionType, partType));
        }
    }
}
