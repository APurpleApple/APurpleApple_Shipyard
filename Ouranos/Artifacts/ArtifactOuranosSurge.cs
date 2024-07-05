using Microsoft.Xna.Framework.Input;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace APurpleApple.Shipyard.Ouranos;

internal sealed class ArtifactOuranosSurge : Artifact, IModArtifact
{
    public int counter = 0;
    public static void Register(IModHelper helper)
    {
        Type type = MethodBase.GetCurrentMethod()!.DeclaringType!;
        helper.Content.Artifacts.RegisterArtifact(type.Name, new()
        {
            ArtifactType = type,
            Meta = new()
            {
                owner = Deck.colorless,
                unremovable = true,
            },
            Sprite = PMod.sprites[PSpr.Artifacts_PowerSurge].Sprite,
            Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "PowerSurge", "name"]).Localize,
            Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "PowerSurge", "description"]).Localize
        });
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        combat.Queue(new AStatus() { targetPlayer = true, status = PMod.statuses["ElectricCharge"].Status, statusAmount = 3, artifactPulse = this.Key() });
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> tooltips = new List<Tooltip>();

        tooltips.Add(new TTGlossary($"status.{PMod.statuses["ElectricCharge"].Status}", new object[1] { 1 }));

        return tooltips;
    }

}
