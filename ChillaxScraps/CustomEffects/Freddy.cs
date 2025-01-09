using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Freddy : PhysicsProp
    {
        public int usage = 0;
        public int maxUsage = 0;
        public bool doom = false;
        public AudioSource? audio;
        public AudioSource? music;
        public BoxCollider? scanNode;
        public BoxCollider? grabArea;
        private int invisibilityChance = 70;

        public Freddy()
        {
            useCooldown = 1.5f;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            maxUsage = Random.Range(3, 6);
            audio = transform.GetChild(3).GetComponent<AudioSource>();
            music = transform.GetChild(2).GetComponent<AudioSource>();
            scanNode = transform.GetChild(0).GetComponent<BoxCollider>();
            grabArea = transform.GetComponent<BoxCollider>();
            invisibilityChance = Plugin.config.freddyInvisibilityChance.Value;
            if ((IsHost || IsServer) && transform.position.y < -80f)
            {
                StartCoroutine(StartBadThings());
            }
        }

        public override void GrabItem()
        {
            base.GrabItem();
            StopDoomServerRpc();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (audio == null || audio.isPlaying)
                return;
            if (StartOfRound.Instance.inShipPhase || usage <= maxUsage)
            {
                if (playerHeldBy != null && Effects.IsUnlucky(playerHeldBy.playerSteamId) && Random.Range(0, 10) < 8 && !StartOfRound.Instance.inShipPhase)
                    TheUnluckyServerRpc(playerHeldBy.transform.position);  // 80% unlucky effect
                else
                    AudioServerRpc(Random.Range(0, 10) <= 2 ? 50 : 48, 1f, 0);
                usage++;
            }
            else if (playerHeldBy != null)
            {
                var player = playerHeldBy;
                AudioServerRpc(49, 1.5f, 0);
                StartCoroutine(DamagePlayer(player, player.health <= 20 ? 100 : player.health - 10));
                Effects.DropItem(player.transform.position);
                maxUsage = Random.Range(3, 8);
                usage = 0;
            }
        }

        private IEnumerator DamagePlayer(PlayerControllerB player, int damage)
        {
            yield return new WaitForEndOfFrame();
            Effects.Damage(player, damage, CauseOfDeath.Mauling, (int)Effects.DeathAnimation.NoHead2);
        }

        private IEnumerator StartBadThings()
        {
            yield return new WaitForEndOfFrame();
            if (Random.Range(0, 100) <= invisibilityChance - 1)
            {
                bool freddyDoomTime = false;
                InvisibilityServerRpc(true);
                yield return new WaitForSeconds(1);
                while (true)  // wait for a player to be at least 30s inside the facility
                {
                    if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                        break;
                    var players = Effects.GetPlayers(excludeOutsideFactory: true);
                    if (players == null || players.Count == 0)
                    {
                        yield return new WaitForSeconds(10);
                        continue;
                    }
                    yield return new WaitForSeconds(30);
                    if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                        break;
                    players = Effects.GetPlayers(excludeOutsideFactory: true);
                    if (players == null || players.Count == 0)
                    {
                        continue;
                    }
                    freddyDoomTime = true;
                    break;
                }
                if (freddyDoomTime)
                {
                    AudioServerRpc(-1, 1, 1);  // start music
                    yield return new WaitForSeconds(10);
                    if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                        yield break;
                    InvisibilityServerRpc(false);
                    doom = true;
                    yield return new WaitForSeconds(45);
                    if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                        yield break;
                    if (doom)
                    {
                        var enemies = new SpawnableEnemyWithRarity[] { GetEnemies.BunkerSpider, GetEnemies.CoilHead, GetEnemies.Butler, GetEnemies.Nutcracker };
                        var position = Effects.GetClosestAINodePosition(RoundManager.Instance.insideAINodes, transform.position);
                        for (int i = 0; i < 5; i++)
                            Effects.Spawn(enemies[Random.Range(0, 4)], position);
                        doom = false;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void AudioServerRpc(int audioID, float volume, int sourceID)
        {
            AudioClientRpc(audioID, volume, sourceID);
        }

        [ClientRpc]
        private void AudioClientRpc(int audioID, float volume, int sourceID)
        {
            if (sourceID == 0 && audio != null)
                audio.PlayOneShot(Plugin.audioClips[audioID], volume);
            else if (sourceID == 1 && music != null)
                music.Play();
        }

        [ServerRpc(RequireOwnership = false)]
        private void InvisibilityServerRpc(bool enabled)
        {
            InvisibilityClientRpc(!enabled);
        }

        [ClientRpc]
        private void InvisibilityClientRpc(bool visibleFlag)
        {
            EnableItemMeshes(visibleFlag);
            if (scanNode != null)
                scanNode.enabled = visibleFlag;
            if (grabArea != null)
                grabArea.enabled = visibleFlag;
            grabbable = visibleFlag;
            grabbableToEnemies = visibleFlag;
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopDoomServerRpc()
        {
            if (doom)
            {
                StopDoomClientRpc();
                doom = false;
            }
        }

        [ClientRpc]
        private void StopDoomClientRpc()
        {
            if (music != null)
                music.Stop();
        }

        [ServerRpc(RequireOwnership = false)]
        private void TheUnluckyServerRpc(Vector3 position)
        {
            Effects.Spawn(GetEnemies.Landmine, position);
            for (int i = 0; i < 4; i++)
                Effects.Spawn(GetEnemies.Turret, position, i * 90);
        }
    }
}
