using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace PhontyPlus
{
    [BepInPlugin("sakyce.baldiplus.phonty", "Phonty", "1.40.0")]
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
            LoadingEvents.RegisterOnAssetsLoaded(this.OnAssetsLoaded, false);
            GeneratorManagement.Register(this, GenerationModType.Addend, AddNPCs);

            CustomOptionsCore.OnMenuInitialize += PhontyMenu.OnMenuInitialize;
            PhontyMenu.Setup();
        }
        private void AddNPCs(string floorName, int floorNumber, LevelObject floorObject) {
#if DEBUG
            floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 1000 });
            foreach (var weighted in floorObject.potentialNPCs)
            {
                print($"{weighted.weight} , {weighted.selection.name}");
            }
#endif

            if (floorNumber > 15)
            {
                return; // Sorry but I don't want Phonty on floor 99 for obvious reasons
            }
            if (floorName.StartsWith("F"))
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 75 });
                floorObject.MarkAsNeverUnload();
            }
            else if (floorName == "END") // Endless
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 80 });
                floorObject.MarkAsNeverUnload();
            }
        }
        private void OnAssetsLoaded()
        {
            Phonty.LoadAssets();
            var phonty = ObjectCreators.CreateNPC<Phonty>(
                "Phonty",
                EnumExtensions.GetFromExtendedName<Character>("Phonty"),
                ObjectCreators.CreatePosterObject(new Texture2D[] { AssetLoader.TextureFromMod(this, "poster.png") }),
                hasLooker: true,
                usesHeatMap: false,
                maxAudioDistance: 300,
                spawnableRooms: new RoomCategory[] { RoomCategory.Faculty }
            );
            phonty.audMan = phonty.GetComponent<AudioManager>();
            CustomSpriteAnimator animator = phonty.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = phonty.spriteRenderer[0];
            phonty.animator = animator;

            var totalBase = (from x in Resources.FindObjectsOfTypeAll<Transform>()
                             where x.name == "TotalBase"
                             select x).First<Transform>();
            var clone = GameObject.Instantiate(totalBase).gameObject;
            DontDestroyOnLoad(clone);
            assetManager.Add("TotalBase", clone);

            var mapIcon = (from x in Resources.FindObjectsOfTypeAll<NoLateIcon>()
                             where x.name == "MapIcon"
                             select x).First<NoLateIcon>();
            assetManager.Add("MapIcon", mapIcon);

            var clock = (ITM_AlarmClock) ObjectFinders.GetFirstInstance(Items.AlarmClock).item;
            assetManager.Add("windup", clock.audWind);

            var silenceRoom = (from x in Resources.FindObjectsOfTypeAll<SilenceRoomFunction>()
                           where x.name == "LibraryRoomFunction"
                           select x).First();
            assetManager.Add<AudioMixer>("Mixer", silenceRoom.mixer);

            assetManager.Add<Phonty>("Phonty", phonty);
            NPCMetaStorage.Instance.Add(new NPCMetadata(Info, new NPC[] { phonty }, "Phonty", NPCFlags.Standard));
        }

    }
}
