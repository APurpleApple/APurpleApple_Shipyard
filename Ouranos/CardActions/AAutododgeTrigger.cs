using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Ouranos
{
    internal class AAutododgeTrigger : AAttack
    {
        public override void Begin(G g, State s, Combat c)
        {
            Ship ship = (targetPlayer ? s.ship : c.otherShip);
            Ship ship2 = (targetPlayer ? c.otherShip : s.ship);
            if (ship == null || ship2 == null || ship.hull <= 0 || (fromDroneX.HasValue && !c.stuff.ContainsKey(fromDroneX.Value)))
            {
                return;
            }

            int? num = GetFromX(s, c);
            RaycastResult? raycastResult = (fromDroneX.HasValue ? CombatUtils.RaycastGlobal(c, ship, fromDrone: true, fromDroneX.Value) : (num.HasValue ? CombatUtils.RaycastFromShipLocal(s, c, num.Value, targetPlayer) : null));
            if (raycastResult != null) ApplyAutododge(c, ship, raycastResult);
        }
    }
}
