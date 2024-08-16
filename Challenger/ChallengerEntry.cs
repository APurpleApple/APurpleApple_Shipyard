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

namespace APurpleApple.Shipyard.Challenger
{
    internal class ChallengerEntry : ShipyardEntry
    {
        internal override IReadOnlyList<Type> ExclusiveArtifacts => [
            typeof(ArtifactChallenger),
            typeof(ArtifactChallengerChampion),
            typeof(ArtifactChallengerSweatband),
            typeof(ArtifactChallengerHighScore),
            ];

        public override void ApplyPatchesPostDB(Harmony harmony)
        {
            harmony.Patch(typeof(Card).GetMethod(nameof(Card.GetAllTooltips)),
                    postfix: typeof(ChallengerPatches).GetMethod(nameof(ChallengerPatches.RemovePartHighlight))
                );

            harmony.Patch(typeof(EffectSpawner).GetMethod(nameof(EffectSpawner.Cannon)),
                    prefix: typeof(ChallengerPatches).GetMethod(nameof(ChallengerPatches.PreventCannonFx))
                );

            harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.Begin)),
                prefix: new HarmonyMethod(typeof(ChallengerPatches).GetMethod(nameof(ChallengerPatches.ChampionBelt)))
                );
        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            PMod.parts.Add("Fist_Cockpit", helper.Content.Ships.RegisterPart("Fist_Cockpit", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_fist_cockpit].Sprite
            }));
            PMod.parts.Add("Fist_Missile", helper.Content.Ships.RegisterPart("Fist_Missile", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_fist_missile].Sprite
            }));
            PMod.parts.Add("Fist_Wing", helper.Content.Ships.RegisterPart("Fist_Wing", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_fist_wing].Sprite
            }));
            PMod.parts.Add("Fist_Fist", helper.Content.Ships.RegisterPart("Fist_Fist", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_fist_full].Sprite
            }));

            PMod.ships.Add("Challenger", helper.Content.Ships.RegisterShip("Challenger", new ShipConfiguration()
            {
                Ship = new StarterShip()
                {
                    ship = new Ship()
                    {
                        hull = 14,
                        hullMax = 15,
                        shieldMaxBase = 5,
                        parts =
                    {
                        new PartChallengerFist()
                        {
                            type = PType.cannon,
                            skin = PMod.parts["Fist_Fist"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "ChallengerFist"
                        },
                        new Part()
                        {
                            type = PType.wing,
                            skin = PMod.parts["Fist_Wing"].UniqueName,
                            damageModifier = PDamMod.armor
                        },
                        new Part()
                        {
                            type = PType.missiles,
                            skin = PMod.parts["Fist_Missile"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = PMod.parts["Fist_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new PartChallengerFist()
                        {
                            type = PType.cannon,
                            skin = PMod.parts["Fist_Fist"].UniqueName,
                            damageModifier = PDamMod.none,
                            flip = true,
                            key = "ChallengerFist"
                        },
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
                    new ArtifactChallenger(),
                }
                },
                ExclusiveArtifactTypes = ExclusiveArtifacts.ToFrozenSet(),
                UnderChassisSprite = PMod.sprites[PSpr.Parts_fist_chassis].Sprite,

                Name = PMod.Instance.AnyLocalizations.Bind(["ship", "Fist", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["ship", "Fist", "description"]).Localize
            }));

            uniqueName = PMod.ships["Challenger"].UniqueName;
        }
    }
}
