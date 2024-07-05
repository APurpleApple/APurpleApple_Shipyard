using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SharedArt;

namespace APurpleApple.Shipyard.Squadron
{
    [HarmonyPatch]
    internal static class SquadronPatches
    {
        public static Deck? GetLeader(State s)
        {
            ArtifactSquadron? artifact = s.EnumerateAllArtifacts().Find((a) => a is ArtifactSquadron) as ArtifactSquadron;

            return artifact == null ? null : artifact.leader;
        }

        public static void SetLeader(State s, Deck? leader)
        {
            ArtifactSquadron? artifact = s.EnumerateAllArtifacts().Find((a) => a is ArtifactSquadron) as ArtifactSquadron;

            if (artifact == null) return;

            artifact.leader = leader;
        }

        //[HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyPostfix]
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

                Deck? leader = GetLeader(g.state);

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

                    Draw.Sprite(PMod.sprites[isMissing ? PSpr.Parts_squadron_fighter_broken : PSpr.Parts_squadron_fighter].Sprite, partPos.x, partPos.y, color: isMissing ? new Color(.75,.75,.75,1) : Colors.white);

                    if (unit.pilot.HasValue)
                    {
                        Draw.Sprite(PMod.sprites[PSpr.Parts_squadron_color_decal].Sprite, partPos.x, partPos.y, color: DB.decks[unit.pilot.Value].color);
                        if (g.state.route is Combat c)
                        {
                            Draw.Text(Character.GetDisplayName(unit.pilot.Value, g.state), partPos.x + 13, partPos.y + 10, color: DB.decks[unit.pilot.Value].color, outline: Colors.black, align: daisyowl.text.TAlign.Center);
                                
                        }
                    }

