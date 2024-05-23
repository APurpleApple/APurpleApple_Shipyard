using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using APurpleApple.Shipyard.Artifacts;
using APurpleApple.Shipyard.Cards;
using HarmonyLib;
namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    internal static class AsteroidPatches
    {
        [HarmonyPatch(typeof(State), nameof(State.PlayerShipCanMove)), HarmonyPostfix]
        public static void StopMovementPostfix(State __instance, ref bool __result)
        {
            if (!CanMove(__instance))
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(Combat), nameof(Combat.RenderMoveButtons)), HarmonyPrefix]
        public static bool RenderMoveButtonPrefix(G g)
        {
            return CanMove(g.state);
        }

        [HarmonyPatch(typeof(Combat), nameof(Combat.DoEvade)), HarmonyPrefix]
        public static bool DoEvadePrefix(G g)
        {
            return CanMove(g.state);
        }

        [HarmonyPatch(typeof(AMove), nameof(AMove.Begin)), HarmonyPrefix]
        public static bool AMoveBeginPrefix(G g)
        {
            return CanMove(g.state);
        }

        [HarmonyPatch(typeof(AStatus), nameof(AStatus.Begin)), HarmonyPrefix]
        public static void StopShieldPrefix(AStatus __instance, G __0, State __1, Combat __2)
        {
            if (__instance.status == SStatus.shield || __instance.status == SStatus.tempShield && __instance.targetPlayer == true)
            {
                if (!CanShield(__1))
                {
                    __instance.statusAmount = 0;
                }
            }
        }

        [HarmonyPatch(typeof(Card), nameof(Card.RenderAction)), HarmonyPrefix]
        public static void DisableShieldActionsPostfix(State state, CardAction action)
        {
            if (action is AStatus a && a.targetPlayer == true)
            {
                if (a.status == SStatus.shield || a.status == SStatus.tempShield)
                {
                    if (!CanShield(state))
                    {
                        a.disabled = true;
                    }
                }
            }
            else if (action is AMove move)
            {
                if (!CanMove(state))
                {
                    move.disabled = true;
                }
            }
        }

        [HarmonyPatch(typeof(InitialBooster), nameof(InitialBooster.ModifyBaseDamage)), HarmonyPostfix]
        public static void InitialBoosterPostfix(ref int __result, int baseDamage, Card? card, State state, Combat? combat, bool fromPlayer)
        {
            if (card is CardAsteroidShot)
            {
                __result = 1;
            }
        }

        public static bool CanShield(State s)
        {
            if (s.artifacts.Any((x) => x is ArtifactAsteroid))
            {
                return s.ship.parts.Any((x) => x.key == "AsteroidComms");
            }
            return true;
        }

        public static bool CanMove(State s)
        {
            if (s.artifacts.Any((x) => x is ArtifactAsteroid))
            {
                return s.ship.parts.Any((x) => x.key == "AsteroidEngine");
            }
            return true;
        }
    }
}
