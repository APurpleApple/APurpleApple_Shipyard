using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using Shockah.Shared;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.IronExpress
{
    internal class IronExpressEntry : ShipyardEntry
    {
        internal override IReadOnlyList<Type> ExclusiveArtifacts => [
            typeof(ArtifactIronExpress),
            typeof(ArtifactIronExpressV2),
            typeof(ArtifactIronExpressMobius)
            ];

        public override void ApplyPatchesPostDB(Harmony harmony)
        {
            harmony.PatchVirtual(typeof(Ship).GetMethod(nameof(Ship.DrawTopLayer)),
                postfix: new HarmonyMethod(typeof(IronExpressPatches).GetMethod(nameof(IronExpressPatches.DrawStuff)))
                );

            harmony.PatchVirtual(typeof(Combat).GetMethod(nameof(Combat.RenderCards)),
                postfix: new HarmonyMethod(typeof(IronExpressPatches).GetMethod(nameof(IronExpressPatches.DrawCardHint)))
                );

            harmony.PatchVirtual(typeof(Card).GetMethod(nameof(Card.GetDataWithOverrides)),
                postfix: new HarmonyMethod(typeof(IronExpressPatches).GetMethod(nameof(IronExpressPatches.ModifyMiddleCardCost)))
                );
        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            PMod.parts.Add("Rail_Cannon", helper.Content.Ships.RegisterPart("Rail_Cannon", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_rail_cannon].Sprite
            }));

            PMod.parts.Add("Rail_Body", helper.Content.Ships.RegisterPart("Rail_Body", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_rail_body].Sprite
            }));

            PMod.parts.Add("Rail_Missile", helper.Content.Ships.RegisterPart("Rail_Missile", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_rail_missile].Sprite
            }));

            PMod.parts.Add("Rail_Cockpit", helper.Content.Ships.RegisterPart("Rail_Cockpit", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_rail_cockpit].Sprite
            }));

            PMod.parts.Add("Rail_Empty", helper.Content.Ships.RegisterPart("Rail_Empty", new PartConfiguration()
            {
                Sprite = SSpr.parts_scaffolding
            }));

            PMod.ships.Add("IronExpress", helper.Content.Ships.RegisterShip("IronExpress", new ShipConfiguration()
            {
                Ship = new StarterShip()
                {
                    ship = new Ship()
                    {
                        hull = 11,
                        hullMax = 11,
                        shieldMaxBase = 4,
                        parts =
                    {
                        new Part()
                        {
                            type = PType.wing,
                            skin = PMod.parts["Rail_Body"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.missiles,
                            skin = PMod.parts["Rail_Missile"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new PartRailCannon()
                        {
                            type = PType.cannon,
                            skin = "",
                            damageModifier = PDamMod.weak,
                            overlapedPart = new Part()
                            {
                                type = PType.empty,
                                skin = PMod.parts["Rail_Empty"].UniqueName,
                                damageModifier = PDamMod.none,
                            }
                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = PMod.parts["Rail_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.wing,
                            skin = PMod.parts["Rail_Body"].UniqueName,
                            damageModifier = PDamMod.none,
                            flip = true
                        }
                    }
                    },
                    cards =
                {
                    new CannonColorless(),
                    new CannonColorless(),
                    new DodgeColorless(),
                    new BasicShieldColorless(),
                },
                    artifacts =
                {
                    new ShieldPrep(),
                    new ArtifactIronExpress()
                }
                },
                ExclusiveArtifactTypes = ExclusiveArtifacts.ToFrozenSet(),
                UnderChassisSprite = SSpr.parts_none,

                Name = PMod.Instance.AnyLocalizations.Bind(["ship", "Rail", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["ship", "Rail", "description"]).Localize
            }));
        }
    }
}