                    if (unit.pilot == leader)
                    {
                        Draw.Sprite(PMod.sprites[PSpr.Icons_crown].Sprite, partPos.x, partPos.y, color: Colors.white.fadeAlpha(Math.Abs(Math.Sin(g.time * 2))));
                    }
                }
            }
        }

        

        //[HarmonyPatch(typeof(State), nameof(State.PopulateRun)), HarmonyPrefix, HarmonyPriority(0)]
        public static void SetPilots(State __instance, StarterShip shipTemplate)
        {
            if (__instance.ship.key == PMod.ships["Squadron"].UniqueName)
            {
                foreach (PartSquadronUnit part in __instance.ship.parts.Where((p)=> p is PartSquadronUnit))
                {
                    part.pilot = null;
                }

                foreach (Deck deck in __instance.runConfig.selectedChars)
                {
                    SetLeader(__instance, deck);
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
             }
        }

        //[HarmonyPatch(typeof(Combat), nameof(Combat.RenderMoveButtons)), HarmonyPrefix]
        public static bool HideMoveButtons(G g)
        {
            return g.state.ship.key != PMod.ships["Squadron"].UniqueName;
        }

        //[HarmonyPatch(typeof(Combat), nameof(Combat.RenderMoveButtons)), HarmonyPostfix]
        public static void RenderMoveButtons(Combat __instance, G g)
        {
            if (g.state.ship.key != PMod.ships["Squadron"].UniqueName) return;
            Combat c = __instance;

            if (!c.isPlayerTurn || g.state.ship.hull <= 0 || c.otherShip.hull <= 0)
            {
                return;
            }

            if (PMod.kokoroApi == null && g.state.ship.Get(SStatus.evade) <= 0)
            {
                return;
            }

            int j = 0;
            for (int i = 0; i < g.state.ship.parts.Count; i++)
            {
                Part part = g.state.ship.parts[i];
                if (part is not PartSquadronUnit unit) continue;

                Color color = Colors.white;

                if (unit.pilot.HasValue)
                {
                    color = DB.decks[unit.pilot.Value].color;
                    Status missingStatus;
                    if (StatusMeta.deckToMissingStatus.TryGetValue(unit.pilot.Value, out missingStatus))
                    {
                        if (g.state.ship.Get(missingStatus) > 0)
                        {
                            j++;
                            continue;
                        }
                    }
                }

                double x = (Combat.arenaPos + c.GetCamOffset() + g.state.ship.GetWorldPos(g.state, c)).x + ((part.xLerped ?? ((double)i)) * 16.0);
                double yOffset = j % 2 == 0 ? 24 : -8;
                j++;


                if ((i != 0 || g.state.ship.parts.Count < 20) && (PMod.kokoroApi == null || PMod.kokoroApi.IsEvadePossible(g.state, c, 1, ExternalAPIs.EvadeHookContext.Rendering)))
                {
                    UIKey uIKey = new UIKey(SUK.btn_move_left, j);
                    Rect rect = new Rect(x - 4, 111.0 + yOffset , 8.0, 11.0);
                    UIKey key = uIKey;
                    OnMouseDown onMouseDown = c ;
                    bool showAsPressed = !c.eyeballPeek && Input.GetGpHeld(Btn.TriggerL);
                    SharedArt.ButtonResult buttonResult = SharedArt.ButtonSprite(g, rect, key, PMod.sprites[PSpr.UI_squadron_move_right].Sprite, PMod.sprites[PSpr.UI_squadron_move_right_on].Sprite, null, null, inactive: false, flipX: true, flipY: false, onMouseDown, autoFocus: false, noHover: false, showAsPressed, gamepadUntargetable: true);
                    if (buttonResult.isHover)
                    {
                        c.isHoveringMove = 2;
                    }
                    Draw.Sprite(buttonResult.isHover ? PMod.sprites[PSpr.UI_squadron_move_right_color_on].Sprite : PMod.sprites[PSpr.UI_squadron_move_right_color].Sprite, rect.x + 4, rect.y + 26, flipX: true, color: color);
                }

                if (i != 19 && (PMod.kokoroApi == null || PMod.kokoroApi.IsEvadePossible(g.state, c, 1, ExternalAPIs.EvadeHookContext.Rendering)))
                {
                    UIKey uIKey = new UIKey(SUK.btn_move_right, j);
                    Rect rect = new Rect(x + 11, 111.0 + yOffset, 8.0, 11.0);
                    UIKey key = uIKey;
                    OnMouseDown onMouseDown = c;
                    bool showAsPressed = !c.eyeballPeek && Input.GetGpHeld(Btn.TriggerR);
                    SharedArt.ButtonResult buttonResult2 = SharedArt.ButtonSprite(g, rect, key, PMod.sprites[PSpr.UI_squadron_move_right].Sprite, PMod.sprites[PSpr.UI_squadron_move_right_on].Sprite, null, null, inactive: false, flipX: false, flipY: false, onMouseDown, autoFocus: false, noHover: false, showAsPressed, gamepadUntargetable: true);
                    if (buttonResult2.isHover)
                    {
                        c.isHoveringMove = 2;
                    }
                    Draw.Sprite(buttonResult2.isHover ? PMod.sprites[PSpr.UI_squadron_move_right_color_on].Sprite : PMod.sprites[PSpr.UI_squadron_move_right_color].Sprite, rect.x + 5, rect.y + 26, flipX: false, color: color);
                }
            }
        }

        //[HarmonyPatch(typeof(Combat), nameof(Combat.DoEvade)), HarmonyPrefix]
        public static void SetLeaderOnMove(G g)
        {
            if (g.state.ship.key != PMod.ships["Squadron"].UniqueName) return;
            if (g.hoverKey == SUK.btn_move_left || g.hoverKey == SUK.btn_move_right)
            {
                int j = 0;
                Ship ship = g.state.ship;
                for (int i = 0; i < ship.parts.Count; i++)
                {
                    if (g.state.ship.parts[i] is not PartSquadronUnit unit) continue;
                    j++;

                    if (j == g.hoverKey.Value.v)
                    {
                        SetLeader(g.state, unit.pilot);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(AAttack), nameof(AAttack.ApplyAutododge)), HarmonyPrefix]
        public static bool AutododgeFix(AAttack __instance, Combat c, Ship target, RaycastResult ray, ref bool __result)
        {
            if (target.key != PMod.ships["Squadron"].UniqueName) return true;

            if (ray.hitShip && !__instance.isBeam)
            {
                if (target.Get(Status.autododgeRight) > 0)
                {
                    target.Add(Status.autododgeRight, -1);
                    PartSquadronUnit? hitPart = target.GetPartAtWorldX(ray.worldX) as PartSquadronUnit;

                    if (hitPart != null)
                    {
                        SetLeader(MG.inst.g.state, hitPart.pilot);

                        c.QueueImmediate(new List<CardAction>
                        {
                            new AMove
                            {
                                targetPlayer = __instance.targetPlayer,
                                dir = 1
                            },
                            __instance
                        });
                        __instance.timer = 0.0;
                        __result = true;
                        return false;
                    }
                    
                }

                if (target.Get(Status.autododgeLeft) > 0)
                {
                    target.Add(Status.autododgeLeft, -1);
                    PartSquadronUnit? hitPart = target.GetPartAtWorldX(ray.worldX) as PartSquadronUnit;

                    if (hitPart != null)
                    {
                        SetLeader(MG.inst.g.state, hitPart.pilot);

                        c.QueueImmediate(new List<CardAction>
                        {
                            new AMove
                            {
                                targetPlayer = __instance.targetPlayer,
                                dir = -1
                            },
                            __instance
                        });
                        __instance.timer = 0.0;
                        __result = true;
                        return false;
                    }
                }
            }

            return true;
        }

        //[HarmonyPatch(typeof(AMove), nameof(AMove.Begin)), HarmonyPostfix]
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

                if (unit.pilot == art.leader)
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

        //[HarmonyPatch(typeof(Character), nameof(Character.RenderCharacters)), HarmonyPrefix]
        public static void MakePortraitsMini(G g, ref bool mini, ref bool finaleMode)
        {
            if (!finaleMode && g.state.characters.Count() > 3) mini = true;
        }

        //[HarmonyPatch(typeof(Character), nameof(Character.Render)), HarmonyPostfix]
        public static void MissingMiniPortraits(G g, int x, int y, bool mini, bool flipX, Character __instance)
        {
            if (g.state.route is not Combat) { return; }
            if (g.state.CharacterIsMissing(__instance.deckType) && mini)
            {
                int index = (int)Mutil.Mod(Math.Floor(g.state.time * 12.0 + (double)x + (double)y), Character.noiseSprites.Count);
                Spr? id = Character.noiseSprites[index];
                Rect xy = g.Peek();
                double x2 = xy.x + (double)(flipX ? 1 : 4) + (double)(mini ? (-1) : 0);
                double y2 = xy.y + 1.0;
                Color color = Colors.textMain;
                Rect? pixelRect = (mini ? new Rect?(new Rect(0.0, 0.0, 29.0, 29.0)) : null);
                Draw.Sprite(id, x2, y2, flipX: false, flipY: false, 0.0, null, null, null, pixelRect, color);
            }
        }

        //[HarmonyPatch(typeof(Card), nameof(Card.GetAllTooltips)), HarmonyPostfix]
        public static void SetPartHilight(Card __instance, G __0, State __1)
        {
            if (__0.state.ship.key != PMod.ships["Squadron"].UniqueName) return;
            if (__1.route is Combat c)
            {
                Deck? leader = GetLeader(__0.state);
                List<CardAction> actionsOverridden = __instance.GetActionsOverridden(__1, (__1.route as Combat) ?? DB.fakeCombat);
                foreach (CardAction action in actionsOverridden)
                {
                    Type actionType = action.GetType();
                    PType? pType = null;
                    foreach (var item in SquadronEntry.cardActionLooksForType)
                    {
                        if (item.Item1 == actionType)
                        {
                            pType = item.Item2;
                            break;
                        }
                        else if (actionType.IsSubclassOf(item.Item1))
                        {
                            pType = item.Item2;
                            break;
                        }
                    }
                    if (pType != null)
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
                                if (part is PartSquadronUnit unit && unit.pilot == leader)
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

        //PatchVirtual [HarmonyPatch(typeof(CardAction), nameof(CardAction.Begin)), HarmonyPrefix]
        public static void ActivateParts(G __0, State __1, Combat __2, CardAction __instance)
        {
            State s = __1;
            if (s.ship.key != PMod.ships["Squadron"].UniqueName) return;

            Type t = __instance.GetType();
            PType? pType = null;


            for (int i = SquadronEntry.cardActionLooksForType.Count-1; i >= 0 ; i--)
            {
                if (SquadronEntry.cardActionLooksForType[i].Item1 == t)
                {
                    pType = SquadronEntry.cardActionLooksForType[i].Item2;
                    break;
                }
                else if (t.IsSubclassOf(SquadronEntry.cardActionLooksForType[i].Item1))
                {
                    pType = SquadronEntry.cardActionLooksForType[i].Item2;
                }
            }

            if (pType != null)
            {
                Deck? leader = GetLeader(s);
                foreach (Part part in s.ship.parts)
                {
                    if (part is PartSquadronUnit unit)
                    {
                        unit.type = unit.pilot == leader ? pType.Value : PType.special;
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
                return true;
            }

            return __result;
        }
    }
}
