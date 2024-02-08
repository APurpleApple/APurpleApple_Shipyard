using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APurpleApple.Shipyard.Artifacts.Ouranos;
using FMOD;
using HarmonyLib;
using Nickel;

namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    internal static class OuranosPatches
    {
        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPostfix]
        public static void SpawnBeamEffect(AAttack __instance, G __0, State __1, Combat __2, bool __runOriginal)
        {
            G g = __0;
            State s = __1;
            Combat c = __2;
            if (__runOriginal && !__instance.fromDroneX.HasValue)
            {
                if (__instance.targetPlayer) { return; }

                if (s.EnumerateAllArtifacts().Any((x)=> x is IOuranosCannon))
                {
                    if (__instance.damage > 0)
                    {
                        EffectSpawnerExtension.RailgunBeam(c, s.ship.parts.FindIndex((Part p) => p.key == "Ouranos_Cannon") + s.ship.x, __instance.damage, new Color("ff8866"));
                    }
                }
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPrefix]
        public static bool StoreAttackInCannon(State __1, Combat __2, AAttack __instance)
        {
            State s = __1;
            Combat c = __2;
            if (!__instance.targetPlayer)
            {
                Artifact? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactOuranosCannonV2);
                ArtifactOuranosCannonV2? cv2 = art as ArtifactOuranosCannonV2;
                if (cv2 != null && !cv2.allowAttacks)
                {
                    cv2.StoreAttack(s, c, __instance);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.GetActualDamage)),HarmonyPriority(0), HarmonyPostfix]
        public static void ReduceAttackDamage(ref int __result, State s, bool targetPlayer)
        {
            if (s.route is Combat c)
            {
                if (!targetPlayer)
                {
                    Artifact? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactOuranosCannon);
                    ArtifactOuranosCannon? cv2 = art as ArtifactOuranosCannon;
                    if (cv2 != null && !cv2.isCannonActive)
                    {
                        __result = 0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyPostfix]
        public static void DrawCannonPart(Ship __instance, G __0, Vec __1, Vec __2)
        {
            if (__instance.ai is FinaleFrienemy) return;

            IOuranosCannon? cannon = __0.state.EnumerateAllArtifacts().Find((x) => x is IOuranosCannon) as IOuranosCannon;
            if (cannon == null) return;

            int partIndex = __instance.parts.FindIndex((Part p) => p.key == "Ouranos_Cannon");
            if (partIndex < 0) return;

            Part part = __instance.parts[partIndex];

            Vec pos = __1 + __2 + new Vec((part.xLerped ?? ((double)partIndex)) * 16.0 -2, -32.0 + (__instance.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
            Vec vec4 = pos + new Vec(-1.0, -1.0 + (double)(__instance.isPlayerShip ? 6 : (-6)) * part.pulse).round();

            Spr? id3 = cannon.CannonSprite;
            double num11 = vec4.x + (id3 == PMod.sprites["Ouranos_Cannon"].Sprite ? 2 : 0);
            double y7 = vec4.y;
            bool flag = !__instance.isPlayerShip;
            bool flip7 = part.flip;
            bool flipY7 = flag;
            Color? color2 = new Color(1.0, 1.0, 1.0, 1.0);
            Draw.Sprite(id3, num11, y7, flip7, flipY7, 0.0, null, null, null, null, color2);
        }
    }
}
