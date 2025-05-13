using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

using UnityEngine;
using UnityEngine.Audio;

namespace PhontyPlus.Patches
{
    [HarmonyPatch]
    internal class RestoreAudioPatch
    {
        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.ReturnToMenu))]
        [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.RestartLevel))]
        [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.LoadNextLevel))]
        [HarmonyPrefix]
        private static void RestoreAudio()
        {
            AudioListener.volume = 1f;
            Mod.assetManager.Get<AudioMixer>("Mixer").SetFloat("EchoWetMix", 0f);
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.pinedebug")]
    [HarmonyPatch(typeof(PineDebug.PineDebugManager), "InitAssets")]
    class PhontyPineDebugIconPatch
    {
        private static bool initialized;

        private static void Postfix()
        {
            if (initialized) return;
            initialized = true;

            PineDebug.PineDebugManager.pinedebugAssets.Add("BorderPhonty", AssetLoader.TextureFromMod(Mod.Instance, "Textures", "BorderPhonty.png"));
        }
    }
}