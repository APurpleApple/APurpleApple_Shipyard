using APurpleApple.Shipyard.Parts;
using APurpleApple.Shipyard.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ExternalAPIs
{
    internal class SquadronKokoroEvadeHook : IEvadeHook
    {
        public bool? IsEvadePossible(State state, Combat combat, EvadeHookContext context){

            if (context == EvadeHookContext.Action)
            {
                if (state.ship.key != PMod.ships["Squadron"].UniqueName) return null;
                if (MG.inst.g.hoverKey == SUK.btn_move_left || MG.inst.g.hoverKey == SUK.btn_move_right)
                {
                    int j = 0;
                    Ship ship = MG.inst.g.state.ship;
                    for (int i = 0; i < ship.parts.Count; i++)
                    {
                        if (state.ship.parts[i] is not PartSquadronUnit unit) continue;
                        j++;
                        if (j == MG.inst.g.hoverKey.Value.v)
                        {
                            SquadronPatches.SetLeader(state, unit.pilot);
                        }
                    }
                }
            }
            return null;
        }
    }
}
