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

        public PType? GetPartTypeForActionType(Type actionType)
        {
            PType? pType = null;
            for (int i = SquadronEntry.cardActionLooksForType.Count - 1; i >= 0; i--)
            {
                if (SquadronEntry.cardActionLooksForType[i].Item1 == actionType)
                {
                    pType = SquadronEntry.cardActionLooksForType[i].Item2;
                    break;
                }
                else if (actionType.IsSubclassOf(SquadronEntry.cardActionLooksForType[i].Item1))
                {
                    pType = SquadronEntry.cardActionLooksForType[i].Item2;
                }
            }
            return pType;
        }

        public void RegisterActionLooksForPartType(Type actionType, PType partType)
        {
            SquadronEntry.AddCardActionLooksForType(new Tuple<Type, PType>(actionType, partType));
        }
    }
}
