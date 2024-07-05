using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Challenger
{
    public class ArtifactChallengerChampion : Artifact, IModArtifact
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
                    pools = [ArtifactPool.Boss],
                },
                Sprite = PMod.sprites[PSpr.Artifacts_ChampionBelt].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "ChampionBelt", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "ChampionBelt", "description"]).Localize
            });
        }
    }
}
