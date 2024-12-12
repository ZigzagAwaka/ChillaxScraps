using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DanceNote : DarkBook
    {
        public bool danceActivated = false;
        public float transitionTime = 3.5f;
        public float danceTime = 6.5f;
        public int usage = 0;
        public readonly GameObject warningPrefab;
        public readonly GameObject glowPrefab;
        public readonly GameObject glowboomPrefab;
        public int[] musicClips = new int[] { 38, 39, 40, 41, 42, 43 };
        private GameObject? glowObj;

        public DanceNote()
        {
            useCooldown = 2;
            canKillEnemies = false;
            punishInOrbit = false;
            musicToPlayID = 37;
            canvasPrefab = Plugin.gameObjects[3];
            warningPrefab = Plugin.gameObjects[4];
            glowPrefab = Plugin.gameObjects[5];
            glowboomPrefab = Plugin.gameObjects[6];
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost || IsServer)
            {
                var rnd = new System.Random();
                musicClips = musicClips.Select(i => (i, rnd.Next()))
                    .OrderBy(tuple => tuple.Item2).Select(tuple => tuple.i).ToArray();
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (!danceActivated)
                base.ItemActivate(used, buttonDown);
        }

        public override void ActivateDeathNote(GameObject objectToKill)
        {
            bool flag = true;
            CloseDeathNote();
            if (objectToKill.transform.TryGetComponent(out PlayerControllerB player) && player != null && !player.isPlayerDead && player.IsSpawned && player.isPlayerControlled)
            {
                StartDancingServerRpc(player.playerClientId, player.OwnerClientId, usage);
            }
            else
                flag = false;
            if (flag)
            {
                UpdateUsageServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateUsageServerRpc()
        {
            UpdateUsageClientRpc();
        }

        [ClientRpc]
        private void UpdateUsageClientRpc()
        {
            usage++;
            if (usage == musicClips.Length)
            {
                canUseDeathNote = false;
                SetControlTips();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartDancingServerRpc(ulong playerId, ulong clientId, int musicID)
        {
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            StartMusicClientRpc(playerId, musicClips[musicID]);
            StartDancingClientRpc(playerId, clientRpcParams);
        }

        [ClientRpc]
        private void StartMusicClientRpc(ulong playerId, int realMusicID)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            danceActivated = true;
            if (player.itemAudio.isPlaying)
                player.itemAudio.Stop();
            player.itemAudio.PlayOneShot(Plugin.audioClips[realMusicID], 1.2f);
            glowObj = Instantiate(glowPrefab, player.transform.position + Vector3.up, Quaternion.identity, player.transform);
            Destroy(glowObj.gameObject, transitionTime + danceTime);
        }

        [ClientRpc]
        private void StartDancingClientRpc(ulong playerId, ClientRpcParams clientRpcParams = default)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            StartCoroutine(DanceOrDie(player));
        }

        private IEnumerator DanceOrDie(PlayerControllerB player)
        {
            var warning = Instantiate(warningPrefab);
            Destroy(warning, transitionTime + 0.5f);
            yield return new WaitForSeconds(transitionTime);
            var actualTime = 0f;
            while (true)
            {
                if (player == null || player.isPlayerDead || actualTime >= danceTime)
                    break;
                if (!player.performingEmote)
                {
                    KillInAreaDanceNoteServerRpc(player.playerClientId, player.transform.position);
                    break;
                }
                yield return new WaitForSeconds(0.1f);
                actualTime += 0.1f;
            }
            EndDanceServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void KillInAreaDanceNoteServerRpc(ulong playerId, Vector3 position)
        {
            KillInAreaDanceNoteClientRpc(playerId, position);
        }

        [ClientRpc]
        private void KillInAreaDanceNoteClientRpc(ulong playerId, Vector3 position)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            StartCoroutine(WaitValidationThenBoom(player, position));
        }

        private IEnumerator WaitValidationThenBoom(PlayerControllerB player, Vector3 position)
        {
            while (!player.grabbedObjectValidated)
            {
                yield return new WaitForEndOfFrame();
            }
            if (player.itemAudio.isPlaying)
                player.itemAudio.Stop();
            if (glowObj != null)
                Destroy(glowObj.gameObject);
            Instantiate(glowboomPrefab, position, Quaternion.identity);
            Landmine.SpawnExplosion(position + Vector3.up * 0.25f, false, 4.5f, 6f, 70, 5f);
        }

        [ServerRpc(RequireOwnership = false)]
        private void EndDanceServerRpc()
        {
            EndDanceClientRpc();
        }

        [ClientRpc]
        private void EndDanceClientRpc()
        {
            danceActivated = false;
        }
    }
}
