using HarmonyLib;

namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    public static class ElectricChargesPatches
    {

        [HarmonyPatch(typeof(Card), nameof(Card.GetActualDamage)), HarmonyPostfix]
        public static void GetElectricChargeDamage(ref int __result, State s, bool targetPlayer)
        {
            if (s.route is Combat route)
            {
                Ship ship = targetPlayer ? route.otherShip : s.ship;
                __result += ship.Get(PMod.statuses["ElectricCharge"].Status);
            }
            else
            {
                __result += s.ship.Get(PMod.statuses["ElectricCharge"].Status);
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.NormalDamage)), HarmonyPostfix]
        public static void ReduceElectricCharge(Ship __instance, Combat c)
        {
            if (__instance.Get(PMod.statuses["ElectricCharge"].Status) > 0)
            {
                c.QueueImmediate(new AStatus() { status = PMod.statuses["ElectricCharge"].Status, statusAmount = -1, targetPlayer = __instance.isPlayerShip });
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPostfix]
        public static void RemoveElectricCharge(AAttack __instance, G __0, State __1, Combat __2, bool __runOriginal)
        {
            G g = __0;
            State s = __1;
            Combat c = __2;
            if (__runOriginal && !__instance.fromDroneX.HasValue)
            {
                Ship source = __instance.targetPlayer ? c.otherShip : s.ship;
                Ship other = __instance.targetPlayer ? s.ship : c.otherShip;
                int electric_charge_count = source.Get(PMod.statuses["ElectricCharge"].Status);

                if (electric_charge_count > 0)
                {
                    var key = DB.story.QuickLookup(s, ".APurpleApple.electric_charge_release");
                    if (key != null)
                    {
                        Narrative.ActivateShoutSequence(g, c, key);
                    }

                    //Vec impactLoc = other.GetShipRect().Center();
                    //PFX.screenSpaceAdd.Add(new Particle { lifetime = 3, sprite = (Spr)VoltPortrait.Id, pos = impactLoc });
                    c.QueueImmediate(new AStatus() { status = PMod.statuses["ElectricCharge"].Status, statusAmount = -electric_charge_count, targetPlayer = source.isPlayerShip });
                }
            }
        }
    }
}
