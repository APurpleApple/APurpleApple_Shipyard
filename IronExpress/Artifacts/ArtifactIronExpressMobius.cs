using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.IronExpress
{
    public class ArtifactIronExpressMobius : Artifact, IModArtifact
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
                    pools = [ArtifactPool.Common],
                    unremovable = false,
                },
                Sprite = PMod.sprites[PSpr.Artifacts_MobiusTracks].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "IronExpressMobius", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "IronExpressMobius", "description"]).Localize
            });
        }
    }
}
