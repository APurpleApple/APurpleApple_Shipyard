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

namespace APurpleApple.Shipyard.Squadron
{
    internal class SquadronEntry : ShipyardEntry
    {
        public static List<Tuple<Type, PType>> cardActionLooksForType = new();

        internal override List<Type> DisabledArtifacts => [
            typeof(TridimensionalCockpit),
            typeof(ArmoredBay),
            typeof(GlassCannon)
            ];

        internal override List<string> DisabledEvents => [
            "AddScaffold",
            "ReorganizeShip"
            ];

        internal override IReadOnlyList<Type> ExclusiveArtifacts => [
            typeof(ArtifactSquadron),
            typeof(ArtifactSquadronPlus),
            typeof(ArtifactSquadronEvade),
        ];

        public override void ApplyPatches(Harmony harmony)
        {
            harmony.Patch(typeof(State).GetMethod(nameof(State.PopulateRun)), 
                postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.SetPilots)));
        }

        public override void ApplyPatchesPostDB(Harmony harmony)
        {
            harmony.TryPatchVirtual(
                original: () => typeof(CardAction).GetMethod(nameof(CardAction.Begin)),
                logger: PMod.Instance.Logger,
                prefix: new HarmonyMethod(typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.ActivateParts)))
            );

            harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.DoWeHaveCannonsThough)), 
                postfix: new HarmonyMethod(typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.DetectCannons)))
            );

            harmony.Patch(typeof(Card).GetMethod(nameof(Card.GetAllTooltips)),
                postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.SetPartHilight))
            );

            harmony.Patch(typeof(Character).GetMethod(nameof(Character.Render)),
                postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.MissingMiniPortraits))
            );

            harmony.Patch(typeof(Character).GetMethod(nameof(Character.RenderCharacters)),
                postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.MakePortraitsMini))
            );

            harmony.Patch(typeof(AMove).GetMethod(nameof(AMove.Begin)),
                postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.ReplaceMovement))
            );

            harmony.Patch(typeof(AAttack).GetMethod(nameof(AAttack.ApplyAutododge)),
                prefix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.AutododgeFix))
            );

            harmony.Patch(typeof(Combat).GetMethod(nameof(Combat.DoEvade)),
                prefix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.SetLeaderOnMove))
            );

            harmony.Patch(typeof(Combat).GetMethod(nameof(Combat.RenderMoveButtons)),
                postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.RenderMoveButtons)),
                prefix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.HideMoveButtons))
            );

            harmony.Patch(typeof(Ship).GetMethod(nameof(Ship.DrawTopLayer)),
                    postfix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.DrawStuff))
                );
        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            cardActionLooksForType.Add(new Tuple<Type, PType>(typeof(AAttack), PType.cannon));
            cardActionLooksForType.Add(new Tuple<Type, PType>(typeof(ASpawn), PType.missiles));

            PMod.ships.Add("Squadron", helper.Content.Ships.RegisterShip("Squadron", new ShipConfiguration()
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
                        new PartSquadronUnit()
                        {
                            type = PType.special,
                            skin = "",
                            damageModifier = PDamMod.none,
                            key = "SquadronUnit"
                        },
                        new Part()
                        {
                            type = PType.empty,
                            skin = "",
                        },
                        new PartSquadronUnit()
                        {
                            type = PType.special,
                            skin = "",
                            damageModifier = PDamMod.none,
                            key = "SquadronUnit"
                        },
                        new Part()
                        {
                            type = PType.empty,
                            skin = "",
                        },
                        new PartSquadronUnit()
                        {
                            type = PType.special,
                            skin = "",
                            damageModifier = PDamMod.none,
                            key = "SquadronUnit"
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
                    new ArtifactSquadron()
                }
                },
                UnderChassisSprite = SSpr.parts_none,
                ExclusiveArtifactTypes = ExclusiveArtifacts.ToFrozenSet(),

                Name = PMod.Instance.AnyLocalizations.Bind(["ship", "Squadron", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["ship", "Squadron", "description"]).Localize
            }));

            uniqueName = PMod.ships["Squadron"].UniqueName;
        }
    }
}
