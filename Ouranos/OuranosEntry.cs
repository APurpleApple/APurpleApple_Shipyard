using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Ouranos
{
    internal class OuranosEntry : ShipyardEntry
    {
        internal override IReadOnlyList<Type> RegisteredCards => [
            typeof(CardOuranosSpark)
            ];

        internal override IReadOnlyList<Type> ExclusiveArtifacts => [
            typeof(ArtifactOuranosCannon),
            typeof(ArtifactOuranosCannonV2),
            typeof(ArtifactOuranosLightningDrive),
            typeof(ArtifactOuranosSurge),
        ];

        public override void ApplyPatchesPostDB(Harmony harmony)
        {
            harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.Begin)),
                postfix: new HarmonyMethod(typeof(ElectricChargesPatches).GetMethod(nameof(ElectricChargesPatches.RemoveElectricCharge)))
                );

            harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.Begin)),
                prefix: new HarmonyMethod(typeof(OuranosPatches).GetMethod(nameof(OuranosPatches.StoreAttackInCannon))),
                postfix: new HarmonyMethod(typeof(OuranosPatches).GetMethod(nameof(OuranosPatches.SpawnBeamEffect)))
                );

            harmony.Patch(typeof(Ship).GetMethod(nameof(Ship.DrawTopLayer)),
                    postfix: typeof(OuranosPatches).GetMethod(nameof(OuranosPatches.DrawCannonPart))
                );

            harmony.Patch(typeof(Card).GetMethod(nameof(Card.GetActualDamage)),
                    postfix: typeof(OuranosPatches).GetMethod(nameof(OuranosPatches.ReduceAttackDamage))
                );

            harmony.Patch(typeof(Ship).GetMethod(nameof(Ship.NormalDamage)),
                    postfix: typeof(ElectricChargesPatches).GetMethod(nameof(ElectricChargesPatches.ReduceElectricCharge))
                );

            harmony.Patch(typeof(Card).GetMethod(nameof(Card.GetActualDamage)),
                    postfix: typeof(ElectricChargesPatches).GetMethod(nameof(ElectricChargesPatches.GetElectricChargeDamage))
                );
        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            PMod.statuses.Add("ElectricCharge", helper.Content.Statuses.RegisterStatus("ElectricCharge", new StatusConfiguration()
            {
                Definition = new StatusDef()
                {
                    affectedByTimestop = false,
                    isGood = true,
                    icon = PMod.sprites[PSpr.Icons_electricChargeStatus].Sprite,
                    border = new Color("ffd400"),
                    color = new Color("ffd400")
                },
                Description = PMod.Instance.AnyLocalizations.Bind(["status", "ElectricCharge", "description"]).Localize,
                Name = PMod.Instance.AnyLocalizations.Bind(["status", "ElectricCharge", "name"]).Localize
            }));

            PMod.statuses.Add("RailgunCharge", helper.Content.Statuses.RegisterStatus("RailgunCharge", new StatusConfiguration()
            {
                Definition = new StatusDef()
                {
                    affectedByTimestop = false,
                    isGood = true,
                    icon = PMod.sprites[PSpr.Icons_railgunChargeStatus].Sprite,
                    border = new Color("ffd400"),
                    color = new Color("ffd400")
                },
                Description = PMod.Instance.AnyLocalizations.Bind(["status", "RailgunCharge", "description"]).Localize,
                Name = PMod.Instance.AnyLocalizations.Bind(["status", "RailgunCharge", "name"]).Localize
            }));

            PMod.parts.Add("Ouranos_Generator", helper.Content.Ships.RegisterPart("Ouranos_Generator", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_ouranos_generator].Sprite,
            }));
            PMod.parts.Add("Ouranos_Cannon", helper.Content.Ships.RegisterPart("Ouranos_Cannon", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_ouranos_cannon].Sprite,
            }));
            PMod.parts.Add("Ouranos_Cockpit", helper.Content.Ships.RegisterPart("Ouranos_Cockpit", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_ouranos_cockpit].Sprite,
            }));
            PMod.parts.Add("Ouranos_Missile", helper.Content.Ships.RegisterPart("Ouranos_Missile", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_ouranos_missile].Sprite,
            }));

            PMod.ships.Add("Ouranos", helper.Content.Ships.RegisterShip("Ouranos", new ShipConfiguration()
            {
                Ship = new StarterShip()
                {
                    ship = new Ship()
                    {
                        hull = 8,
                        hullMax = 8,
                        shieldMaxBase = 4,
                        parts =
                    {
                        new Part()
                        {
                            type = PType.missiles,
                            skin = PMod.parts["Ouranos_Missile"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.comms,
                            skin = PMod.parts["Ouranos_Generator"].UniqueName,
                            damageModifier = PDamMod.weak
                        },
                        new Part()
                        {
                            type = PType.cannon,
                            skin = PMod.parts["Ouranos_Cannon"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "Ouranos_Cannon",

                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = PMod.parts["Ouranos_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none
                        }
                    }
                    },
                    cards =
                {
                    new CannonColorless(),
                    new DodgeColorless(),
                    new BasicShieldColorless(),
                    new CardOuranosSpark(),
                },
                    artifacts =
                {
                    new ArtifactOuranosCannon(),
                    new ShieldPrep()
                }
                },
                ExclusiveArtifactTypes = ExclusiveArtifacts.ToFrozenSet(),

                UnderChassisSprite = PMod.sprites[PSpr.Parts_ouranos_chassis].Sprite,

                Name = PMod.Instance.AnyLocalizations.Bind(["ship", "Ouranos", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["ship", "Ouranos", "description"]).Localize
            }));
        }
    }
}
