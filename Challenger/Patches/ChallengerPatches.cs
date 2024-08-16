using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Challenger
{
    [HarmonyPatch]
    internal static class ChallengerPatches
    {
        //[HarmonyPatch(typeof(EffectSpawner), nameof(EffectSpawner.Cannon)), HarmonyPrefix]
        public static bool PreventCannonFx(G __0, bool __1, RaycastResult __2, DamageDone __3)
        {
            if (!__1 && __0.state.ship.key == PMod.ships["Challenger"].UniqueName)
            {
                if (__2.hitShip || __2.hitDrone)
                {
                    EffectSpawner.NonCannonHit(__0, __1, __2, __3);
                }
                return false;
            }
            return true;
        }

        //[HarmonyPatch(typeof(Card), nameof(Card.GetAllTooltips)), HarmonyPostfix]
        public static void RemovePartHighlight(Card __instance, G __0, State __1)
        {
            if (__1.route is Combat c)
            {
                int handCount = c.hand.Count;
                int handPosition = c.hand.FindIndex((x) => x == __instance);
                foreach (Part p in __1.ship.parts)
                {
                    if (p.key == "ChallengerFist")
                    {
                        if (handCount % 2 == 1 && handPosition == handCount / 2)
                        {
                        }
                        else
                        {
                            if (handPosition < handCount / 2)
                            {
                                p.hilight = p.hilight & !p.flip;
                            }
                            else
                            {
                                p.hilight = p.hilight & p.flip;
                            }
                        }
                    }
                }
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPrefix]
        public static void ChampionBelt(State __1, Combat __2, AAttack __instance)
        {
            if (__1.ship.key != PMod.ships["Challenger"].UniqueName) return;
            if (__instance.targetPlayer) return;
            if (__instance.fromDroneX.HasValue) return;
            int? x = __instance.GetFromX(__1, __2);
            if (!x.HasValue) return;
            Part? cannon = __1.ship.GetPartAtLocalX(x.Value);
            if (cannon == null) return;
            if (cannon is not PartChallengerFist fist) return;

            RaycastResult raycastResult = CombatUtils.RaycastFromShipLocal(__1, __2, x.Value, __instance.targetPlayer);
            fist.xTarget = raycastResult.worldX;
            fist.yTarget = (int)(raycastResult.hitDrone ? FxPositions.Drone(0).y : (raycastResult.hitShip ? FxPositions.Hull(0, false).y : 0));

            if (!__1.EnumerateAllArtifacts().Any((x) => x is ArtifactChallengerChampion)) return;

            if (!raycastResult.hitShip && !raycastResult.hitDrone)
            {
                for (int i = -1; i <= 1; i += 2) 
                {
                    if (CombatUtils.RaycastGlobal(__2, __2.otherShip, fromDrone: true, raycastResult.worldX + i).hitShip)
                    {
                        __instance.fromX = x + i;
                        fist.pulse = 1.0;
                        fist.xTarget = raycastResult.worldX + i;
                        fist.yTarget = (int)FxPositions.Hull(0, false).y;
                        break;
                    }
                }
            }
        }
    }
}
