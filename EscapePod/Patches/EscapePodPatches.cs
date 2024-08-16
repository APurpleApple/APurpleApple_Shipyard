using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.EscapePod
{
    [HarmonyPatch]
    internal static class EscapePodPatches
    {
        //[HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPrefix]
        public static void DamageOnEmptyPrefix(AAttack __instance, G g, State s, Combat c)
        {
            if (!__instance.targetPlayer) return;
            if (!s.EnumerateAllArtifacts().Any(a=>a is ArtifactEscapePod)) return;

            int? num = __instance.GetFromX(s, c);
            RaycastResult? raycastResult = (__instance.fromDroneX.HasValue ? CombatUtils.RaycastGlobal(c, s.ship, fromDrone: true, __instance.fromDroneX.Value) : (num.HasValue ? CombatUtils.RaycastFromShipLocal(s, c, num.Value, true) : null));

            if (raycastResult != null && !raycastResult.hitDrone)
            {
                Part? hitPart = s.ship.GetPartAtLocalX(raycastResult.worldX - s.ship.x);
                if( hitPart != null && hitPart.type == PType.empty)
                {
                    s.ship.NormalDamage(s, c, 1, raycastResult.worldX, false);
                }
            }
        }
    }
}
