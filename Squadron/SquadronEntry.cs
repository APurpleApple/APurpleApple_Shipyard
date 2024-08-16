using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using Shockah.Shared;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Squadron
{
    internal class SquadronEntry : ShipyardEntry
    {
        public static List<Tuple<Type, PType>> cardActionLooksForType = new();
        public static HashSet<Type> uniquePatchedTypes = new();

        public static void AddCardActionLooksForType(Tuple<Type, PType> type)
        {
            cardActionLooksForType.Add(type);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Type> subtypes = Enumerable.Empty<Type>();
                subtypes = assembly.GetTypes().Where(t => t.IsAssignableTo(type.Item1));

                foreach (Type subtype in subtypes)
                {
                    if (!uniquePatchedTypes.Contains(subtype))
                    {
                        PMod.Instance.harmony.Patch(
                            original: subtype.GetMethod("Begin"),
                            prefix: new HarmonyMethod(typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.ActivateParts)))
                        );
                        uniquePatchedTypes.Add(subtype);
                    }
                }
            }
        }

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
                prefix: typeof(SquadronPatches).GetMethod(nameof(SquadronPatches.MakePortraitsMini))
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

        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            AddCardActionLooksForType(new Tuple<Type, PType>(typeof(AAttack), PType.cannon));
            AddCardActionLooksForType(new Tuple<Type, PType>(typeof(ASpawn), PType.missiles));

            PMod.parts.Add("Squadron", helper.Content.Ships.RegisterPart("Squadron", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_squadron_fighter].Sprite
            }));

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
                            skin = PMod.parts["Squadron"].UniqueName,
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
                            skin = PMod.parts["Squadron"].UniqueName,
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
                            skin = PMod.parts["Squadron"].UniqueName,
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
                    new ArtifactSquadron(),
                    new ArtifactSquadronEvade()
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
