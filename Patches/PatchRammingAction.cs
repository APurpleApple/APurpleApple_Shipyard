using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using APurpleApple.Shipyard.VFXs;
namespace APurpleApple.Shipyard.HarmonyPatches
{
    [HarmonyPatch]
    public static class PatchRammingAction
    {
        [HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyPrefix]
        public static void DrawShipOverPrefix(G g, ref Vec worldPos, Ship __instance)
        {
            if (__instance.isPlayerShip)
            {
                if (g.state.route is Combat c)
                {
                    foreach(FX fx in c.fx)
                    {
                        if (fx is ShipRamm shipRamm)
                        {
                            if (shipRamm.age < .2)
                            {
                                double percent = Ease.InElastic(shipRamm.age) / .2;
                                worldPos.y -= 70 * percent;
                            }
                            else if (shipRamm.age < .6)
                            {
                                double percent = Ease.OutSin(1 - ((shipRamm.age - .2) / .4));
                                worldPos.y -= 70 * percent;
                            }
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.DrawBottomLayer)), HarmonyPrefix]
        public static void DrawShipUnderPrefix(G g, ref Vec worldPos, Ship __instance)
        {
            if (__instance.isPlayerShip)
            {
                if (g.state.route is Combat c)
                {
                    foreach (FX fx in c.fx)
                    {
                        if (fx is ShipRamm shipRamm)
                        {
                            if (shipRamm.age < .2)
                            {
                                double percent = Ease.InElastic(shipRamm.age) / .2;
                                worldPos.y -= 70 * percent;
                            }
                            else if (shipRamm.age < .6)
                            {
                                double percent = Ease.OutSin(1 - ((shipRamm.age - .2) / .4));
                                worldPos.y -= 70 * percent;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
