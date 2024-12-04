

using APurpleApple.Shipyard.Squadron;
using Shockah.Kokoro;
using static Shockah.Kokoro.IKokoroApi.IV2.IEvadeHookApi.IHook;

namespace APurpleApple.Shipyard.ExternalAPIs
{
    internal class SquadronKokoroEvadeHook : IKokoroApi.IV2.IEvadeHookApi.IHook
    {
        public void AfterEvade(IAfterEvadeArgs args) 
        {
            if (args.State.ship.key != PMod.ships["Squadron"].UniqueName) return;
            if (MG.inst.g.hoverKey == SUK.btn_move_left || MG.inst.g.hoverKey == SUK.btn_move_right)
            {
                int j = 0;
                Ship ship = MG.inst.g.state.ship;
                for (int i = 0; i < ship.parts.Count; i++)
                {
                    if (args.State.ship.parts[i] is not PartSquadronUnit unit) continue;
                    j++;
                    if (j == MG.inst.g.hoverKey.Value.v)
                    {
                        SquadronPatches.SetLeader(args.State, unit.pilot);
                    }
                }
            }
        }

        public bool? ShouldShowEvadeButton(IShouldShowEvadeButtonArgs args)
        {
            return (args.State.ship.key != PMod.ships["Squadron"].UniqueName);
        }
    }
}
