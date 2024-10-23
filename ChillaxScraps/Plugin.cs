using BepInEx;
using ChillaxScraps.CustomEffects;
using ChillaxScraps.Utils;
using LethalLib.Modules;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace ChillaxScraps
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.chillaxscraps";
        const string NAME = "ChillaxScraps";
        const string VERSION = "1.1.0";

        public static Plugin instance;
        public static List<AudioClip> audioClips;
        public static List<GameObject> gameObjects;
        internal static Config config { get; private set; } = null!;

        void LoadItemBehaviour(Item item, int behaviourId)
        {
            GrabbableObject script;
            switch (behaviourId)
            {
                case 1: script = item.spawnPrefab.AddComponent<DeathNote>(); break;
                case 2: script = item.spawnPrefab.AddComponent<Boink>(); break;
                case 3: script = item.spawnPrefab.AddComponent<Food>(); break;
                case 4: script = item.spawnPrefab.AddComponent<UnoReverse>(); break;
                case 5: script = item.spawnPrefab.AddComponent<EmergencyMeeting>(); break;
                default: return;
            }
            script.grabbable = true;
            script.isInFactory = true;
            script.grabbableToEnemies = true;
            script.itemProperties = item;
        }

        void Awake()
        {
            instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chillaxscraps");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            string directory = "Assets/Data/";

            gameObjects = new List<GameObject> {
                bundle.LoadAsset<GameObject>(directory + "DeathNote/DeathNoteCanvas.prefab"),
                bundle.LoadAsset<GameObject>(directory + "EmergencyMeeting/EmergencyMeetingCanvas.prefab")
            };

            audioClips = new List<AudioClip> {
                bundle.LoadAsset<AudioClip>(directory + "_audio/Page_Turn_Sound_Effect.wav"),
                bundle.LoadAsset<AudioClip>(directory + "_audio/Death_Note_Heart_Attack_Sound_Effect.wav"),
                bundle.LoadAsset<AudioClip>(directory + "_audio/Boink.wav"),
                bundle.LoadAsset<AudioClip>(directory + "_audio/EatSFX.wav"),
                bundle.LoadAsset<AudioClip>(directory + "_audio/uno-reverse-biaatch.wav"),
                bundle.LoadAsset<AudioClip>(directory + "_audio/emergencymeeting.wav")
            };

            var scraps = new List<Scrap> {
                new Scrap("DeathNote/DeathNoteItem.asset", 5, 1),
                new Scrap("Boink/BoinkItem.asset", 12, 2),
                new Scrap("Eevee/EeveeItem.asset", 10),
                new Scrap("CupNoodle/CupNoodleItem.asset", 11, 3),
                new Scrap("Moai/MoaiItem.asset", 9),
                new Scrap("UnoReverseCard/UnoReverseCardItem.asset", 8, 4),
                new Scrap("FroggyChair/FroggyChairItem.asset", 10),
                new Scrap("EmergencyMeeting/EmergencyMeetingItem.asset", 6, 5)
            };

            int i = 0; config = new Config(base.Config, scraps);
            SetupScript.Network();

            foreach (Scrap scrap in scraps)
            {
                Item item = bundle.LoadAsset<Item>(directory + scrap.asset);
                if (scrap.behaviourId != 0) LoadItemBehaviour(item, scrap.behaviourId);
                NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
                Utilities.FixMixerGroups(item.spawnPrefab);
                Items.RegisterScrap(item, config.entries[i++].Value, Levels.LevelTypes.All);
            }

            Logger.LogInfo("ChillaxScraps is loaded !");
        }
    }
}
