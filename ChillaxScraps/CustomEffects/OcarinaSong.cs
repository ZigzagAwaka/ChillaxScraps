using ChillaxScraps.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal enum Condition
    {
        None,
        Invalid,
        IsPlayerInsideFactory,
        IsPlayerOutsideFactory,
        IsPlayerFacingDoor,
        IsTimeAfternoon,
        IsTimeNight,
        IsOutsideTimeNight,
        IsOutsideWeatherNotStormy,
        IsOutsideWeatherStormy,
        IsPlayerNearOldBirdNest,
        IsPlayerInShip,
        IsInsideTimeAfternoon,
        IsOutsideAtLeastOneBaboonSpawned,
        IsPlayerInShipSpeakerNotPlaying,
        IsPlayerNotInShip
    }

    internal class OcarinaSong
    {
        public string title;
        public Color color;
        public static float colorMultiplicator = 2f;
        public System.Func<Ocarina, PlayerControllerB, int, bool>? effect;
        public int usage = 0;
        public int maxUsage;
        public Condition[] conditions;
        public bool canBeUsedInOrbit = false;

        public OcarinaSong(string title, Color color, System.Func<Ocarina, PlayerControllerB, int, bool>? effect,
                            int maxUsage, params Condition[] conditions)
        {
            this.title = title;
            this.color = color;
            this.effect = effect;
            this.maxUsage = maxUsage;
            this.conditions = conditions;
        }

        internal class ConditionComponents
        {
            public DoorLock? door;
            public EnemyAINestSpawnObject? birdNest;
        }

        private static bool Verif(Condition condition, PlayerControllerB player)
        {
            return Verif(condition, player, out _);
        }

        private static bool Verif(Condition condition, PlayerControllerB player, out ConditionComponents components)
        {
            components = new ConditionComponents();
            return condition switch
            {
                Condition.None => true,
                Condition.Invalid => false,
                Condition.IsPlayerInsideFactory => player.isInsideFactory,
                Condition.IsPlayerOutsideFactory => !player.isInsideFactory,
                Condition.IsPlayerFacingDoor => Effects.IsPlayerFacingObject<DoorLock>(player, out components.door, 3f) && components.door.isLocked && !components.door.isPickingLock,
                Condition.IsTimeAfternoon => TimeOfDay.Instance.currentDayTime >= 360,
                Condition.IsTimeNight => TimeOfDay.Instance.currentDayTime >= 720,
                Condition.IsOutsideTimeNight => !player.isInsideFactory && TimeOfDay.Instance.currentDayTime >= 720,
                Condition.IsOutsideWeatherNotStormy => !player.isInsideFactory && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Stormy,
                Condition.IsOutsideWeatherStormy => !player.isInsideFactory && StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Stormy,
                Condition.IsPlayerNearOldBirdNest => Effects.IsPlayerNearObject<EnemyAINestSpawnObject>(player, out components.birdNest, 10f) && components.birdNest.enemyType == GetEnemies.OldBird.enemyType,
                Condition.IsPlayerInShip => player.isInElevator && player.isInHangarShipRoom,
                Condition.IsInsideTimeAfternoon => player.isInsideFactory && TimeOfDay.Instance.currentDayTime >= 360,
                Condition.IsOutsideAtLeastOneBaboonSpawned => !player.isInsideFactory && Effects.IsPlayerNearObject<BaboonBirdAI>(player, out _, 1000f),
                Condition.IsPlayerInShipSpeakerNotPlaying => player.isInElevator && player.isInHangarShipRoom && !StartOfRound.Instance.speakerAudioSource.isPlaying,
                Condition.IsPlayerNotInShip => !player.isInElevator && !player.isInHangarShipRoom,
                _ => false
            };
        }

        public (bool, int) IsValid(PlayerControllerB player, bool isOcarinaRestricted)
        {
            var result = (false, 0);
            if (player != null && (!StartOfRound.Instance.inShipPhase || canBeUsedInOrbit) && (usage < maxUsage || !isOcarinaRestricted))
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    result = (Verif(conditions[i], player), i);
                    if (result.Item1 == true)
                        return result;
                }
            }
            return result;
        }

        public bool StartEffect(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (ocarina == null || player == null || effect == null)
                return false;
            return effect(ocarina, player, variationId);
        }

        public static bool ZeldaLullaby(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsPlayerFacingDoor, player, out var components))
                    ocarina.StartCoroutine(ocarina.OpenDoorZeldaStyle(components.door));
                else
                    return false;
            }
            else
            {
                if (Verif(Condition.IsTimeAfternoon, player))
                    ocarina.StartCoroutine(ocarina.TeleportTo(player, StartOfRound.Instance.middleOfShipNode.position, false));
                else
                    return false;
            }
            return true;
        }

        public static bool EponaSong(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsPlayerOutsideFactory, player))
                    ocarina.StartCoroutine(ocarina.SpawnZeldaEnemy(0, player.transform.position));
                else
                    return false;
            }
            return true;
        }

        public static bool SunSong(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                ocarina.ChargeAllItemsServerRpc();
            }
            return true;
        }

        public static bool SariaSong(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                var position = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length - 1)].transform.position;
                ocarina.StartCoroutine(ocarina.TeleportTo(player, position, true));
            }
            return true;
        }

        public static bool SongOfTime(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsTimeNight, player))
                    ocarina.ChangeTimeServerRpc(TimeOfDay.Instance.currentDayTime - 120);
                else
                    return false;
            }
            return true;
        }

        public static bool SongOfStorms(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsOutsideWeatherNotStormy, player))
                    ocarina.ChangeWeatherServerRpc(LevelWeatherType.Stormy);
                else
                    return false;
            }
            else
            {
                if (Verif(Condition.IsOutsideWeatherStormy, player))
                    ocarina.StartCoroutine(ocarina.SpawnSuperStormy());
                else
                    return false;
            }
            return true;
        }

        public static bool SongOfHealing(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                ocarina.HealPlayersInAreaServerRpc(player.transform.position);
            }
            return true;
        }

        public static bool SongOfSoaring(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsPlayerNotInShip, player))
                    ocarina.StartCoroutine(ocarina.SoaringEffect(player));
                else
                    return false;
            }
            return true;
        }

        public static bool SonataOfAwakening(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsPlayerNearOldBirdNest, player, out var components))
                    ocarina.StartCoroutine(ocarina.WakeTheBird(components.birdNest));
                else
                    return false;
            }
            return true;
        }

        public static bool GoronLullaby(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsOutsideAtLeastOneBaboonSpawned, player))
                    ocarina.StartCoroutine(ocarina.MakeAllBaboonSleep());
                else
                    return false;
            }
            else
            {
                if (Verif(Condition.IsInsideTimeAfternoon, player))
                    ocarina.StartCoroutine(ocarina.SpawnZeldaEnemy(3, player.transform.position));
                else
                    return false;
            }
            return true;
        }

        public static bool NewWaveBossaNova(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsPlayerInShipSpeakerNotPlaying, player))
                    ocarina.PlayMusicInShipServerRpc(33);
                else
                    return false;
            }
            return true;
        }

        public static bool ElegyOfEmptiness(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                ocarina.StartCoroutine(ocarina.SpawnZeldaEnemy(2, player.transform.position, player.playerClientId));
            }
            return true;
        }

        public static bool OathToOrder(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Verif(Condition.IsOutsideTimeNight, player))
                    ocarina.StartCoroutine(ocarina.SpawnZeldaEnemy(1, player.transform.position));
                else
                    return false;
            }
            return true;
        }
    }
}
