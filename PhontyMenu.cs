using BepInEx.Configuration;
using MTM101BaldAPI.OptionsAPI;
using System;
using UnityEngine;

namespace PhontyPlus
{
    internal class PhontyMenu : MonoBehaviour
    {
        public static ConfigEntry<bool> nonLethalConfig;
        public static ConfigEntry<int> timeLeftUntilMad;

        private MenuToggle nonlethalToggle;

        private void Initialize(OptionsMenu __instance)
        {
            nonlethalToggle = CustomOptionsCore.CreateToggleButton(__instance,
                new Vector2(0, 0), "NonLethal",
                nonLethalConfig.Value,
                "If enabled, Phonty will deaf the player instead of ending the game"
            );

            nonlethalToggle.transform.SetParent(transform, false);
            nonlethalToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() => nonLethalConfig.Value = nonlethalToggle.Value);
        }

        internal static void OnMenuInitialize(OptionsMenu __instance)
        {
            var obj = CustomOptionsCore.CreateNewCategory(__instance, "Phonty Phonograph");
            var phontyMenu = obj.AddComponent<PhontyMenu>();
            phontyMenu.Initialize(__instance);
        }
        /// <summary>
        /// Triggered when launching the mod, mostly to setup BepInEx config bindings.
        /// </summary>
        internal static void Setup()
        {
            nonLethalConfig = Mod.Instance.Config.Bind("Phonty", "NonLethal", false, "Enabling this will replace Phonty's ability to kill the player with deaf.");
            timeLeftUntilMad = Mod.Instance.Config.Bind("Phonty", "WindUpTime", 180, "Amount of seconds until Phonty will become mad if not wound up.");
        }
    }
}
