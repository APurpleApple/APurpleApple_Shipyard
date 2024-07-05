using Microsoft.Xna.Framework.Input;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace APurpleApple.Shipyard.EscapePod;

internal sealed class ArtifactEscapePod : Artifact, IModArtifact
{
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
            Sprite = PMod.sprites[PSpr.Artifacts_GrazedWing].Sprite,
            Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "EscapePod", "name"]).Localize,
            Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "EscapePod", "description"]).Localize
        });
    }
}
