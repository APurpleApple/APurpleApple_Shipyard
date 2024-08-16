using APurpleApple.Shipyard.ShootingStar;
using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using Shockah.Shared;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;

namespace APurpleApple.Shipyard.EscapePod
{
    internal class EscapePodEntry : ShipyardEntry
    {
        internal override IReadOnlyList<Type> ExclusiveArtifacts => [
            typeof(ArtifactEscapePod)
            ];
        internal override IReadOnlyList<Type> RegisteredCards => [
            typeof(CardBasicRam)
            ];

        internal override List<Type> DisabledArtifacts => [
            typeof(TridimensionalCockpit),
            typeof(GlassCannon),
            typeof(ArmoredBay)
            ];

        internal override List<string> DisabledEvents => [
            "AddScaffold",
            "ReorganizeShip"
            ];

        public override void ApplyPatchesPostDB(Harmony harmony)
        {
            harmony.Patch(typeof(Ship).GetMethod(nameof(Ship.DrawBottomLayer)),
                prefix: typeof(PatchRammingAction).GetMethod(nameof(PatchRammingAction.DrawShipUnderPrefix))
            );

            harmony.Patch(typeof(Ship).GetMethod(nameof(Ship.DrawTopLayer)),
                prefix: typeof(PatchRammingAction).GetMethod(nameof(PatchRammingAction.DrawShipOverPrefix))
            );

            harmony.Patch(typeof(AAttack).GetMethod(nameof(AAttack.Begin)),
                prefix: typeof(EscapePodPatches).GetMethod(nameof(EscapePodPatches.DamageOnEmptyPrefix))
            );
        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            PMod.parts.Add("EscapePod_Cockpit", helper.Content.Ships.RegisterPart("EscapePod_Cockpit", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_escapepod_cockpit].Sprite,
            }));

            PMod.parts.Add("EscapePod_Wing", helper.Content.Ships.RegisterPart("EscapePod_Wing", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_escapepod_wing].Sprite,
            }));

            PMod.ships.Add("EscapePod", helper.Content.Ships.RegisterShip("EscapePod", new ShipConfiguration()
            {
                Ship = new StarterShip()
                {
                    ship = new Ship()
                    {
                        hull = 6,
                        hullMax = 6,
                        shieldMaxBase = 3,
                        parts =
                    {
                        new Part()
                        {
                            type = PType.empty,
                            skin = PMod.parts["EscapePod_Wing"].UniqueName,
                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = PMod.parts["EscapePod_Cockpit"].UniqueName,
                        },
                        new Part()
                        {
                            type = PType.empty,
                            skin = PMod.parts["EscapePod_Wing"].UniqueName,
                            flip = true
                        },
                    }
                    },
                    cards =
                {
                    new CardBasicRam(),
                    new CardBasicRam(),
                    new DodgeColorless(),
                    new BasicShieldColorless()
                },
                    artifacts =
                    {
                        new ShieldPrep(),
                        new ArtifactEscapePod()
                    }
                },
                UnderChassisSprite = PMod.sprites[PSpr.Parts_escapepod_chassis].Sprite,

                Name = PMod.Instance.AnyLocalizations.Bind(["ship", "EscapePod", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["ship", "EscapePod", "description"]).Localize
            }));
        }
    }
}
