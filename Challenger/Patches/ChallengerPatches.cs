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
        public static bool PreventCannonFx(G g, bool targetPlayer, RaycastResult ray, DamageDone dmg)
        {
            if (targetPlayer) return true;
            Part? part = g.state.ship.GetPartAtWorldX(ray.worldX);
            if (part == null) return true;
            if (part is PartChallengerFist)
            {
                if (ray.hitShip || ray.hitDrone)
                {
                    EffectSpawner.NonCannonHit(g, targetPlayer, ray, dmg);
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
                bool hilight = false;

                int handCount = c.hand.Count;
                int handPosition = c.hand.FindIndex((x) => x == __instance);

                IEnumerable<PartChallengerFist> fists = __1.ship.parts.Where(x=>x is PartChallengerFist).Cast<PartChallengerFist>();

                foreach (PartChallengerFist fist in fists)
                {
                    if (fist.hilight)
                    {
                        hilight = true;
                        break;
                    }
                }

                if (hilight)
                {
                    foreach (PartChallengerFist fist in fists)
                    {
                        if (handCount % 2 == 1 && handPosition == handCount / 2)
                        {
                            fist.hilight = true;
                        }
                        else
                        {
                            if (handPosition < handCount / 2)
                            {
                                fist.hilight = !fist.flip;
                            }
                            else
                            {
                                fist.hilight = fist.flip;
                            }
                        }
                    }
                }
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPrefix]
        public static void AAttackBegin_Prefixes(G g, State s, Combat c, AAttack __instance)
        {
            AddAttackMovement(g, s, c, __instance);
            ChampionBelt(s, c, __instance);
        }

        public static void AddAttackMovement(G g, State s, Combat c, AAttack __instance)
        {
            if (__instance.targetPlayer) return;
            if (__instance.fromDroneX.HasValue) return;
            Part? part = g.state.ship.GetPartAtLocalX(__instance.GetFromX(s, c) ?? -1);
            if (part == null) return;
            if (part is PartChallengerFist)
            {
                __instance.moveEnemy = part.flip ? -1 : 1;
            }
        }

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
