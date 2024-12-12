using BepInEx;
using BepInEx.Bootstrap;
using ChillaxScraps.CustomEffects;
using ChillaxScraps.Utils;
using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace ChillaxScraps
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("zigzag.premiumscraps", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CodeRebirth.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.chillaxscraps";
        const string NAME = "ChillaxScraps";
        const string VERSION = "1.4.0";

        public static Plugin instance;
        public static List<AudioClip> audioClips = new List<AudioClip>();
        public static List<GameObject> gameObjects = new List<GameObject>();
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;

        void HarmonyPatchAll()
        {
            harmony.CreateClassProcessor(typeof(GetEnemies), true).Patch();  // getenemies patch
            harmony.CreateClassProcessor(typeof(TotemItemPlayerControllerBPatch), true).Patch();  // totem patch
            harmony.CreateClassProcessor(typeof(EnemyAIPatch), true).Patch();  // ocarina enemyai patch
        }

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
                case 6: script = item.spawnPrefab.AddComponent<SuperSneakers>(); break;
                case 7: script = item.spawnPrefab.AddComponent<MasterSword>(); SetupScript.Copy((Shovel)script, item); break;
                case 8: script = item.spawnPrefab.AddComponent<Ocarina>(); SetupScript.Copy((NoisemakerProp)script, item); break;
                case 9: script = item.spawnPrefab.AddComponent<TotemOfUndying>(); break;
                case 10: script = item.spawnPrefab.AddComponent<DanceNote>(); break;
                case 11: script = item.spawnPrefab.AddComponent<Nokia>(); SetupScript.Copy((WalkieTalkie)script, item); break;
                default: return;
            }
            script.grabbable = true;
            script.isInFactory = true;
            script.grabbableToEnemies = true;
            script.itemProperties = item;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        void Awake()
        {
            instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chillaxscraps");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            string directory = "Assets/Data/";

            var prefabs = new string[] { "DeathNote/DeathNoteCanvas.prefab", "EmergencyMeeting/EmergencyMeetingCanvas.prefab",
                "Ocarina/ElegyOfEmptiness.prefab", "DanceNote/DanceNoteCanvas.prefab", "DanceNote/WarningCanvas.prefab",
                "DanceNote/Glow.prefab", "DanceNote/GlowBoom.prefab"
            };

            var audios = new string[] { "Page_Turn_Sound_Effect.wav", "Death_Note_Heart_Attack_Sound_Effect.wav",
                "Boink.wav", "EatSFX.wav", "uno-reverse-biaatch.wav", "emergencymeeting.wav", "sneakers-activate.wav",
                "sneakers-deactivate.wav", "OOT_Fanfare_Item.wav", "undying.wav", "OOT_Warp_Song_In.wav",
                "OOT_Warp_Song_Out.wav", "OOT_Secret.wav", "OOT_SpiritualStone_Appear.wav", "OOT_Warp_Respawn_In.wav",
                "OOT_YoungEpona_Neigh.wav", "Epona_Breath.wav", "Epona_KillPlayer.wav", "OOT_YoungEpona_Walk.wav",
                "OOT_Epona_Walk.wav", "OOT_Epona_Hooves.wav", "Epona_growl.wav", "Epona_Lunge.wav", "giant-spawn.wav",
                "giant-eating.wav", "MM_ClockTower_Bell.wav", "MM_Wizrobe_Appear.wav", "MM_GaleWarp_Out.wav",
                "MM_Goron_Ohhh.wav", "OOT_Biggoron_Ohh.wav", "OOT_Goron_Oop.wav", "OOT_Goron_Ohrr.wav", "MM_Goron_Oh.wav",
                "New_Wave_Bossa_Nova_by_The_Indigo-gos.wav", "MM_Warp.wav", "ChargeItem.ogg", "DeathNoteL.wav",
                "DanceNoteBassPractice.wav", "DanceNote1-Ching.wav", "DanceNote2-Giorno.wav", "DanceNote3-Nyan.wav",
                "DanceNote4-Spectre.wav", "DanceNote5-Gucci.wav", "DanceNote6-Heyyeya.wav", "nokia1.wav", "nokia2.wav",
                "nokia1-far.wav", "nokia2-far.wav"
            };

            foreach (string prefab in prefabs)
            {
                gameObjects.Add(bundle.LoadAsset<GameObject>(directory + prefab));
            }

            foreach (string sfx in audios)
            {
                audioClips.Add(bundle.LoadAsset<AudioClip>(directory + "_audio/" + sfx));
            }

            var scraps = new List<Scrap> {
                new Scrap("DeathNote/DeathNoteItem.asset", 5, 1),
                new Scrap("Boink/BoinkItem.asset", 12, 2),
                new Scrap("Eevee/EeveeItem.asset", 10),
                new Scrap("CupNoodle/CupNoodleItem.asset", 11, 3),
                new Scrap("Moai/MoaiItem.asset", 9),
                new Scrap("UnoReverseCard/UnoReverseCardItem.asset", 8, 4),
                new Scrap("FroggyChair/FroggyChairItem.asset", 10),
                new Scrap("EmergencyMeeting/EmergencyMeetingItem.asset", 6, 5),
                new Scrap("SuperSneakers/SuperSneakersItem.asset", 10, 6),
                new Scrap("MasterSword/MasterSwordItem.asset", 7, 7),
                new Scrap("Ocarina/OcarinaItem.asset", 10, 8),
                new Scrap("TotemOfUndying/TotemOfUndyingItem.asset", 6, 9),
                new Scrap("DanceNote/DanceNoteItem.asset", 5, 10),
                new Scrap("Nokia/NokiaItem.asset", 13, 11)
            };

            int i = 0; config = new Config(base.Config, scraps);
            config.SetupCustomConfigs();
            if (Chainloader.PluginInfos.ContainsKey("zigzag.premiumscraps") && new System.Version("2.0.11").CompareTo(Chainloader.PluginInfos.GetValueOrDefault("zigzag.premiumscraps").Metadata.Version) <= 0)
                config.SetupUnluckyPlayersConfig();  // get unlucky players of PremiumScraps
            SetupScript.Network();

            foreach (Scrap scrap in scraps)
            {
                Item item = bundle.LoadAsset<Item>(directory + scrap.asset);
                if (config.scrapValues[i].Item1 != -1) { item.minValue = config.scrapValues[i].Item1; item.maxValue = config.scrapValues[i].Item2; }
                if (scrap.behaviourId != 0) LoadItemBehaviour(item, scrap.behaviourId);
                NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
                Utilities.FixMixerGroups(item.spawnPrefab);
                Items.RegisterScrap(item, config.entries[i++].Value, Levels.LevelTypes.All);
            }

            HarmonyPatchAll();
            Logger.LogInfo("ChillaxScraps is loaded !");
        }
    }
}
