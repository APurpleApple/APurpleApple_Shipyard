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

namespace APurpleApple.Shipyard.ShootingStar
{
    internal class ShootingStarEntry : ShipyardEntry
    {
        internal override IReadOnlyList<Type> ExclusiveArtifacts => [
            typeof(ArtifactAsteroid),
            typeof(ArtifactAsteroidDrawBoss),
            typeof(ArtifactAsteroidRetain),
            ];

        internal override IReadOnlyList<Type> RegisteredCards => [
            typeof(CardAsteroidBay),
            typeof(CardAsteroidShot),
            typeof(CardAsteroidBlock),
            typeof(CardAsteroidDodge),
            ];

        public override void ApplyPatchesPostDB(Harmony harmony)
        {
            harmony.Patch(typeof(InitialBooster).GetMethod(nameof(InitialBooster.ModifyBaseDamage)),
                postfix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.InitialBoosterPostfix))
            );

            harmony.Patch(typeof(Card).GetMethod(nameof(Card.RenderAction)),
                postfix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.DisableShieldActionsPostfix))
            );

            harmony.Patch(typeof(AStatus).GetMethod(nameof(AStatus.Begin)),
                prefix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.StopShieldPrefix))
            );

            harmony.Patch(typeof(AMove).GetMethod(nameof(AMove.Begin)),
                prefix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.AMoveBeginPrefix))
            );

            harmony.Patch(typeof(Combat).GetMethod(nameof(Combat.DoEvade)),
                prefix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.DoEvadePrefix))
            );

            harmony.Patch(typeof(Combat).GetMethod(nameof(Combat.RenderMoveButtons)),
                prefix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.RenderMoveButtonPrefix))
            );

            harmony.Patch(typeof(State).GetMethod(nameof(State.PlayerShipCanMove)),
                postfix: typeof(AsteroidPatches).GetMethod(nameof(AsteroidPatches.StopMovementPostfix))
            );
        }

        public override void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            base.Register(helper, package);

            PMod.parts.Add("Asteroid_Cannon", helper.Content.Ships.RegisterPart("Asteroid_Cannon", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_asteroid_cannon].Sprite,
            }));
            PMod.parts.Add("Asteroid_Missile", helper.Content.Ships.RegisterPart("Asteroid_Missile", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_asteroid_missile].Sprite,
            }));
            PMod.parts.Add("Asteroid_Comms", helper.Content.Ships.RegisterPart("Asteroid_Comms", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_asteroid_comms].Sprite,
            }));
            PMod.parts.Add("Asteroid_Engine", helper.Content.Ships.RegisterPart("Asteroid_Engine", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_asteroid_engine].Sprite,
            }));
            PMod.parts.Add("Asteroid_Cockpit", helper.Content.Ships.RegisterPart("Asteroid_Cockpit", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_asteroid_cockpit].Sprite,
            }));
            PMod.parts.Add("Asteroid_Scaffolding", helper.Content.Ships.RegisterPart("Asteroid_Scaffolding", new PartConfiguration()
            {
                Sprite = PMod.sprites[PSpr.Parts_asteroid_scaffolding].Sprite,
            }));



            PMod.ships.Add("Asteroid", helper.Content.Ships.RegisterShip("Asteroid", new ShipConfiguration()
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
                            type = PType.cockpit,
                            skin = PMod.parts["Asteroid_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "AsteroidCockpit"
                        },
                        new Part()
                        {
                            type = PType.cannon,
                            skin = PMod.parts["Asteroid_Cannon"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "AsteroidCannon"
                        },
                        new Part()
                        {
                            type = PType.wing,
                            skin = PMod.parts["Asteroid_Engine"].UniqueName,
                            damageModifier = PDamMod.weak,
                            key = "AsteroidEngine",
                        },
                        new Part()
                        {
                            type = PType.comms,
                            skin = PMod.parts["Asteroid_Comms"].UniqueName,
                            damageModifier = PDamMod.weak,
                            key = "AsteroidComms"
                        },
                        new Part()
                        {
                            type = PType.missiles,
                            skin = PMod.parts["Asteroid_Missile"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "AsteroidMissile"
                        }
                    }
                    },
                    cards =
                {
                    new CardAsteroidShot(),
                    new CardAsteroidBlock(),
                    new CardAsteroidBay(),
                    new CardAsteroidDodge(),
                },
                    artifacts =
                {
                    new ArtifactAsteroid(),
                    new ShieldPrep()
                }
                },
                ExclusiveArtifactTypes = ExclusiveArtifacts.ToFrozenSet(),
                UnderChassisSprite = PMod.sprites[PSpr.Parts_asteroid_chassis].Sprite,

                Name = PMod.Instance.AnyLocalizations.Bind(["ship", "Asteroid", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["ship", "Asteroid", "description"]).Localize
            }));
        }
    }
}
