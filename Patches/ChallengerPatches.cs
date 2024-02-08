using APurpleApple.Shipyard.Artifacts.Challenger;
using APurpleApple.Shipyard.Artifacts.Ouranos;
using APurpleApple.Shipyard.Parts;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    internal static class ChallengerPatches
    {
        [HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyPostfix]
        public static void DrawStuff(Ship __instance, G __0, Vec __1, Vec __2)
        {
            Vec worldPos = __2;
            Vec v = __1;
            G g = __0;
            if (__instance.key == PMod.ships["Challenger"].UniqueName)
            {
                for (int i = 0; i < __instance.parts.Count; i++)
                {
                    Part part = __instance.parts[i];

                    if (part is not PartChallengerFist fist) continue;

                    Vec partPos = worldPos + new Vec((part.xLerped ?? ((double)i)) * 16.0, -32.0 + (__instance.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
                    partPos += v;
                    partPos += new Vec(-1.0, -1.0);

                    double x = partPos.x;
                    double y = partPos.y;
                    bool flipX = part.flip;
                    bool flipY = !__instance.isPlayerShip;
                    Color? color = new Color(1.0, 1.0, 1.0, 1.0);

                    double u = y + (double)(__instance.isPlayerShip ? -70 : (70)) * part.pulse + 27;
                    double d = y + (double)(__instance.isPlayerShip ? 6 : (-6)) * part.pulse + 24;
                    double distance = Math.Abs(u - d);
                    double curveOffset = GetCurveOffset(part.pulse, flipX);

                    double fistX = double.Lerp(x, fist.xTarget * 16.0 + v.x, fist.pulse);

                    //Draw.Sprite(PMod.sprites["Fist_Chain"].Sprite, num11 + 4 + curveOffset, u + 27, flip7, flipY7, 0, null, null, new Vec(1,distance/19), null, color2);

                    double segments = 4;
                    double segmentsLength = part.pulse / segments;
                    for (int j = 0; j < segments; j++)
                    {
                        Vec segmentStart = new Vec(double.Lerp(x, fist.xTarget * 16.0 + v.x, segmentsLength * j) + GetCurveOffset(segmentsLength * j, flipX), d - distance * j / segments);
                        Vec segmentEnd = new Vec(double.Lerp(x, fist.xTarget * 16.0 + v.x, segmentsLength * (j + 1)) + GetCurveOffset(segmentsLength * (j+1), flipX), d - distance * (j+1) / segments);
                        Vec diff = (segmentEnd - segmentStart).normalized();
                        double dist = (segmentEnd - segmentStart).len();
                        double angle = Math.Atan2(-diff.y, -diff.x) - Math.PI * .5;
                        Draw.Sprite(PMod.sprites["Fist_Chain_Segment"].Sprite, segmentStart.x + 8.5, segmentStart.y, flipX, flipY, angle, new Vec(4.5,7), null, new Vec(1, dist / 7), null, color);
                    }
                    Vec fistVec = new Vec(GetCurveOffset(part.pulse, flipX) - GetCurveOffset(part.pulse - 0.01, flipX), distance * part.pulse - distance * (part.pulse - 0.01)).normalized();
                    double fistAngle = part.pulse == 0 ? 0 : Math.Clamp(Math.Atan2(fistVec.y, -fistVec.x), -.2,.2) * (flipX ? -1:1);
                    Draw.Sprite(PMod.sprites["Fist_Fist"].Sprite, fistX + curveOffset + 8.5, u, flipX, flipY, fistAngle, new Vec(8.5,27), null, null, null, color);
                    Draw.Sprite(PMod.sprites["Fist_Cannon"].Sprite, x, d -24, flipX, flipY, 0, null, null, null, null, color);
                }
            }
        }
        
        private static double GetCurveOffset(double pulse, bool flip)
        {
            return (flip ? 10 : -10) * Math.Sin(pulse * Math.PI * 1.3);
        }

        [HarmonyPatch(typeof(EffectSpawner), nameof(EffectSpawner.Cannon)), HarmonyPrefix]
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

        [HarmonyPatch(typeof(Card), nameof(Card.GetAllTooltips)), HarmonyPostfix]
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
                        break;
                    }
                }
            }
        }
    }
}
