using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;

using System.Collections;
using System.IO;
using System.Linq;

using UnityEngine;

namespace PhontyPlus
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public unsafe class Mod : BaseUnityPlugin
    {
        public const string ModName = "Phonty";
        public const string ModGuid = "io.github.uncertainluei.baldiplus.phonty";
        public const string ModVersion = "4.0";

        public static AssetManager assetManager = new AssetManager();

#pragma warning disable CS8618 // Initialized in Awake
        public string modpath;
        public static Mod Instance;
        private PhontySaveGameIO saveGame;
#pragma warning restore CS8618


        public void Awake()
        {
            Harmony harmony = new Harmony("sakyce.baldiplus.phonty");
            harmony.PatchAllConditionals();
            modpath = AssetLoader.GetModPath(this);
            Instance = this;

            AssetLoader.LocalizationFromFile(Path.Combine(modpath, "Lang_En.json"), Language.English);

            LoadingEvents.RegisterOnAssetsLoaded(Info, OnAssetsLoaded(), false);
            GeneratorManagement.Register(this, GenerationModType.Addend, GeneratorAddend);

            CustomOptionsCore.OnMenuInitialize += PhontyMenu.OnMenuInitialize;
            PhontyMenu.Setup(Config);

            saveGame = new PhontySaveGameIO(Info);
            ModdedSaveGame.AddSaveHandler(saveGame);
        }

        public void ReloadSaveTags()
        {
            saveGame.GenerateTags();
            ModdedFileManager.Instance.RegenerateTags();
        }

        private void GeneratorAddend(string floorName, int floorNumber, SceneObject sceneObject) {
#if DEBUG
            sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 1000 });
            foreach (var weighted in sceneObject.potentialNPCs)
            {
                print($"{weighted.weight} , {weighted.selection.name}");
            }
#else
            if (floorName.StartsWith("F"))
            {
                sceneObject.MarkAsNeverUnload();
                AddNpc(floorNumber == 0, sceneObject);
                return;
            }
            if (floorName == "END") // Endless
            {
                sceneObject.MarkAsNeverUnload();
                AddNpc(true, sceneObject);
            }
#endif
        }

        private void AddNpc(bool guaranteeSpawn, SceneObject sceneObject)
        {
            if (!PhontyMenu.guaranteeSpawn.Value)
            {
                sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 75});
                return;
            }
            if (!guaranteeSpawn) return;
            sceneObject.forcedNpcs = sceneObject.forcedNpcs.AddToArray(assetManager.Get<NPC>("Phonty"));
            sceneObject.additionalNPCs = Mathf.Max(0, sceneObject.additionalNPCs - 1);
        }

        private IEnumerator OnAssetsLoaded()
        {
            yield return 2;
            yield return "Loading Phonty assets";
            Phonty.LoadAssets();
            yield return "Creating Phonty NPC";
            var phonty = new NPCBuilder<Phonty>(Info)
                .SetName("Phonty")
                .SetEnum("Phonty")
                .SetMetaName("Phonty")
                .AddMetaFlag(NPCFlags.Standard)
                .SetPoster(AssetLoader.TextureFromMod(this, "Textures", "pri_phonty.png"), "Phonty_Pri_1", "Phonty_Pri_2")
                .AddLooker()
                .SetMinMaxAudioDistance(0, 300)
                .AddSpawnableRoomCategories(RoomCategory.Faculty)
                .Build();

            phonty.audMan = phonty.GetComponent<AudioManager>();
            CustomSpriteAnimator animator = phonty.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = phonty.spriteRenderer[0];
            phonty.animator = animator;

            assetManager.Add("TotalBase", Instantiate(Resources.FindObjectsOfTypeAll<MathMachine>().First(x => x.GetInstanceID() >= 0).totalTmp.transform.parent.gameObject, MTM101BaldiDevAPI.prefabTransform));

            assetManager.Add("MapIcon", Resources.FindObjectsOfTypeAll<NoLateIcon>().First(x => x.name == "MapIcon" && x.GetInstanceID() >= 0));
            assetManager.Add("windup", ((ITM_AlarmClock)ItemMetaStorage.Instance.FindByEnum(Items.AlarmClock).value.item).audWind);

            var silenceRoom = Resources.FindObjectsOfTypeAll<SilenceRoomFunction>().First(x => x.name == "LibraryRoomFunction" && x.GetInstanceID() >= 0);
            assetManager.Add("Mixer", silenceRoom.mixer);

            assetManager.Add("Phonty", phonty);
        }
    }

    public class PhontySaveGameIO : ModdedSaveGameIOBinary
    {
        public PhontySaveGameIO(PluginInfo info)
        {
            this.info = info;
        }

        private readonly PluginInfo info;
        public override PluginInfo pluginInfo => info;

        public override void Load(BinaryReader reader)
        {
            reader.ReadByte();
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write((byte)0);
        }

        public override void Reset()
        {
        }

        public override string[] GenerateTags()
        {
            return new string[]
            {
                PhontyMenu.nonLethalConfig.ToString(),
                PhontyMenu.timeLeftUntilMad.ToString(),
                PhontyMenu.guaranteeSpawn.ToString()
            };
        }

        public override string DisplayTags(string[] tags)
        {
            if (tags.Length != 3) return "Invalid";
            return $"Non-Lethal: {tags[0]}\nWind-up Time: {tags[1]}s\nGuarantee Spawn: {tags[2]}";
        }
    }
}
