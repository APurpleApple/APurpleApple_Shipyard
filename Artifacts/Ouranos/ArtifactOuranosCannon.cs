using Microsoft.Xna.Framework.Input;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace APurpleApple.Shipyard.Artifacts.Ouranos;

internal sealed class ArtifactOuranosCannon : Artifact, IModArtifact, IOuranosCannon
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
            Sprite = PMod.sprites["Ouranos_Artifact_Cannon"].Sprite,
            Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos", "name"]).Localize,
            Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos", "description"]).Localize
        });
    }

    public bool isCannonActive = true;

    public Spr CannonSprite { get; set; }

    public ArtifactOuranosCannon()
    {
        CannonSprite = PMod.sprites["Ouranos_Cannon"].Sprite;
    }

    public override void OnReceiveArtifact(State state)
    {
        Part? p = state.ship.parts.Find((x) => x.key == "Ouranos_Cannon");
        if (p != null) p.skin = "";
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> tooltips = new List<Tooltip>();
        
        tooltips.Add(new TTGlossary($"status.{PMod.statuses["ElectricCharge"].Status}", new object[1] { 1 }));

        return tooltips;
    }

    public override int ModifyBaseDamage(int baseDamage, Card? card, State state, Combat? combat, bool fromPlayer)
    {
        if (!fromPlayer) return 0;
        return isCannonActive ? 0 : -99999;
    }

    public override void OnTurnStart(State state, Combat combat)
    {
        isCannonActive = true;

        CannonSprite = PMod.sprites["Ouranos_Cannon"].Sprite;

        Pulse();
    }

    public override void OnTurnEnd(State state, Combat combat)
    {
        combat.Queue(new AStatus()
        {
            status = PMod.statuses["ElectricCharge"].Status,
            statusAmount = 1,
            targetPlayer = true,
        });
    }

    public override void OnPlayerAttack(State state, Combat combat)
    {
        if (isCannonActive)
        {
            CannonSprite = PMod.sprites["Ouranos_Cannon_Off"].Sprite;
            isCannonActive = false;
            Pulse();
        }
    }

    public override Spr GetSprite()
    {
        return this.isCannonActive ? PMod.sprites["Ouranos_Artifact_Cannon"].Sprite : PMod.sprites["Ouranos_Artifact_Cannon_Off"].Sprite;
    }
}
