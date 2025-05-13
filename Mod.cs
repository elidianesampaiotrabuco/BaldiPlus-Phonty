using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;

using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace PhontyPlus
{
    [BepInPlugin("sakyce.baldiplus.phonty", "Phonty", "4.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public unsafe class Mod : BaseUnityPlugin
    {
        public static AssetManager assetManager = new AssetManager();

#pragma warning disable CS8618 // Initialized in Awake
        public string modpath;
        public static Mod Instance;
#pragma warning restore CS8618

        public void Awake()
        {
            Harmony harmony = new Harmony("sakyce.baldiplus.phonty");
            harmony.PatchAllConditionals();
            modpath = AssetLoader.GetModPath(this);
            Instance = this;

            EnumExtensions.ExtendEnum<Character>("Phonty");
            LoadingEvents.RegisterOnAssetsLoaded(Info, OnAssetsLoaded(), false);
            GeneratorManagement.Register(this, GenerationModType.Addend, AddNPCs);

            CustomOptionsCore.OnMenuInitialize += PhontyMenu.OnMenuInitialize;
            PhontyMenu.Setup();
        }

        

        private void AddNPCs(string floorName, int floorNumber, SceneObject sceneObject) {
#if DEBUG
            sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 1000 });
            foreach (var weighted in sceneObject.potentialNPCs)
            {
                print($"{weighted.weight} , {weighted.selection.name}");
            }
#endif
            if (floorName.StartsWith("F"))
            {
                sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = PhontyMenu.guaranteeSpawn.Value ? 10000 : 75 });
                sceneObject.MarkAsNeverUnload();
            }
            else if (floorName == "END") // Endless
            {
                sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = PhontyMenu.guaranteeSpawn.Value ? 10000 : 80 });
                sceneObject.MarkAsNeverUnload();
            }
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
                .SetPoster(ObjectCreators.CreatePosterObject(new Texture2D[] { AssetLoader.TextureFromMod(this, "poster.png") }))
                .AddLooker()
                .SetMinMaxAudioDistance(0, 300)
                .AddSpawnableRoomCategories(RoomCategory.Faculty)
                .Build();

            phonty.audMan = phonty.GetComponent<AudioManager>();
            CustomSpriteAnimator animator = phonty.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = phonty.spriteRenderer[0];
            phonty.animator = animator;

            var totalBase = (from x in Resources.FindObjectsOfTypeAll<Transform>()
                             where x.name == "TotalBase"
                             select x).First();
            var clone = GameObject.Instantiate(totalBase).gameObject;
            DontDestroyOnLoad(clone);
            assetManager.Add("TotalBase", clone);

            var mapIcon = (from x in Resources.FindObjectsOfTypeAll<NoLateIcon>()
                             where x.name == "MapIcon"
                             select x).First();
            assetManager.Add("MapIcon", mapIcon);

            var clock = (ITM_AlarmClock)ItemMetaStorage.Instance.FindByEnum(Items.AlarmClock).value.item;
            assetManager.Add("windup", clock.audWind);

            var silenceRoom = (from x in Resources.FindObjectsOfTypeAll<SilenceRoomFunction>()
                           where x.name == "LibraryRoomFunction"
                           select x).First();
            assetManager.Add("Mixer", silenceRoom.mixer);

            assetManager.Add("Phonty", phonty);
        }

    }
}
