using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using APurpleApple.Shipyard.Parts;
using APurpleApple.Shipyard.Artifacts.IronExpress;
using APurpleApple.Shipyard.Artifacts.Challenger;
using APurpleApple.Shipyard.Patches;
using Shockah.Shared;
using APurpleApple.Shipyard.Artifacts.Ouranos;
using APurpleApple.Shipyard.Cards;
using APurpleApple.Shipyard.Artifacts;

namespace APurpleApple.Shipyard;

public sealed class PMod : SimpleMod
{
    internal static PMod Instance { get; private set; } = null!;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }

    public static Dictionary<string, ISpriteEntry> sprites = new();
    public static Dictionary<string, IStatusEntry> statuses = new();
    public static Dictionary<string, IPartEntry> parts = new();
    public static Dictionary<string, TTGlossary> glossaries = new();
    public static Dictionary<string, ICharacterAnimationEntry> animations = new();
    public static Dictionary<string, IArtifactEntry> artifacts = new();
    public static Dictionary<string, ICardEntry> cards = new();
    public static Dictionary<string, ICharacterEntry> characters = new();
    public static Dictionary<string, IShipEntry> ships = new();
    public static Dictionary<string, IDeckEntry> decks = new();
    internal static IReadOnlyList<Type> Registered_Card_Types { get; } = [
        typeof(CardOuranosSpark),
        typeof(CardAsteroidShot),
        typeof(CardAsteroidBay),
        typeof(CardAsteroidBlock),
        typeof(CardAsteroidDodge),
    ];

    internal static IReadOnlyList<Type> Registered_Artifact_Types { get; } = [
        typeof(ArtifactIronExpress),
        typeof(ArtifactIronExpressV2),
        typeof(ArtifactIronExpressMobius),
        typeof(ArtifactChallenger),
        typeof(ArtifactChallengerChampion),
        typeof(ArtifactOuranosCannon),
        typeof(ArtifactOuranosCannonV2),
        typeof(ArtifactAsteroid)
        //typeof(ArtifactChallengerHighScore)
    ];

    internal static IReadOnlyList<Type> IronExpressExclusiveArtifacts { get; } = [
        typeof(ArtifactIronExpress),
        typeof(ArtifactIronExpressV2),
        typeof(ArtifactIronExpressMobius)
    ];

    internal static IReadOnlyList<Type> ChallengerExclusiveArtifacts { get; } = [
        typeof(ArtifactChallenger),
        typeof(ArtifactChallengerChampion)
        // typeof(ArtifactChallengerHighScore)
    ];

    internal static IReadOnlyList<Type> OuranosExclusiveArtifacts { get; } = [
        typeof(ArtifactOuranosCannon),
        typeof(ArtifactOuranosCannonV2)
    ];

    internal static IReadOnlyList<Type> AsteroidExclusiveArtifacts { get; } = [
        typeof(ArtifactAsteroid),
    ];

    public void RegisterSprite(string key, string fileName, IPluginPackage<IModManifest> package)
    {
        sprites.Add(key, Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/" + fileName)));
    }

    private void Patch()
    {
        Harmony harmony = new("APurpleApple.Shipyard");
        harmony.PatchAll();

        CustomTTGlossary.ApplyPatches(harmony);

        var postf = new HarmonyMethod(typeof(ElectricChargesPatches).GetMethod(nameof(ElectricChargesPatches.RemoveElectricCharge)));
        harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.Begin)), Logger, postfix: postf);

        postf = new HarmonyMethod(typeof(OuranosPatches).GetMethod(nameof(OuranosPatches.SpawnBeamEffect)));
        var pref = new HarmonyMethod(typeof(OuranosPatches).GetMethod(nameof(OuranosPatches.StoreAttackInCannon)));
        harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.Begin)), Logger, prefix: pref, postfix: postf);

        pref = new HarmonyMethod(typeof(ChallengerPatches).GetMethod(nameof(ChallengerPatches.ChampionBelt)));
        harmony.PatchVirtual(typeof(AAttack).GetMethod(nameof(AAttack.Begin)), Logger, prefix: pref);
    }

    public PMod(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;

        Patch();

        this.AnyLocalizations = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
        );
        this.Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(this.AnyLocalizations)
        );

        RegisterFistShip(package);
        RegisterIronExpress(package);
        RegisterOuranos(package);
        RegisterAsteroidShip(package);

        foreach (var cardType in Registered_Card_Types)
            AccessTools.DeclaredMethod(cardType, nameof(IModCard.Register))?.Invoke(null, [helper]);

        foreach (var artifactType in Registered_Artifact_Types)
            AccessTools.DeclaredMethod(artifactType, nameof(IModArtifact.Register))?.Invoke(null, [helper]);
    }

    private void RegisterAsteroidShip(IPluginPackage<IModManifest> package)
    {
        RegisterSprite("Asteroid_Cannon", "Parts/asteroid_cannon.png", package);
        RegisterSprite("Asteroid_Missile", "Parts/asteroid_missile.png", package);
        RegisterSprite("Asteroid_Comms", "Parts/asteroid_comms.png", package);
        RegisterSprite("Asteroid_Engine", "Parts/asteroid_engine.png", package);
        RegisterSprite("Asteroid_Cockpit", "Parts/asteroid_cockpit.png", package);
        RegisterSprite("Asteroid_Chassis", "Parts/asteroid_chassis.png", package);
        RegisterSprite("Asteroid_Scaffolding", "Parts/asteroid_scaffolding.png", package);
        RegisterSprite("Asteroid_Artifact", "Artifacts/AsteroidArtifact.png", package);

        RegisterSprite("ATossPart", "Icons/tossPart.png", package);
        RegisterSprite("ATossPartFar", "Icons/tossPartFar.png", package);

        parts.Add("Asteroid_Cannon", Helper.Content.Ships.RegisterPart("Asteroid_Cannon", new PartConfiguration()
        {
            Sprite = sprites["Asteroid_Cannon"].Sprite,
        }));
        parts.Add("Asteroid_Missile", Helper.Content.Ships.RegisterPart("Asteroid_Missile", new PartConfiguration()
        {
            Sprite = sprites["Asteroid_Missile"].Sprite,
        }));
        parts.Add("Asteroid_Comms", Helper.Content.Ships.RegisterPart("Asteroid_Comms", new PartConfiguration()
        {
            Sprite = sprites["Asteroid_Comms"].Sprite,
        }));
        parts.Add("Asteroid_Engine", Helper.Content.Ships.RegisterPart("Asteroid_Engine", new PartConfiguration()
        {
            Sprite = sprites["Asteroid_Engine"].Sprite,
        }));
        parts.Add("Asteroid_Cockpit", Helper.Content.Ships.RegisterPart("Asteroid_Cockpit", new PartConfiguration()
        {
            Sprite = sprites["Asteroid_Cockpit"].Sprite,
        }));
        parts.Add("Asteroid_Scaffolding", Helper.Content.Ships.RegisterPart("Asteroid_Scaffolding", new PartConfiguration()
        {
            Sprite = sprites["Asteroid_Scaffolding"].Sprite,
        }));

        

        ships.Add("Asteroid", Helper.Content.Ships.RegisterShip("Asteroid", new ShipConfiguration()
        {
            Ship = new StarterShip()
            {
                ship = new Ship()
                {
                    hull = 5,
                    hullMax = 5,
                    shieldMaxBase = 3,
                    parts =
                    {
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = parts["Asteroid_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "AsteroidCockpit"
                        },
                        new Part()
                        {
                            type = PType.cannon,
                            skin = parts["Asteroid_Cannon"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "AsteroidCannon"
                        },
                        new Part()
                        {
                            type = PType.wing,
                            skin = parts["Asteroid_Engine"].UniqueName,
                            damageModifier = PDamMod.weak,
                            key = "AsteroidEngine",
                        },
                        new Part()
                        {
                            type = PType.comms,
                            skin = parts["Asteroid_Comms"].UniqueName,
                            damageModifier = PDamMod.weak,
                            key = "AsteroidComms"
                        },
                        new Part()
                        {
                            type = PType.missiles,
                            skin = parts["Asteroid_Missile"].UniqueName,
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
            ExclusiveArtifactTypes = AsteroidExclusiveArtifacts.ToFrozenSet(),
            UnderChassisSprite = sprites["Asteroid_Chassis"].Sprite,

            Name = this.AnyLocalizations.Bind(["ship", "Asteroid", "name"]).Localize,
            Description = this.AnyLocalizations.Bind(["ship", "Asteroid", "description"]).Localize
        }));
    }

    private void RegisterOuranos(IPluginPackage<IModManifest> package)
    {
        RegisterSprite("Ouranos_Artifact_Cannon", "Artifacts/Ouranos.png", package);
        RegisterSprite("Ouranos_Artifact_Cannon_V2", "Artifacts/OuranosV2.png", package);
        RegisterSprite("Ouranos_Artifact_Cannon_Off", "Artifacts/OuranosOff.png", package);

        RegisterSprite("Ouranos_Cannon", "Parts/ouranos_cannon.png", package);
        RegisterSprite("Ouranos_Cannon_Off", "Parts/ouranos_cannon_off.png", package);
        RegisterSprite("Ouranos_Cannon_V2_1", "Parts/ouranos_cannon_v2_1.png", package);
        RegisterSprite("Ouranos_Cannon_V2_2", "Parts/ouranos_cannon_v2_2.png", package);
        RegisterSprite("Ouranos_Cannon_V2_3", "Parts/ouranos_cannon_v2_3.png", package);
        RegisterSprite("Ouranos_Chassis", "Parts/ouranos_chassis.png", package);
        RegisterSprite("Ouranos_Generator", "Parts/ouranos_generator.png", package);
        RegisterSprite("Ouranos_Missile", "Parts/ouranos_missile.png", package);
        RegisterSprite("Ouranos_Cockpit", "Parts/ouranos_cockpit.png", package);

        RegisterSprite("Status_ElectricCharge", "Icons/electricChargeStatus.png", package);
        RegisterSprite("Status_RailgunCharge", "Icons/railgunChargeStatus.png", package);

        RegisterSprite("FX_RailgunBeam", "FX/RailgunCharge.png", package);

        statuses.Add("ElectricCharge", Helper.Content.Statuses.RegisterStatus("ElectricCharge", new StatusConfiguration()
        {
            Definition = new StatusDef()
            {
                affectedByTimestop = false,
                isGood = true,
                icon = sprites["Status_ElectricCharge"].Sprite,
                border = new Color("ffd400"),
                color = new Color("ffd400")
            },
            Description = this.AnyLocalizations.Bind(["status", "ElectricCharge", "description"]).Localize,
            Name = this.AnyLocalizations.Bind(["status", "ElectricCharge", "name"]).Localize
        }));

        statuses.Add("RailgunCharge", Helper.Content.Statuses.RegisterStatus("RailgunCharge", new StatusConfiguration()
        {
            Definition = new StatusDef()
            {
                affectedByTimestop = false,
                isGood = true,
                icon = sprites["Status_RailgunCharge"].Sprite,
                border = new Color("ffd400"),
                color = new Color("ffd400")
            },
            Description = this.AnyLocalizations.Bind(["status", "RailgunCharge", "description"]).Localize,
            Name = this.AnyLocalizations.Bind(["status", "RailgunCharge", "name"]).Localize
        }));

        parts.Add("Ouranos_Generator", Helper.Content.Ships.RegisterPart("Ouranos_Generator", new PartConfiguration()
        {
            Sprite = sprites["Ouranos_Generator"].Sprite,
        }));
        parts.Add("Ouranos_Cannon", Helper.Content.Ships.RegisterPart("Ouranos_Cannon", new PartConfiguration()
        {
            Sprite = sprites["Ouranos_Cannon"].Sprite,
        }));
        parts.Add("Ouranos_Cockpit", Helper.Content.Ships.RegisterPart("Ouranos_Cockpit", new PartConfiguration()
        {
            Sprite = sprites["Ouranos_Cockpit"].Sprite,
        }));
        parts.Add("Ouranos_Missile", Helper.Content.Ships.RegisterPart("Ouranos_Missile", new PartConfiguration()
        {
            Sprite = sprites["Ouranos_Missile"].Sprite,
        }));

        ships.Add("Ouranos", Helper.Content.Ships.RegisterShip("Ouranos", new ShipConfiguration()
        {
            Ship = new StarterShip()
            {
                ship = new Ship()
                {
                    hull = 7,
                    hullMax = 7,
                    shieldMaxBase = 4,
                    parts =
                    {
                        new Part()
                        {
                            type = PType.missiles,
                            skin = parts["Ouranos_Missile"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.comms,
                            skin = parts["Ouranos_Generator"].UniqueName,
                            damageModifier = PDamMod.weak
                        },
                        new Part()
                        {
                            type = PType.cannon,
                            skin = parts["Ouranos_Cannon"].UniqueName,
                            damageModifier = PDamMod.none,
                            key = "Ouranos_Cannon",

                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = parts["Ouranos_Cockpit"].UniqueName,
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
            ExclusiveArtifactTypes = OuranosExclusiveArtifacts.ToFrozenSet(),

            UnderChassisSprite = sprites["Ouranos_Chassis"].Sprite,

            Name = this.AnyLocalizations.Bind(["ship", "Ouranos", "name"]).Localize,
            Description = this.AnyLocalizations.Bind(["ship", "Ouranos", "description"]).Localize
        }));
    }

    private void RegisterFistShip(IPluginPackage<IModManifest> package)
    {
        RegisterSprite("Fist_Chassis", "Parts/fist_chassis.png", package);
        RegisterSprite("Fist_Cannon", "Parts/fist_cannon.png", package);
        RegisterSprite("Fist_Chain", "Parts/fist_chain.png", package);
        RegisterSprite("Fist_Chain_Segment", "Parts/fist_chain_segment.png", package);
        RegisterSprite("Fist_Missile", "Parts/fist_missile.png", package);
        RegisterSprite("Fist_Cockpit", "Parts/fist_cockpit.png", package);
        RegisterSprite("Fist_Fist", "Parts/fist_fist.png", package);
        RegisterSprite("Fist_Wing", "Parts/fist_wing.png", package);

        RegisterSprite("Artifact_Challenger", "Artifacts/Challenger.png", package);
        RegisterSprite("Artifact_ChampionBelt", "Artifacts/ChampionBelt.png", package);
        RegisterSprite("Artifact_HighScore", "Artifacts/HighScore.png", package);

        parts.Add("Fist_Cockpit", Helper.Content.Ships.RegisterPart("Fist_Cockpit", new PartConfiguration()
        {
            Sprite = sprites["Fist_Cockpit"].Sprite
        }));
        parts.Add("Fist_Missile", Helper.Content.Ships.RegisterPart("Fist_Missile", new PartConfiguration()
        {
            Sprite = sprites["Fist_Missile"].Sprite
        }));
        parts.Add("Fist_Wing", Helper.Content.Ships.RegisterPart("Fist_Wing", new PartConfiguration()
        {
            Sprite = sprites["Fist_Wing"].Sprite
        }));

        ships.Add("Challenger", Helper.Content.Ships.RegisterShip("Challenger", new ShipConfiguration()
        {
            Ship = new StarterShip()
            {
                ship = new Ship()
                {
                    hull = 10,
                    hullMax = 11,
                    shieldMaxBase = 5,
                    parts =
                    {
                        new PartChallengerFist()
                        {
                            type = PType.cannon,
                            skin = "",
                            damageModifier = PDamMod.none,
                            key = "ChallengerFist"
                        },
                        new Part()
                        {
                            type = PType.wing,
                            skin = parts["Fist_Wing"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.missiles,
                            skin = parts["Fist_Missile"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = parts["Fist_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new PartChallengerFist()
                        {
                            type = PType.cannon,
                            skin = "",
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
            ExclusiveArtifactTypes = ChallengerExclusiveArtifacts.ToFrozenSet(),
            UnderChassisSprite = sprites["Fist_Chassis"].Sprite,

            Name = this.AnyLocalizations.Bind(["ship", "Fist", "name"]).Localize,
            Description = this.AnyLocalizations.Bind(["ship", "Fist", "description"]).Localize
        }));
    }

    private void RegisterIronExpress(IPluginPackage<IModManifest> package)
    {
        RegisterSprite("Rail_Cannon", "Parts/rail_cannon.png", package);
        RegisterSprite("Rail_Body", "Parts/rail_body.png", package);
        RegisterSprite("Rail_Cockpit", "Parts/rail_cockpit.png", package);
        RegisterSprite("Rail_Missile", "Parts/rail_missile.png", package);
        RegisterSprite("Rail_Track", "Parts/rail_track.png", package);
        RegisterSprite("Rail_TrackEnd", "Parts/rail_trackend.png", package);
        RegisterSprite("Rail_Wagon", "Parts/rail_wagon.png", package);

        RegisterSprite("Artifact_IronExpress", "Artifacts/IronExpress.png", package);
        RegisterSprite("Artifact_IronExpressV2", "Artifacts/IronExpressV2.png", package);
        RegisterSprite("Artifact_MobiusTracks", "Artifacts/MobiusTracks.png", package);

        RegisterSprite("UI_IronExpress_Slide", "UI/ironexpress_slide.png", package);
        RegisterSprite("UI_IronExpress_Rotate", "UI/ironexpress_rotate.png", package);

        parts.Add("Rail_Cannon", Helper.Content.Ships.RegisterPart("Rail_Cannon", new PartConfiguration()
        {
            Sprite = sprites["Rail_Cannon"].Sprite
        }));

        parts.Add("Rail_Body", Helper.Content.Ships.RegisterPart("Rail_Body", new PartConfiguration()
        {
            Sprite = sprites["Rail_Body"].Sprite
        }));

        parts.Add("Rail_Missile", Helper.Content.Ships.RegisterPart("Rail_Missile", new PartConfiguration()
        {
            Sprite = sprites["Rail_Missile"].Sprite
        }));

        parts.Add("Rail_Cockpit", Helper.Content.Ships.RegisterPart("Rail_Cockpit", new PartConfiguration()
        {
            Sprite = sprites["Rail_Cockpit"].Sprite
        }));

        parts.Add("Rail_Empty", Helper.Content.Ships.RegisterPart("Rail_Empty", new PartConfiguration()
        {
            Sprite = Spr.parts_scaffolding
        }));

        ships.Add("IronExpress", Helper.Content.Ships.RegisterShip("IronExpress", new ShipConfiguration()
        {
            Ship = new StarterShip()
            {
                ship = new Ship()
                {
                    hull = 10,
                    hullMax = 10,
                    shieldMaxBase = 4,
                    parts =
                    {
                        new Part()
                        {
                            type = PType.wing,
                            skin = parts["Rail_Body"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.missiles,
                            skin = parts["Rail_Missile"].UniqueName,
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
                                skin = parts["Rail_Empty"].UniqueName,
                                damageModifier = PDamMod.none,
                            }
                        },
                        new Part()
                        {
                            type = PType.cockpit,
                            skin = parts["Rail_Cockpit"].UniqueName,
                            damageModifier = PDamMod.none
                        },
                        new Part()
                        {
                            type = PType.wing,
                            skin = parts["Rail_Body"].UniqueName,
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
                    new ArtifactIronExpress(),
                    new ShieldPrep()
                }
            },
            ExclusiveArtifactTypes = IronExpressExclusiveArtifacts.ToFrozenSet(),
            UnderChassisSprite = Spr.parts_none,

            Name = this.AnyLocalizations.Bind(["ship", "Rail", "name"]).Localize,
            Description = this.AnyLocalizations.Bind(["ship", "Rail", "description"]).Localize
        }));
    }
}
