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
            AddTooltip(nonlethalToggle, "If enabled, Phonty will deaf the player instead of ending the game");
            nonlethalToggle.transform.SetParent(transform, false);
            nonlethalToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() => nonLethalConfig.Value = nonlethalToggle.Value);

            guaranteeSpawnToggle = CreateToggle(
                "GuaranteeSpawn",
                "Guarantee Spawn",
                guaranteeSpawn.Value,
                new Vector2(20f, -25f),
                300f
            );
            AddTooltip(guaranteeSpawnToggle, "If enabled, Phonty spawn weight will be increased by a thousand");
            guaranteeSpawnToggle.transform.SetParent(transform, false);
            guaranteeSpawnToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() => guaranteeSpawn.Value = guaranteeSpawnToggle.Value);
        }

        internal static void OnMenuInitialize(OptionsMenu optionsMenu, CustomOptionsHandler handler)
        {
            handler.AddCategory<PhontyMenu>("Phonty Phonograph");
        }
        /// <summary>
        /// Triggered when launching the mod, mostly to setup BepInEx config bindings.
        /// </summary>
        internal static void Setup()
        {
            nonLethalConfig = Mod.Instance.Config.Bind("Phonty", "NonLethal", false, "Enabling this will replace Phonty's ability to kill the player with deaf.");
            timeLeftUntilMad = Mod.Instance.Config.Bind("Phonty", "WindUpTime", 180, "Amount of seconds until Phonty will become mad if not wound up.");
            guaranteeSpawn = Mod.Instance.Config.Bind("Phonty", "GuaranteeSpawn", false, "Enabling this will make sure that Phonty will ALWAYS spawn. Used to check if Phonty actually works.");
        }
    }
}
