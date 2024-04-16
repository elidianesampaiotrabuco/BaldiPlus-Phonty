using BaldiEndless;
using MTM101BaldAPI.AssetTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhontyPlus
{
    internal class EndlessFloorsCompatibility
    {
        public static void Initialize()
        {
            EndlessFloorsPlugin.AddGeneratorAction(Mod.Instance.Info, EndlessGeneratorAction);
        }
        private static void EndlessGeneratorAction(GeneratorData data)
        {
#if DEBUG
            data.npcs.Add(new WeightedNPC() { selection = Mod.assetManager.Get<NPC>("Phonty"), weight = 1000 });
#endif
            data.npcs.Add(new WeightedNPC() { selection = Mod.assetManager.Get<NPC>("Phonty"), weight = 75 });
        }
    }
}
