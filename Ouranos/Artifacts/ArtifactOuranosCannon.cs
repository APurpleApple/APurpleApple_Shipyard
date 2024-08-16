using APurpleApple.Shipyard.Ouranos.Parts;
using Microsoft.Xna.Framework.Input;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace APurpleApple.Shipyard.Ouranos;

internal sealed class ArtifactOuranosCannon : Artifact, IModArtifact, IOuranosCannon
{
    public int counter = 0;

    public static void SetCannonSprite(Spr sprite, State s)
    {
        PartOuranosCannon? p = GetCannon(s);
        if (p != null) p.sprite = sprite;
    }
    public static PartOuranosCannon? GetCannon(State s)
    {
        PartOuranosCannon? p = s.ship.parts.Find((x) => x is PartOuranosCannon) as PartOuranosCannon;
        return p;
    }
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
                pools = [ArtifactPool.EventOnly]
            },
            Sprite = PMod.sprites[PSpr.Artifacts_Ouranos].Sprite,
            Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos", "name"]).Localize,
            Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos", "description"]).Localize
        });
    }

    public bool isCannonActive = true;

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

        SetCannonSprite(PMod.sprites[PSpr.Parts_ouranos_cannon].Sprite, state);

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
            SetCannonSprite(PMod.sprites[PSpr.Parts_ouranos_cannon_off].Sprite, state);
            isCannonActive = false;
            Pulse();
        }
    }

    public override Spr GetSprite()
    {
        return this.isCannonActive ? PMod.sprites[PSpr.Artifacts_Ouranos].Sprite : PMod.sprites[PSpr.Artifacts_OuranosOff].Sprite;
    }
}
