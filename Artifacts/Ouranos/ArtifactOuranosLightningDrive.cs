using Microsoft.Xna.Framework.Input;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace APurpleApple.Shipyard.Artifacts.Ouranos;

internal sealed class ArtifactOuranosLightningDrive : Artifact, IModArtifact
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
            Sprite = PMod.sprites["Ouranos_Artifact_LightningDrive"].Sprite,
            Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "LightningDrive", "name"]).Localize,
            Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "LightningDrive", "description"]).Localize
        });
    }

    public override void OnTurnStart(State state, Combat combat)
    {
        if (state.ship.Get(PMod.statuses["ElectricCharge"].Status) >= 2)
        {
            combat.Queue(new AStatus() { artifactPulse = this.Key(), status = Status.evade, targetPlayer = true, statusAmount = 1 });
        }
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> tooltips = new List<Tooltip>();

        tooltips.Add(new TTGlossary($"status.{PMod.statuses["ElectricCharge"].Status}", new object[1] { 1 }));
        tooltips.Add(new TTGlossary($"status.evade"));

        return tooltips;
    }

}
