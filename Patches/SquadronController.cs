using APurpleApple.Shipyard.Parts;
using FSPRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Patches
{
    internal class SquadronController : OnMouseDown
    {
        public void OnMouseDown(G g, Box b)
        {
            if (g.state.route is not Combat c) return;
            if (!b.key.HasValue) return;

            if (!c.PlayerCanAct(g.state))
            {
                return;
            }

            bool flag = FeatureFlags.Debug && Input.shift;
            if (!(g.state.ship.Get(SStatus.evade) > 0 || flag))
            {
                return;
            }

            if (!flag && g.state.route is Combat combat && combat.hand.OfType<TrashAnchor>().Any())
            {
                Audio.Play(Event.Status_PowerDown);
                g.state.ship.shake += 1.0;
                return;
            }

            int j = 0;
            Ship ship = g.state.ship;
            for (int i = 0; i < ship.parts.Count; i++)
            {
                if (g.state.ship.parts[i] is not PartSquadronUnit unit) continue;
                j++;
                unit.hasCrown = j == b.key.Value.v;
            }

            if (b.key == SUK.btn_move_left)
            {
                c.DoEvade(g, -1);
            }
            if (b.key == SUK.btn_move_right)
            {
                c.DoEvade(g, +1);
            }
        }
    }
}
