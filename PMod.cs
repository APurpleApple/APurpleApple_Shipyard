using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using APurpleApple.Shipyard.Challenger;
using APurpleApple.Shipyard.ExternalAPIs;
using System.Reflection;
using System.Linq;
using APurpleApple.Shipyard.ShootingStar;
using APurpleApple.Shipyard.Squadron;
using APurpleApple.Shipyard.Ouranos;
using APurpleApple.Shipyard.Shared;
using APurpleApple.Shipyard.IronExpress;
using APurpleApple.Shipyard.EscapePod;

namespace APurpleApple.Shipyard;

public sealed class PMod : SimpleMod
{
    internal static PMod Instance { get; private set; } = null!;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }

    public static Dictionary<PSpr, ISpriteEntry> sprites = new();
    public static Dictionary<string, IStatusEntry> statuses = new();
    public static Dictionary<string, IPartEntry> parts = new();
    public static Dictionary<string, TTGlossary> glossaries = new();
    public static Dictionary<string, ICharacterAnimationEntry> animations = new();
    public static Dictionary<string, IArtifactEntry> artifacts = new();
    public static Dictionary<string, ICardEntry> cards = new();
    public static Dictionary<string, ICharacterEntry> characters = new();
    public static Dictionary<string, IShipEntry> ships = new();
    public static Dictionary<string, IDeckEntry> decks = new();

    public static IKokoroApi? kokoroApi { get; private set; }

    internal static Dictionary<string, ShipyardEntry> shipyardEntries = new(); 
    
    public override object? GetApi(IModManifest requestingMod) => new ApiImplementation();

    public void RegisterSprite(PSpr key, string path, IPluginPackage<IModManifest> package)
    {
        sprites.Add(key, Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile(path)));
    }


    Harmony harmony = new ("APurpleApple.Shipyard");

    private void PatchAfterDB()
    {
        //harmony.PatchAll();
        CustomTTGlossary.ApplyPatches(harmony);

        harmony.Patch(typeof(ArtifactReward).GetMethod(nameof(ArtifactReward.GetBlockedArtifacts)),
                    postfix: typeof(SharedPatches).GetMethod(nameof(SharedPatches.FilterOutArtifacts))
                );

        harmony.Patch(typeof(StoryNode).GetMethod(nameof(StoryNode.Filter)),
                    postfix: typeof(SharedPatches).GetMethod(nameof(SharedPatches.FilterOutEvents))
                );

        harmony.Patch(typeof(Card).GetMethod(nameof(Card.RenderAction)),
                    postfix: typeof(PatchDrawOversizedActions).GetMethod(nameof(PatchDrawOversizedActions.RenderOversizedActionsPostfix))
                );

        harmony.Patch(typeof(Combat).GetMethod(nameof(Combat.UpdateFx)),
                    postfix: typeof(BgFxPatches).GetMethod(nameof(BgFxPatches.FXUpdatePostfix))
                );

        harmony.Patch(typeof(Combat).GetMethod(nameof(Combat.DrawBG)),
                    postfix: typeof(BgFxPatches).GetMethod(nameof(BgFxPatches.FXRenderPostfix))
                );

        foreach (var shipyardEntry in shipyardEntries.Values)
        {
            shipyardEntry.ApplyPatchesPostDB(harmony);
        }
    }

    private void Patch()
    {
        foreach (var shipyardEntry in shipyardEntries.Values)
        {
            shipyardEntry.ApplyPatches(harmony);
        }
    }

    private void RegisterShipyardEntry(string key, ShipyardEntry entry, IModHelper helper, IPluginPackage<IModManifest> package)
    {
        shipyardEntries.Add(key, entry);
        entry.Register(helper, package);
    }

    public PMod(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;

        this.AnyLocalizations = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
        );
        this.Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(this.AnyLocalizations)
        );

        var PSprType = typeof(PSpr);
        foreach (PSpr item in Enum.GetValues(PSprType))
        {
            SpritePath spath = PSprType.GetMember(item.ToString()).FirstOrDefault()!.GetCustomAttribute<SpritePath>()!;
            RegisterSprite(item, spath.pathWithExt, package);
        }

        RegisterShipyardEntry("challenger", new ChallengerEntry(), helper, package);
        RegisterShipyardEntry("squadron", new SquadronEntry(), helper, package);
        RegisterShipyardEntry("ouranos", new OuranosEntry(), helper, package);
        RegisterShipyardEntry("shootingStar", new ShootingStarEntry(), helper, package);
        RegisterShipyardEntry("ironExpress", new IronExpressEntry(), helper, package);
        RegisterShipyardEntry("excapePod", new EscapePodEntry(), helper, package);

        Patch();
        helper.Events.OnModLoadPhaseFinished += (object? sender, ModLoadPhase e) => {
            if (e == ModLoadPhase.AfterDbInit)
            {
                kokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro");

                helper.ModRegistry.GetApi<IDraculaApi>("Shockah.Dracula")?.RegisterBloodTapOptionProvider(statuses["ElectricCharge"].Status, (_, _, status) => [
                    new AHurt { targetPlayer = true, hurtAmount = 1 },
                    new AStatus { targetPlayer = true, status = status, statusAmount = 1 },
                ]);
                

                if (kokoroApi != null)
                {
                    kokoroApi.RegisterEvadeHook(new SquadronKokoroEvadeHook(), double.PositiveInfinity);
                }

                PatchAfterDB();
            }
        };


    }
}
