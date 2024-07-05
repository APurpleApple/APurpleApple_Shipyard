using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;

namespace APurpleApple.Shipyard
{
    internal class ShipyardEntry
    {
        internal virtual IReadOnlyList<Type> ExclusiveArtifacts { get; } = [];
        internal virtual IReadOnlyList<Type> RegisteredCards { get; } = [];
        internal virtual List<string> DisabledEvents { get; } = [];
        internal virtual List<Type> DisabledArtifacts { get; } = [];

        internal string uniqueName = "";

        public virtual void Register(IModHelper helper, IPluginPackage<IModManifest> package)
        {
            foreach (var cardType in RegisteredCards)
                AccessTools.DeclaredMethod(cardType, nameof(IModCard.Register))?.Invoke(null, [helper]);

            foreach (var artifactType in ExclusiveArtifacts)
                AccessTools.DeclaredMethod(artifactType, nameof(IModArtifact.Register))?.Invoke(null, [helper]);
        }

        public virtual void ApplyPatches(Harmony harmony)
        {

        }

        public virtual void ApplyPatchesPostDB(Harmony harmony)
        {

        }
    }
}
