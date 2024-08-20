using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APurpleApple.Shipyard.Ouranos.Parts;
using FMOD;
using FMOD.Studio;
using HarmonyLib;
using Nickel;

namespace APurpleApple.Shipyard.Ouranos
{
    [HarmonyPatch]
    internal static class OuranosPatches
    {
        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPostfix]
        public static void SpawnBeamEffect(G g, State s, Combat c, AAttack __instance)
        {
            EnableCannonAfterAttack(s, c, __instance);
        }

        public static void EnableCannonAfterAttack(State s, Combat c, AAttack __instance)
        {
            ArtifactOuranosCannonV2? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactOuranosCannonV2) as ArtifactOuranosCannonV2;
            if (art == null) return;
            PartOuranosCannon? cannon = ArtifactOuranosCannon.GetCannon(s);
            if (cannon != null)
            {
                //cannon.type = PType.cannon;
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPrefix]
        public static void DisableCannonAndStoreAttack(State s, Combat c, AAttack __instance)
        {
            if (__instance.targetPlayer) return;

            ArtifactOuranosCannonV2? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactOuranosCannonV2) as ArtifactOuranosCannonV2;
            if (art == null) return;
            PartOuranosCannon? cannon = ArtifactOuranosCannon.GetCannon(s);
            if (cannon != null)
            {
                //cannon.type = PType.special;
            }

            if (__instance.fromDroneX.HasValue) return;
            if (__instance.multiCannonVolley) return;

            if (__instance is ABeamAttack) return;
            art.StoreAttack(s, c, __instance);
        }

        //[HarmonyPatch(typeof(Card), nameof(Card.GetActualDamage)),HarmonyPriority(0), HarmonyPostfix]
        public static void ReduceAttackDamage(ref int __result, State s, bool targetPlayer)
        {
            if (s.route is Combat c)
            {
                if (!targetPlayer)
                {
                    ArtifactOuranosCannon? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactOuranosCannon) as ArtifactOuranosCannon;
                    if (art != null && !art.isCannonActive)
                    {
                        __result = 0;
                    }
                }
            }
        }
    }
}
