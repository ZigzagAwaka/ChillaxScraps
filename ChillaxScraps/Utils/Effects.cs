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

        public static List<PlayerControllerB> GetPlayers(bool includeDead = false, bool excludeOutsideFactory = false)
        {
            List<PlayerControllerB> rawList = Object.FindObjectsOfType<PlayerControllerB>().ToList();
            List<PlayerControllerB> updatedList = new List<PlayerControllerB>(rawList);
            foreach (var p in rawList)
            {
                if (p.playerSteamId <= 0 || !p.IsSpawned || !p.isPlayerControlled || (!includeDead && p.isPlayerDead) || (excludeOutsideFactory && !p.isInsideFactory))
                {
                    updatedList.Remove(p);
                }
            }
            return updatedList;
        }

        public static List<EnemyAI> GetEnemies(bool includeDead = false, bool includeCanDie = false, bool excludeDaytime = false)
        {
            List<EnemyAI> rawList = Object.FindObjectsOfType<EnemyAI>().ToList();
            List<EnemyAI> updatedList = new List<EnemyAI>(rawList);
            if (includeDead)
                return updatedList;
            foreach (var e in rawList)
            {
                if (!e.IsSpawned || e.isEnemyDead || (!includeCanDie && !e.enemyType.canDie) || (excludeDaytime && e.enemyType.isDaytimeEnemy))
                {
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

        public static void Teleportation(PlayerControllerB player, Vector3 position, bool ship = false, bool exterior = false, bool interior = false)
        {
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
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(position, true);
            player.beamOutParticle.Play();
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }

        public static bool IsPlayerFacingDoor(PlayerControllerB player, out DoorLock? doorLock)
        {
            if (Physics.Raycast(new Ray(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward), out var hitInfo, 3f, 2816))
            {
                doorLock = hitInfo.transform.GetComponent<DoorLock>();
                if (doorLock != null)
                    return true;
            }
            doorLock = null;
            return false;
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

        public static void Audio(int audioID, Vector3 position, float volume, bool adjust = true)
        {
            var finalPosition = position;
            if (adjust)
                finalPosition += (Vector3.up * 2);
            AudioSource.PlayClipAtPoint(Plugin.audioClips[audioID], finalPosition, volume);
        }

        public static void Audio(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume, PlayerControllerB player)
        {
            if (player != null && GameNetworkManager.Instance.localPlayerController.playerClientId == player.playerClientId)
                Audio(audioID, hostVolume);
            else
                Audio(audioID, clientPosition, clientVolume);
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

        public static void Spawn(SpawnableEnemyWithRarity enemy, Vector3 position)
        {
            GameObject gameObject = Object.Instantiate(enemy.enemyType.enemyPrefab, position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
        }

        public static void Spawn(SpawnableMapObject trap, Vector3 position)
        {
            GameObject gameObject = Object.Instantiate(trap.prefabToSpawn, position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
            gameObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public static SpawnableItemWithRarity GetScrap(string scrapName)
        {
            return RoundManager.Instance.currentLevel.spawnableScrap.FirstOrDefault(i => i.spawnableItem.name.Equals(scrapName));
        }

        public static ScrapReference Spawn(SpawnableItemWithRarity scrap, Vector3 position)
        {
            var parent = RoundManager.Instance.spawnedScrapContainer == null ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer;
            GameObject gameObject = Object.Instantiate(scrap.spawnableItem.spawnPrefab, position + Vector3.up * 0.25f, Quaternion.identity, parent);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.fallTime = 0f;
            component.scrapValue = (int)(Random.Range(scrap.spawnableItem.minValue, scrap.spawnableItem.maxValue) * RoundManager.Instance.scrapValueMultiplier);
            component.NetworkObject.Spawn();
            component.FallToGround(true);
            return new ScrapReference(gameObject.GetComponent<NetworkObject>(), component.scrapValue);
        }

        public static IEnumerator SyncScrap(ScrapReference reference)
        {
            yield return new WaitForSeconds(3f);
            RoundManager.Instance.SyncScrapValuesClientRpc(new NetworkObjectReference[] { reference.netObjectRef }, new int[] { reference.scrapValue });
        }
    }
}
