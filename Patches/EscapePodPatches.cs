using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    internal static class EscapePodPatches
    {
        
        [HarmonyPatch(typeof(ArtifactReward), nameof(ArtifactReward.GetBlockedArtifacts)), HarmonyPostfix]
        public static void FilterOutArtifacts(State s, ref HashSet<Type> __result)
        {
            if (s.ship.key != PMod.ships["EscapePod"].UniqueName) return;
            __result.Add(typeof(TridimensionalCockpit));
        }
    }
}
