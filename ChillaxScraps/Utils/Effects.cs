using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.Utils
{
    internal class Effects
    {
        public enum DeathAnimation
        {
            Normal,  // classic death
            NoHead1,  // remove head from body
            Spring,  // remove head and replace it with spring
            Haunted,  // body moves a little after classic death
            Mask1,  // comedy mask attached to body
            Mask2,  // tragedy mask attached to body
            Fire,  // burned death
            CutInHalf,  // cut the body in half
            NoHead2  // same as NoHead but without sound
        }

        public static int NbOfPlayers()
        {
            return StartOfRound.Instance.connectedPlayersAmount + 1;
        }

        public static bool IsUnlucky(ulong playerId)
        {
            if (Plugin.config.unluckyPlayersID.Count == 0)
                return false;
            return Plugin.config.unluckyPlayersID.Find(id => id == playerId) != default;
        }

        public static List<PlayerControllerB> GetPlayers(bool includeDead = false, bool excludeOutsideFactory = false)
        {
            List<PlayerControllerB> rawList = StartOfRound.Instance.allPlayerScripts.ToList();
            List<PlayerControllerB> updatedList = new List<PlayerControllerB>(rawList);
            foreach (var p in rawList)
            {
                if (!p.IsSpawned || !p.isPlayerControlled || (!includeDead && p.isPlayerDead) || (excludeOutsideFactory && !p.isInsideFactory))
                {
                    updatedList.Remove(p);
                }
            }
            return updatedList;
        }

        public static List<EnemyAI> GetEnemies(bool includeDead = false, bool includeImmortal = false, bool excludeDaytime = false, List<string>? keepSpecificEnemiesList = null)
        {
            List<EnemyAI> rawList = Object.FindObjectsOfType<EnemyAI>().ToList();
            List<EnemyAI> updatedList = new List<EnemyAI>(rawList);
            if (includeDead)
                return updatedList;
            foreach (var e in rawList)
            {
                if (!e.IsSpawned || e.isEnemyDead || (!includeImmortal && !e.enemyType.canDie) || (excludeDaytime && e.enemyType.isDaytimeEnemy))
                {
                    if (keepSpecificEnemiesList == null || keepSpecificEnemiesList.Count() == 0 || !keepSpecificEnemiesList.Contains(e.enemyType.enemyName))
                        updatedList.Remove(e);
                }
            }
            return updatedList;
        }

        public static void Damage(PlayerControllerB player, int damageNb, CauseOfDeath cause = 0, int animation = 0, bool criticalBlood = true)
        {
            damageNb = player.health > 100 && damageNb == 100 ? 900 : damageNb;
            if (criticalBlood && player.health - damageNb <= 20)
                player.bleedingHeavily = true;
            player.DamagePlayer(damageNb, causeOfDeath: cause, deathAnimation: animation);
        }

        public static IEnumerator DamageHost(PlayerControllerB player, int damageNb, CauseOfDeath cause = 0, int animation = 0, bool criticalBlood = true)
        {
            yield return new WaitForEndOfFrame();
            Damage(player, damageNb, cause, animation, criticalBlood);
        }

        public static void Heal(ulong playerID, int health)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerID];
            player.health = player.health > 100 ? player.health : health;
            player.criticallyInjured = false;
            player.bleedingHeavily = false;
            player.playerBodyAnimator.SetBool("Limp", false);
        }

        public static void Teleportation(PlayerControllerB player, Vector3 position)
        {
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(position, true);
            player.beamOutParticle.Play();
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }

        public static void SetPosFlags(ulong playerID, bool ship = false, bool exterior = false, bool interior = false)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerID];
            if (ship)
            {
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
            }
            if (exterior)
            {
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = false;
            }
            if (interior)
            {
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = true;
            }
            foreach (var item in player.ItemSlots)
            {
                if (item != null)
                {
                    item.isInFactory = player.isInsideFactory;
                    item.isInElevator = player.isInElevator;
                    item.isInShipRoom = player.isInHangarShipRoom;
                }
            }
            if (GameNetworkManager.Instance.localPlayerController.playerClientId == player.playerClientId)
            {
                if (player.isInsideFactory)
                    TimeOfDay.Instance.DisableAllWeather();
                else
                    ActivateWeatherEffect();
            }
        }

        public static bool IsPlayerFacingObject<T>(PlayerControllerB player, out T obj, float distance)
        {
            if (Physics.Raycast(new Ray(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward), out var hitInfo, distance, 2816))
            {
                obj = hitInfo.transform.GetComponent<T>();
                if (obj != null)
                    return true;
            }
            obj = default;
            return false;
        }

        public static bool IsPlayerNearObject<T>(PlayerControllerB player, out T obj, float distance) where T : Component
        {
            T[] array = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            for (int i = 0; i < array.Length; i++)
            {
                if (Vector3.Distance(player.transform.position, array[i].transform.position) <= distance)
                {
                    obj = array[i];
                    return true;
                }
            }
            obj = default;
            return false;
        }

        public static Vector3 GetClosestAINodePosition(GameObject[] nodes, Vector3 position)
        {
            return nodes.OrderBy((GameObject x) => Vector3.Distance(position, x.transform.position)).ToArray()[0].transform.position;
        }

        public static void Knockback(Vector3 position, float range, int damage = 0, float physicsForce = 30)
        {
            Landmine.SpawnExplosion(position, false, 0, range, damage, physicsForce);
        }

        public static void DropItem(Vector3 placingPosition = default)
        {
            GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, placePosition: placingPosition);
        }

        public static void Audio(int audioID, float volume)
        {
            RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, new AudioClip[] { Plugin.audioClips[audioID] }, randomize: false, oneShotVolume: volume);
        }

        public static void Audio(int audioID, Vector3 startPosition, float localVolume, float clientVolume, PlayerControllerB player)
        {
            if (player != null && GameNetworkManager.Instance.localPlayerController.playerClientId == player.playerClientId)
                Audio(audioID, localVolume);
            else if (player != null)
                player.itemAudio.PlayOneShot(Plugin.audioClips[audioID], clientVolume);
            else
                AudioSource.PlayClipAtPoint(Plugin.audioClips[audioID], startPosition + (Vector3.up * 2), clientVolume);
        }

        public static void Audio3D(int audioID, Vector3 position, float volume = 1f, float distance = 20f)
        {
            GameObject gameObject = new GameObject("One shot audio");
            gameObject.transform.position = position;
            AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0;
            audioSource.maxDistance = distance;
            audioSource.PlayOneShot(Plugin.audioClips[audioID], volume);
            Object.Destroy(gameObject, Plugin.audioClips[audioID].length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }

        public static IEnumerator FadeOutAudio(AudioSource source, float time)
        {
            yield return new WaitForEndOfFrame();
            var volume = source.volume;
            while (source.volume > 0)
            {
                source.volume -= volume * Time.deltaTime / time;
                yield return null;
            }
            source.Stop();
            source.volume = volume;
        }

        public static void Message(string title, string bottom, bool warning = false)
        {
            HUDManager.Instance.DisplayTip(title, bottom, warning);
        }

        public static void ChangeWeather(LevelWeatherType weather)
        {
            var original = StartOfRound.Instance.currentLevel.currentWeather;
            StartOfRound.Instance.currentLevel.currentWeather = weather;
            if (Plugin.config.WeatherRegistery)
            {
                ChangeWeatherWR(weather);
                return;
            }
            RoundManager.Instance.SetToCurrentLevelWeather();
            TimeOfDay.Instance.SetWeatherBasedOnVariables();
            if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
                return;
            ActivateWeatherEffect(original);
        }

        public static void ActivateWeatherEffect(LevelWeatherType originalWeather = default)
        {
            for (var i = 0; i < TimeOfDay.Instance.effects.Length; i++)
            {
                var effect = TimeOfDay.Instance.effects[i];
                var enabled = (int)StartOfRound.Instance.currentLevel.currentWeather == i;
                effect.effectEnabled = enabled;
                if (effect.effectPermanentObject != null)
                    effect.effectPermanentObject.SetActive(enabled);
                if (effect.effectObject != null)
                    effect.effectObject.SetActive(enabled);
                if (TimeOfDay.Instance.sunAnimator != null)
                {
                    if (enabled && !string.IsNullOrEmpty(effect.sunAnimatorBool))
                        TimeOfDay.Instance.sunAnimator.SetBool(effect.sunAnimatorBool, true);
                    else
                    {
                        TimeOfDay.Instance.sunAnimator.Rebind();
                        TimeOfDay.Instance.sunAnimator.Update(0);
                    }
                }
            }
            if (originalWeather == LevelWeatherType.Flooded)
            {
                var player = GameNetworkManager.Instance.localPlayerController;
                player.isUnderwater = false;
                player.sourcesCausingSinking = Mathf.Clamp(player.sourcesCausingSinking - 1, 0, 100);
                player.isMovementHindered = Mathf.Clamp(player.isMovementHindered - 1, 0, 100);
                player.hinderedMultiplier = 1f;
            }
        }

        public static void ChangeWeatherWR(LevelWeatherType weather)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsHost)
                WeatherRegistry.WeatherController.SetWeatherEffects(weather);
        }

        public static void AddCombinedWeatherWR(string weatherNameResolvable)
        {
            AddCombinedWeatherWR(new WeatherRegistry.WeatherNameResolvable(weatherNameResolvable).WeatherType);
        }

        public static void AddCombinedWeatherWR(LevelWeatherType weather)
        {
            WeatherRegistry.WeatherController.AddWeatherEffect(weather);
            //WeatherRegistry.Patches.SunAnimator.OverrideSunAnimator(weather);
        }

        public static bool IsWeatherEffectPresent(string weatherNameResolvable)
        {
            if (Plugin.config.WeatherRegistery)
                return IsWeatherEffectPresentWR(weatherNameResolvable);
            return false;
        }

        public static bool IsWeatherEffectPresent(LevelWeatherType weatherType)
        {
            if (Plugin.config.WeatherRegistery)
                return IsWeatherEffectPresentWR(weatherType);
            return StartOfRound.Instance.currentLevel.currentWeather == weatherType;
        }

        public static bool IsWeatherEffectPresentWR(string weatherNameResolvable)
        {
            return IsWeatherEffectPresentWR(new WeatherRegistry.WeatherNameResolvable(weatherNameResolvable).WeatherType);
        }

        public static bool IsWeatherEffectPresentWR(LevelWeatherType weatherType)
        {
            for (int i = 0; i < WeatherRegistry.WeatherManager.CurrentEffectTypes.Count; i++)
            {
                if (WeatherRegistry.WeatherManager.CurrentEffectTypes[i] == weatherType)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsMajoraActiveAndFinalHours()
        {
            if (!Plugin.config.WeatherRegistery || !IsWeatherEffectPresent("majoramoon"))
                return false;
            return IsMajoraFinalHours();
        }

        public static bool IsMajoraFinalHours()
        {
            var majoraMoon = Object.FindObjectOfType<LegendWeathers.Weathers.MajoraMoon>();
            if (majoraMoon == null)
                return false;
            return majoraMoon.finalHoursDisplayingTimer && !majoraMoon.finalHoursFinishing && !majoraMoon.oathToOrderStopingMoon;
        }

        public static void StopMajora()
        {
            var majoraMoon = Object.FindObjectOfType<LegendWeathers.Weathers.MajoraMoon>();
            if (majoraMoon == null)
                return;
            majoraMoon.StopMoonCrash();
        }

        public static void ReplaceRedwoodSfxCR(GameObject enemy)
        {
            var giantAI = enemy.GetComponent<CodeRebirth.src.Content.Enemies.RedwoodTitanAI>();
            giantAI.roarSound = null;
            giantAI.spawnSound = Plugin.audioClips[23];
            giantAI.eatenSound = Plugin.audioClips[24];
        }

        public static NetworkObjectReference Spawn(SpawnableEnemyWithRarity enemy, Vector3 position, float yRot = 0f)
        {
            GameObject gameObject = Object.Instantiate(enemy.enemyType.enemyPrefab, position, Quaternion.Euler(new Vector3(0f, yRot, 0f)));
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            return new NetworkObjectReference(gameObject);
        }

        public static void SpawnMaskedOfPlayer(ulong playerId, Vector3 position)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            bool flag = player.transform.position.y < -80f;
            var netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(position, player.transform.eulerAngles.y, -1, Utils.GetEnemies.Masked.enemyType);
            if (netObjectRef.TryGet(out var networkObject))
            {
                var component = networkObject.GetComponent<MaskedPlayerEnemy>();
                component.SetSuit(player.currentSuitID);
                component.mimickingPlayer = player;
                component.SetEnemyOutside(!flag);
                component.CreateMimicClientRpc(netObjectRef, flag, (int)playerId);
            }
        }

        public static void Spawn(SpawnableMapObject trap, Vector3 position, float yRot = 0f)
        {
            GameObject gameObject = Object.Instantiate(trap.prefabToSpawn, position, Quaternion.Euler(new Vector3(0f, yRot, 0f)), RoundManager.Instance.mapPropsContainer.transform);
            gameObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public static SpawnableItemWithRarity GetScrap(string scrapName)
        {
            return RoundManager.Instance.currentLevel.spawnableScrap.FirstOrDefault(i => i.spawnableItem.name.Equals(scrapName));
        }

        public static NetworkReference Spawn(SpawnableItemWithRarity scrap, Vector3 position)
        {
            var parent = RoundManager.Instance.spawnedScrapContainer == null ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer;
            GameObject gameObject = Object.Instantiate(scrap.spawnableItem.spawnPrefab, position + Vector3.up * 0.25f, Quaternion.identity, parent);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.fallTime = 0f;
            component.scrapValue = (int)(Random.Range(scrap.spawnableItem.minValue, scrap.spawnableItem.maxValue) * RoundManager.Instance.scrapValueMultiplier);
            component.NetworkObject.Spawn();
            component.FallToGround(true);
            return new NetworkReference(gameObject.GetComponent<NetworkObject>(), component.scrapValue);
        }

        public static IEnumerator SyncScrap(NetworkReference reference)
        {
            yield return new WaitForSeconds(3f);
            RoundManager.Instance.SyncScrapValuesClientRpc(new NetworkObjectReference[] { reference.netObjectRef }, new int[] { reference.value });
        }

        // Modified from Mrov's version
        public static void SpawnLightningBolt(Vector3 strikePosition, bool damage = true, bool redirectInside = true)
        {
            LightningBoltPrefabScript localLightningBoltPrefabScript;
            var random = new System.Random(StartOfRound.Instance.randomMapSeed);
            random.Next(-32, 32); random.Next(-32, 32);
            var vector = strikePosition + Vector3.up * 160f + new Vector3(random.Next(-32, 32), 0f, random.Next(-32, 32));
            if (redirectInside && Physics.Linecast(vector, strikePosition + Vector3.up * 0.5f, out _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                if (!Physics.Raycast(vector, strikePosition - vector, out var rayHit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    return;
                strikePosition = rayHit.point;
            }
            StormyWeather stormy = Object.FindObjectOfType<StormyWeather>(true);
            localLightningBoltPrefabScript = Object.Instantiate(stormy.targetedThunder);
            localLightningBoltPrefabScript.enabled = true;
            localLightningBoltPrefabScript.Camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
            localLightningBoltPrefabScript.AutomaticModeSeconds = 0.2f;
            localLightningBoltPrefabScript.Source.transform.position = vector;
            localLightningBoltPrefabScript.Destination.transform.position = strikePosition;
            localLightningBoltPrefabScript.CreateLightningBoltsNow();
            AudioSource audioSource = Object.Instantiate(stormy.targetedStrikeAudio);
            audioSource.transform.position = strikePosition + Vector3.up * 0.5f;
            audioSource.enabled = true;
            if (damage)
                Landmine.SpawnExplosion(strikePosition + Vector3.up * 0.25f, spawnExplosionEffect: false, 2.4f, 5f);
            stormy.PlayThunderEffects(strikePosition, audioSource);
        }
    }
}
