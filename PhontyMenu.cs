using BepInEx.Configuration;
using MTM101BaldAPI.OptionsAPI;
using UnityEngine;

namespace PhontyPlus
{
    internal class PhontyMenu : CustomOptionsCategory
    {
        public static ConfigEntry<bool> nonLethalConfig;
        public static ConfigEntry<int> timeLeftUntilMad;
        public static ConfigEntry<bool> guaranteeSpawn;

        private MenuToggle nonlethalToggle;
        private MenuToggle guaranteeSpawnToggle;

        public override void Build()
        {
            nonlethalToggle = CreateToggle(
                "NonLethal",
                "Non-Lethal",
                nonLethalConfig.Value,
                new Vector2(20f, 0),
                300f
            );
            AddTooltip(nonlethalToggle, "If enabled, Phonty will deafen the player instead of ending the game");
            nonlethalToggle.transform.SetParent(transform, false);
            nonlethalToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() =>
            {
                nonLethalConfig.Value = !nonlethalToggle.Value;
                Mod.Instance.ReloadSaveTags();
            });

            guaranteeSpawnToggle = CreateToggle(
                "GuaranteeSpawn",
                "Guarantee Spawn",
                guaranteeSpawn.Value,
                new Vector2(20f, -25f),
                300f
            );
            AddTooltip(guaranteeSpawnToggle, "If enabled, Phonty will be guaranteed to always spawn. <color=red>(requires game reload)</color>");
            guaranteeSpawnToggle.transform.SetParent(transform, false);
            guaranteeSpawnToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() =>
            {
                guaranteeSpawn.Value = !guaranteeSpawn.Value;
                Mod.Instance.ReloadSaveTags();
            });
        }

        internal static void OnMenuInitialize(OptionsMenu optionsMenu, CustomOptionsHandler handler)
        {
            if (!BaseGameManager.Instance)
                handler.AddCategory<PhontyMenu>("Phonty Phonograph");
        }
        /// <summary>
        /// Triggered when launching the mod, mostly to setup BepInEx config bindings.
        /// </summary>
        internal static void Setup(ConfigFile config)
        {
            nonLethalConfig = config.Bind("Phonty", "NonLethal", false, "Enabling this will replace Phonty's ability to end the game with deafening the player.");
            timeLeftUntilMad = config.Bind("Phonty", "WindUpTime", 180, "Amount of seconds until Phonty will become mad if not wound up.");
            guaranteeSpawn = config.Bind("Phonty", "GuaranteeSpawn", false, "Enabling this will make sure that Phonty will ALWAYS spawn. Used to check if Phonty actually works.");
        }
    }
}
