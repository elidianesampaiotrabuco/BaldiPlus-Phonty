using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace PhontyPlus
{
    public class Phonty : NPC, IClickable<int>
    {
        public static AssetManager sprites = new AssetManager();
        public static List<SoundObject> records = new List<SoundObject>();
        public static AssetManager audios    = new AssetManager();

        public static List<Sprite> emergeFrames = new List<Sprite>();
        public static List<Sprite> chaseFrames = new List<Sprite>();

        public CustomSpriteAnimator animator;
        public AudioManager audMan;
        public TextMeshPro counter;
        public GameObject totalDisplay;

        public MapIcon mapIconPre;
        public NoLateIcon mapIcon;

        public bool angry = false;
        public static void LoadAssets() {
            var PIXELS_PER_UNIT = 26f;
            sprites.Add("idle_forward",AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(Mod.Instance, "idle_forward.png"), PIXELS_PER_UNIT));
            audios.Add("angry", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Mod.Instance, "PhontyAngry.ogg"), "* Angry Phonograph *", SoundType.Voice, Color.yellow));
            audios.Add("shockwave", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Mod.Instance, "PhontyShot.ogg"), "", SoundType.Effect, Color.yellow));

            for (int i = 0; i <= 37; i++)
            {
                emergeFrames.Add(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(Mod.Instance, "emerge/frame_"+i+".png"), PIXELS_PER_UNIT));
            }

            for (int i = 0; i <= 19; i++)
            {
                chaseFrames.Add(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(Mod.Instance, "angry/frame_" + i + ".png"), PIXELS_PER_UNIT));
            }

            var recordsFolder = Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(Mod.Instance), "phonty_records"));
            foreach (var path in recordsFolder)
            {
                records.Add(ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(path), "* Phonograph Music *", SoundType.Voice, Color.yellow));
            }
        }

        public bool ClickableHidden() => angry;

        public bool ClickableRequiresNormalHeight() => false;

        public void ClickableSighted(int player) {}

        public void ClickableUnsighted(int player) {}

        public void Clicked(int player)
        {
            if (!angry)
            {
                ResetTimer();
            }
        }
        public void EndGame(Transform player)
        {
            Time.timeScale = 0f;
            Singleton<MusicManager>.Instance.StopMidi();
            var core = Singleton<CoreGameManager>.Instance;
            core.disablePause = true;
            core.GetCamera(0).UpdateTargets(transform, 0);
            core.GetCamera(0).offestPos = (player.position - transform.position).normalized * 2f + Vector3.up;
            core.GetCamera(0).controllable = false;
            core.GetCamera(0).matchTargetRotation = false;
            core.audMan.volumeModifier = 0.6f;
            core.audMan.PlaySingle(audios.Get<SoundObject>("shockwave"));
            core.StartCoroutine(core.EndSequence());
            Singleton<InputManager>.Instance.Rumble(1f, 2f);
        }
        public override void Initialize()
        {
            base.Initialize();
            animator.animations.Add("Idle", new CustomAnimation<Sprite>(new Sprite[] { Phonty.sprites.Get<Sprite>("idle_forward") }, 1f));
            animator.animations.Add("Chase", new CustomAnimation<Sprite>(chaseFrames.ToArray(), 1f));
            animator.animations.Add("Emerge", new CustomAnimation<Sprite>(emergeFrames.ToArray(), 1f));
            animator.SetDefaultAnimation("Idle", 1f);

            // Counter on top of Phonty
            var totalBase = GameObject.Instantiate(Mod.assetManager.Get<GameObject>("TotalBase"));
            totalBase.transform.parent = transform;
            totalBase.transform.localPosition = new Vector3(0,3,0);
            totalBase.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            totalBase.SetActive(true);
            totalDisplay = totalBase.gameObject.transform.GetChild(0).gameObject;
            totalDisplay.SetActive(true);
            counter = totalDisplay.GetComponent<TextMeshPro>();

            mapIconPre = Mod.assetManager.Get<NoLateIcon>("MapIcon");
            mapIcon = (NoLateIcon)ec.map.AddIcon(mapIconPre, gameObject.transform, Color.white);
            mapIcon.spriteRenderer.sprite = Phonty.sprites.Get<Sprite>("idle_forward");
            mapIcon.gameObject.SetActive(true);

            behaviorStateMachine.ChangeState(new Phonty_PlayingMusic(this));
            navigator.SetSpeed(4f);
            navigator.maxSpeed = 20f;
            navigator.Entity.SetHeight(7f);
            gameObject.layer = LayerMask.NameToLayer("ClickableEntities");
        }

        public void ResetTimer()
        {
            behaviorStateMachine.ChangeState(new Phonty_PlayingMusic(this));
        }

        public void UpdateCounter(int count) {
            counter.SetText(string.Join("", count.ToString().Select(ch => "<sprite=" + ch + ">")));
            mapIcon.timeText.SetText(count.ToString());
            mapIcon.UpdatePosition(ec.map);
        }
    }

    public class Phonty_StateBase : NpcState
    {
        public Phonty_StateBase(Phonty phonty) : base(phonty)
        {
            this.phonty = phonty;
        }
        public Phonty phonty;

    }
    public class Phonty_PlayingMusic : Phonty_StateBase
    {
        public Phonty_PlayingMusic(Phonty phonty) : base(phonty) {}
        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_DoNothing(phonty, 63));
            phonty.audMan.FlushQueue(true);
            phonty.audMan.PlaySingle(Mod.assetManager.Get<SoundObject>("windup"));
            phonty.audMan.QueueRandomAudio(Phonty.records.ToArray());
            phonty.audMan.SetLoop(true);
            phonty.animator.Play("Idle", 1f);
        }
        public override void Update()
        {
            base.Update();
            if (timeLeft <= 0)
            {
                phonty.behaviorStateMachine.ChangeState(new Phonty_Chase(phonty));
            }
            else {
                timeLeft -= Time.deltaTime * phonty.ec.NpcTimeScale;
            }
            phonty.UpdateCounter((int)timeLeft);
        }
        protected float timeLeft = 180f;
    }
    public class Phonty_Chase : Phonty_StateBase
    {
        protected NavigationState_TargetPlayer targetState;
        protected PlayerManager player;
        public Phonty_Chase(Phonty phonty) : base(phonty)
        {
            player = phonty.ec.Players[0];
            targetState = new NavigationState_TargetPlayer(phonty, 64, player.transform.position);
        }
        public override void Enter()
        {
            base.Enter();
            targetState = new NavigationState_TargetPlayer(phonty, 64, player.transform.position);
            base.ChangeNavigationState(targetState);
            phonty.angry = true;
            phonty.totalDisplay.SetActive(false);
            phonty.mapIcon.gameObject.SetActive(false);

            phonty.audMan.FlushQueue(true);
            phonty.audMan.QueueAudio(Phonty.audios.Get<SoundObject>("angry"), true);
            phonty.audMan.SetLoop(true);

            phonty.animator.Play("Emerge", 1f);
            phonty.animator.SetDefaultAnimation("Chase", 1f);
        }
        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            base.ChangeNavigationState(new NavigationState_WanderRandom(phonty, 32));
        }
        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (this.player == player)
            {
                base.ChangeNavigationState(targetState);
                targetState.UpdatePosition(player.transform.position);
            }
        }
        public override void OnStateTriggerEnter(Collider other)
        {
            base.OnStateTriggerEnter(other);
            if (other.CompareTag("Player") && other.GetComponent<PlayerManager>() == player)
            {
                phonty.EndGame(other.transform);
            }
        }
    }
}
