﻿using APurpleApple.Shipyard.Artifacts.Ouranos;
using APurpleApple.Shipyard.Artifacts.Squadron;
using APurpleApple.Shipyard.Parts;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Patches
{
    [HarmonyPatch]
    internal static class SquadronPatches
    {
        public static SquadronController controller = new SquadronController();

        [HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyPostfix]
        public static void DrawStuff(Ship __instance, G __0, Vec __1, Vec __2)
        {
            Vec worldPos = __2;
            Vec v = __1;
            G g = __0;

            if (__instance.key == PMod.ships["Squadron"].UniqueName)
            {
                int j = 0;
                if (g.state.IsOutsideRun())
                {
                    Deck[] pilots = g.state.runConfig.selectedChars.ToArray();

                    for (int i = 0; i < __instance.parts.Count; i++)
                    {
                        Part part = __instance.parts[i];
                        if (part is not PartSquadronUnit unit) continue;
                        unit.pilot = pilots.Length > j ? pilots[j] : null;
                        j++;
                    }
                }

                j = 0;
                for (int i = 0; i < __instance.parts.Count; i++)
                {
                    Part part = __instance.parts[i];

                    if (part is not PartSquadronUnit unit) continue;
                    double yOffset = j % 2 == 0 ? 9 : 0;

                    bool isMissing = false;

                    if (unit.pilot.HasValue)
                    {

                        Status missingStatus;
                        if (StatusMeta.deckToMissingStatus.TryGetValue(unit.pilot.Value, out missingStatus))
                        {
                            if (__instance.Get(missingStatus) > 0)
                            {
                                isMissing = true;
                            } 
                        }
                    }

                    unit.yLerped = Mutil.MoveTowards(unit.yLerped, yOffset, g.dt * Math.Abs(unit.yLerped - yOffset) * 5);
                    part.xLerped = Mutil.MoveTowards(part.xLerped ?? ((double)i), i, g.dt * Math.Max(Math.Abs((part.xLerped ?? (double)i) - i) -1, 0) * 10);
                    j++;

                    Vec partPos = worldPos + new Vec((part.xLerped ?? ((double)i)) * 16.0,unit.yLerped + -32.0 + (__instance.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
                    partPos += v;
                    partPos += new Vec(-5.0, 3.0);

                    Draw.Sprite(PMod.sprites[isMissing ? "Squadron_Unit_Broken" : "Squadron_Unit"].Sprite, partPos.x, partPos.y, color: isMissing ? new Color(.75,.75,.75,1) : Colors.white);

                    if (unit.pilot.HasValue)
                    {
                        Draw.Sprite(PMod.sprites["Squadron_Color_Decal"].Sprite, partPos.x, partPos.y, color: DB.decks[unit.pilot.Value].color);
                        if (g.state.route is Combat c)
                        {
                            Draw.Text(Character.GetDisplayName(unit.pilot.Value, g.state), partPos.x + 13, partPos.y + 10, color: DB.decks[unit.pilot.Value].color, outline: Colors.black, align: daisyowl.text.TAlign.Center);
                                
                        }
                    }

                    if (unit.hasCrown)
                    {
                        Draw.Sprite(PMod.sprites["Squadron_Crown"].Sprite, partPos.x, partPos.y, color: Colors.white.fadeAlpha(Math.Abs(Math.Sin(g.time * 2))));
                    }
                }
            }
        }

        

        [HarmonyPatch(typeof(State), nameof(State.PopulateRun)), HarmonyPostfix]
        public static void SetPilots(State __instance, StarterShip shipTemplate)
        {
            if (__instance.ship.key == PMod.ships["Squadron"].UniqueName)
            {
                foreach (Deck deck in __instance.runConfig.selectedChars)
                {
                    foreach (Part part in __instance.ship.parts)
                    {
                        if (part is PartSquadronUnit squadronUnit)
                        {
                            if (!squadronUnit.pilot.HasValue)
                            {
                                squadronUnit.pilot = deck;
                                break;
                            }
                        }
                    }
                }

                ArtifactSquadron? art = __instance.artifacts.Find((x) => x is ArtifactSquadron) as ArtifactSquadron;
                if (art != null)
                {
                    foreach (Part part in __instance.ship.parts)
                    {
                        if (part is PartSquadronUnit squadronUnit && squadronUnit.hasCrown)
                        {
                            art.activePilot = squadronUnit.pilot;
                            break;
                        }
                    }
                }
             }
        }

        [HarmonyPatch(typeof(Combat), nameof(Combat.RenderMoveButtons)), HarmonyPrefix]
        public static bool HideMoveButtons(G g)
        {
            return g.state.ship.key != PMod.ships["Squadron"].UniqueName;
        }

        [HarmonyPatch(typeof(Combat), nameof(Combat.RenderMoveButtons)), HarmonyPostfix]
        public static void RenderMoveButtons(Combat __instance, G g)
        {
            if (g.state.ship.key != PMod.ships["Squadron"].UniqueName) return;
            Combat c = __instance;

            if (!c.isPlayerTurn || g.state.ship.hull <= 0 || c.otherShip.hull <= 0 || g.state.ship.Get(SStatus.evade) <= 0)
            {
                return;
            }

            if (g.state.route is Combat combat)
            {
                foreach (Card item in combat.hand)
                {
                    if (item is TrashAnchor)
                    {
                        return;
                    }
                }
            }

            int j = 0;
            for (int i = 0; i < g.state.ship.parts.Count; i++)
            {
                Part part = g.state.ship.parts[i];
                if (part is not PartSquadronUnit unit) continue;

                double yOffset = j % 2 == 0 ? 0 : -9;
                j++;

                if (unit.pilot.HasValue)
                {
                    Status missingStatus;
                    if (StatusMeta.deckToMissingStatus.TryGetValue(unit.pilot.Value, out missingStatus))
                    {
                        if (g.state.ship.Get(missingStatus) > 0)
                        {
                            continue;
                        }
                    }
                }
                double x = (Combat.arenaPos + c.GetCamOffset() + g.state.ship.GetWorldPos(g.state, c)).x + ((part.xLerped ?? ((double)i)) * 16.0);

                if (i != 0 || g.state.ship.parts.Count < 20)
                {
                    UIKey uIKey = new UIKey(SUK.btn_move_left, j);
                    Rect rect = new Rect(x - 6, 111.0 + yOffset, 8.0, 11.0);
                    UIKey key = uIKey;
                    OnMouseDown onMouseDown = controller;
                    bool showAsPressed = !c.eyeballPeek && Input.GetGpHeld(Btn.TriggerL);
                    SharedArt.ButtonResult buttonResult = SharedArt.ButtonSprite(g, rect, key, PMod.sprites["Squadron_MoveButton"].Sprite, PMod.sprites["Squadron_MoveButtonOn"].Sprite, null, null, inactive: false, flipX: true, flipY: false, onMouseDown, autoFocus: false, noHover: false, showAsPressed, gamepadUntargetable: true);
                    if (buttonResult.isHover)
                    {
                        c.isHoveringMove = 2;
                    }
                }

                if (i != 19)
                {
                    UIKey uIKey = new UIKey(SUK.btn_move_right, j);
                    Rect rect = new Rect(x + 13, 111.0 + yOffset, 8.0, 11.0);
                    UIKey key = uIKey;
                    OnMouseDown onMouseDown = controller;
                    bool showAsPressed = !c.eyeballPeek && Input.GetGpHeld(Btn.TriggerR);
                    SharedArt.ButtonResult buttonResult2 = SharedArt.ButtonSprite(g, rect, key, PMod.sprites["Squadron_MoveButton"].Sprite, PMod.sprites["Squadron_MoveButtonOn"].Sprite, null, null, inactive: false, flipX: false, flipY: false, onMouseDown, autoFocus: false, noHover: false, showAsPressed, gamepadUntargetable: true);
                    if (buttonResult2.isHover)
                    {
                        c.isHoveringMove = 2;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AMove), nameof(AMove.Begin)), HarmonyPostfix]
        public static void ReplaceMovement(AMove __instance, G g, State s, Combat c)
        {
            if (!__instance.targetPlayer) return;
            if (s.ship.key != PMod.ships["Squadron"].UniqueName) return;
            int j = 0;
            Ship ship = g.state.ship;
            ArtifactSquadron? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactSquadron) as ArtifactSquadron;
            if (art == null) return;
            s.ship.x -= __instance.dir;
            
            for (int i = 0; i < ship.parts.Count; i++)
            {
                if (s.ship.parts[i] is not PartSquadronUnit unit) continue;
                j++;

                if (unit.hasCrown)
                {
                    int targetIndex = i + __instance.dir;
                    
                    bool expand = targetIndex >= ship.parts.Count || targetIndex < 0;

                    if (expand)
                    {
                        if (ship.parts.Count >= 20)
                        {
                            return;
                        }

                        if (targetIndex >= ship.parts.Count)
                        {
                            int expansion = targetIndex - ship.parts.Count;
                            for (int k = 0; k <= expansion; k++)
                            {
                                ship.parts.Add(new Part() { type = PType.empty });
                            }
                        }

                        if (targetIndex < 0)
                        {
                            for (int k = 0; k < 0 - targetIndex; k++)
                            {
                                ship.parts.Insert(0, new Part() { type = PType.empty });
                            }

                            foreach (Part part in ship.parts)
                            {
                                part.xLerped -= targetIndex;
                            }

                            ship.x += targetIndex;
                            ship.xLerped = ship.x;
                            targetIndex = 0;
                        }
                    }

                    ship.parts.Remove(unit);
                    ship.parts.Insert(targetIndex, unit);

                    if (!expand)
                    {
                        if (j == 1)
                        {
                            int reduction = 0;
                            for (int k = 0; k < targetIndex; k++)
                            {
                                if (ship.parts[0] is PartSquadronUnit) break;
                                ship.parts.RemoveAt(0);
                                reduction++;
                            }
                            foreach(Part part in ship.parts)
                            {
                                part.xLerped -= reduction;
                            }
                            ship.x += reduction;
                            ship.xLerped = ship.x;
                        }

                        if (j == 3)
                        {
                            for (int k = ship.parts.Count - 1; k > targetIndex; k--)
                            {
                                if (ship.parts[ship.parts.Count - 1] is PartSquadronUnit) break;
                                ship.parts.RemoveAt(ship.parts.Count - 1);
                            }
                        }
                    }

                    break;
                }
            }
        }

        [HarmonyPatch(typeof(Card), nameof(Card.GetAllTooltips)), HarmonyPostfix]
        public static void SetPartHilight(Card __instance, G __0, State __1)
        {
            if (__0.state.ship.key != PMod.ships["Squadron"].UniqueName) return;
            if (__1.route is Combat c)
            {
                List<CardAction> actionsOverridden = __instance.GetActionsOverridden(__1, (__1.route as Combat) ?? DB.fakeCombat);
                foreach (CardAction action in actionsOverridden)
                {
                    if (action is AAttack || action is ASpawn)
                    {
                        bool foundPilot = false;
                        foreach (Part part in __1.ship.parts)
                        {
                            if (part is PartSquadronUnit unit)
                            {
                                unit.hilight = unit.pilot == __instance.GetMeta().deck;
                                if (unit.hilight)
                                {
                                    foundPilot = true;
                                }
                            }
                        }
                        if (!foundPilot)
                        {
                            foreach (Part part in __1.ship.parts)
                            {
                                if (part is PartSquadronUnit unit && unit.hasCrown)
                                {
                                    unit.hilight = true;
                                    break;
                                }
                            }
                        }
                        break;
                    } 
                }
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin)), HarmonyPrefix]
        public static void ActivateCannons(State __1, Combat __2, AAttack __instance)
        {
            if (__1.ship.key != PMod.ships["Squadron"].UniqueName) return;
            State s = __1;
            Combat c = __2;
            if (!__instance.targetPlayer)
            {
                ArtifactSquadron? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactSquadron) as ArtifactSquadron;
                if ( art != null) {
                    foreach (Part part in __1.ship.parts)
                    {
                        if (part is PartSquadronUnit unit)
                        {
                            unit.type = unit.hasCrown ? PType.cannon : PType.special;
                        }
                    }
                }
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(ASpawn), nameof(ASpawn.Begin)), HarmonyPrefix]
        public static void ActivateBays(State __1, Combat __2, ASpawn __instance)
        {
            if (__1.ship.key != PMod.ships["Squadron"].UniqueName) return;
            State s = __1;
            Combat c = __2;
            if (__instance.fromPlayer)
            {
                ArtifactSquadron? art = s.EnumerateAllArtifacts().Find((x) => x is ArtifactSquadron) as ArtifactSquadron;
                if (art != null)
                {
                    foreach (Part part in __1.ship.parts)
                    {
                        if (part is PartSquadronUnit unit )
                        {
                            unit.type = unit.hasCrown ? PType.missiles : PType.special;
                        }
                    }
                }
            }
        }

        //PatchVirtual [HarmonyPatch(typeof(AAttack), nameof(AAttack.DoWeHaveCannonsThough)), HarmonyPostfix]
        public static bool DetectCannons(bool __result, State __0, AAttack __instance)
        {
            if (__0.ship.key != PMod.ships["Squadron"].UniqueName) return __result;
            if (!__instance.targetPlayer)
            {
                foreach (Part part in __0.ship.parts)
                {
                    if (part is PartSquadronUnit unit && unit.hasCrown)
                    {
                        return true;
                    }
                }
            }

            return __result;
        }
    }
}
