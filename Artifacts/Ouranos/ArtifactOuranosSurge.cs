using Microsoft.Xna.Framework.Input;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace APurpleApple.Shipyard.Artifacts.Ouranos;

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
            Sprite = PMod.sprites["Ouranos_Artifact_Cannon"].Sprite,
            Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos", "name"]).Localize,
            Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos", "description"]).Localize
        });
    }

}
