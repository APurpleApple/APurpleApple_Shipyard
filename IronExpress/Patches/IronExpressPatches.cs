using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.PluginManager;

namespace APurpleApple.Shipyard.IronExpress
{
    [HarmonyPatch]
    internal static class IronExpressPatches
    {
        //[HarmonyPatch(typeof(Ship), nameof(Ship.DrawTopLayer)), HarmonyPostfix]
        public static void DrawStuff(Ship __instance, G __0, Vec __1, Vec __2)
        {
            Vec worldPos = __2;
            Vec v = __1;
            G g = __0;
            if (__instance.key == PMod.ships["IronExpress"].UniqueName)
            {
                for (int i = 0; i < __instance.parts.Count; i++)
                {
                    Part part = __instance.parts[i];

                    PartRailCannon? cannon = null;
                    if (part is PartRailCannon)
                    {
                        cannon = (PartRailCannon)part;
                        cannon.rotLerped = Mutil.MoveTowards(cannon.rotLerped, cannon.isCannon ? (cannon.rotLerped >= double.Pi ? double.Pi * 2 : 0.0) : double.Pi, g.dt * 15.0) % (double.Pi *2);
                    }

                    Vec vec2 = worldPos + new Vec((part.xLerped ?? ((double)i)) * 16.0, -32.0 + (__instance.isPlayerShip ? part.offset.y : (1.0 + (0.0 - part.offset.y))));
                    Vec vec3 = v + vec2;

                    double num3 = 1.0;

                    Vec vec4 = vec3 + new Vec(-1.0, -1.0);
                    double num11 = vec4.x;
                    double y7 = vec4.y;
                    bool flag = !__instance.isPlayerShip;
                    bool flip7 = part.flip;
                    bool flipY7 = flag;
                    Color? color2 = new Color(1.0, 1.0, 1.0, num3);

                    if (cannon != null)
                    {
                        if (cannon.overlapedPart != null)
                        {
                            Spr? s = (cannon.overlapedPart.active ? DB.parts : DB.partsOff).GetOrNull(cannon.overlapedPart.skin ?? cannon.overlapedPart.type.Key());
                            if (s != null)
                            {
                                Draw.Sprite(s, num11, y7, flip7, flipY7, 0.0, null, null, null, null, new Color(0.5, 0.5, 0.5, num3));
                            }
                        }
                    } 

                    Draw.Sprite(PMod.sprites[PSpr.Parts_rail_track].Sprite, num11, y7, flip7, flipY7, 0.0, null, null, null, null, color2);

                    if (i == 0)
                    {
                        Draw.Sprite(PMod.sprites[PSpr.Parts_rail_trackend].Sprite, num11 - 16, y7, false, flipY7, 0.0, null, null, null, null, color2);
                    }
                    if (i == __instance.parts.Count-1)
                    {
                        Draw.Sprite(PMod.sprites[PSpr.Parts_rail_trackend].Sprite, num11 + 2, y7, false, flipY7, 0.0, null, null, new Vec(1,1), null, color2);
                    }

                    if (cannon != null)
                    {
                        Draw.Sprite(PMod.sprites[PSpr.Parts_rail_wagon].Sprite, num11, y7, flip7, flipY7, 0.0, null, null, null, null, color2);
                        Draw.Sprite(PMod.sprites[PSpr.Parts_rail_cannon].Sprite, num11 + 8.5, y7 + 32.5 + (double)(__instance.isPlayerShip ? 6 : (-6)) * part.pulse, flip7, flipY7, cannon.rotLerped, new Vec(8.5, 32.5), null, null, null, color2);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Combat), nameof(Combat.RenderCards)), HarmonyPostfix]
        public static void DrawCardHint(Combat __instance, G __0)
        {
            G g = __0;
            if (g.state.EnumerateAllArtifacts().Any<Artifact>((a) => a is ArtifactIronExpress))
            {
                if (__instance.hand.Count % 2 == 1)
                {
                    Card card = __instance.hand[__instance.hand.Count / 2];
                    Rect rect = card.GetScreenRect() + card.pos + new Vec(0, card.hoverAnim * -2.0);
                    Draw.Sprite(PMod.sprites[PSpr.UI_ironexpress_rotate].Sprite, rect.x + rect.w * .5 - 7, rect.y + 13);
                }

                if (__instance.hand.Count > 1)
                {
                    Card leftCard = __instance.hand[0];
                    Rect leftRect = leftCard.GetScreenRect() + leftCard.pos + new Vec(0, leftCard.hoverAnim * -2.0);
                    Draw.Sprite(PMod.sprites[PSpr.UI_ironexpress_slide].Sprite, leftRect.x - 9, leftRect.y + 35, true);

                    Card rightCard = __instance.hand[__instance.hand.Count-1];
                    Rect rightRect = rightCard.GetScreenRect() + rightCard.pos + new Vec(0, rightCard.hoverAnim * -2.0);
                    Draw.Sprite(PMod.sprites[PSpr.UI_ironexpress_slide].Sprite, rightRect.x + rightRect.w -1, rightRect.y + 35);
                }
            }
        }

        //[HarmonyPatch(typeof(Card), nameof(Card.GetDataWithOverrides)), HarmonyPostfix]
        public static void ModifyMiddleCardCost(Card __instance, State __0, ref CardData __result, bool __runOriginal)
        {
            // if the original didn't run, then it's likely another mod didn't want any expensive code to run (e.g. Nickel card codex caching)
            if (!__runOriginal)
                return;
            
            State s = __0;
            if (s.route is Combat c)
            {
                if (s.EnumerateAllArtifacts().Any((a) => a is ArtifactIronExpressV2)) {
                    if (c.hand.Count % 2 == 1 && c.hand.FindIndex((x) => x == __instance) == c.hand.Count / 2)
                    {
                        PartRailCannon? cannon = s.ship.parts.Find((x) => x is PartRailCannon) as PartRailCannon;
                        if (cannon != null && !cannon.isCannon)
                        {
                            __result.cost -= 1;
                        }
                    }
                }
            }
        }
    }
}
