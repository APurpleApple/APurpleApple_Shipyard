using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Shared
{
    internal static class SharedPatches
    {
        //[HarmonyPatch(typeof(StoryNode), nameof(StoryNode.Filter)), HarmonyPostfix]
        public static void FilterOutEvents(string key, State s, ref bool __result)
        {
            if (__result == false) return;
            ShipyardEntry? ship = PMod.shipyardEntries.Values.FirstOrDefault(e => e?.uniqueName == s.ship.key, null);
            if (ship == null) return;

            for (int i = 0; i < ship.DisabledEvents.Count; i++)
            {
                if (key == ship.DisabledEvents[i])
                {
                    __result = false;
                    return;
                }
            }
        }

        //[HarmonyPatch(typeof(ArtifactReward), nameof(ArtifactReward.GetBlockedArtifacts)), HarmonyPostfix]
        public static void FilterOutArtifacts(State s, ref HashSet<Type> __result)
        {
            ShipyardEntry? ship = PMod.shipyardEntries.Values.FirstOrDefault(e => e?.uniqueName == s.ship.key, null);
            if (ship == null) return;

            for (int i = 0; i < ship.DisabledArtifacts.Count; i++)
            {
                __result.Add(ship.DisabledArtifacts[i]);
            }
        }
    }
}
