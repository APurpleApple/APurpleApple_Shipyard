using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    internal static class BgFxPatches
    {

        public static List<FX> fx = new List<FX>();

        [HarmonyPatch(typeof(Combat), nameof(Combat.UpdateFx)), HarmonyPostfix]
        public static void FXUpdatePostfix(G g)
        {
            foreach (FX item in fx)
            {
                item.Update(g);
            }

            fx.RemoveAll((FX f) => f.IsDone);
        }

        [HarmonyPatch(typeof(Combat), nameof(Combat.DrawBG)), HarmonyPostfix]
        public static void FXRenderPostfix(Combat __instance, G g)
        {
            Vec v = __instance.GetCamOffset() + Combat.arenaPos;
            foreach (FX item in fx)
            {
                item.Render(g, v);
            }
        }
    }
}
