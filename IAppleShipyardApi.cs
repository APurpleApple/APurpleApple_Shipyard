using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard;
public interface IAppleShipyardApi
{
    // "squadron"
    // "challenger"
    // "shootingStar"
    // "escapePod"
    // "ouranos"
    // "ironExpress"
    void RegisterActionLooksForPartType(Type actionType, PType partType);
    void DisableArtifactForShip(string ship, Type artifactType);
    void DisableEventForShip(string ship, string eventKey);
}
