using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DeathNote : DarkBook
    {
        public DeathNote()
        {
            useCooldown = 2;
            usageOnServerMax = 5;
            rechargeTimeMin = 30;
            rechargeTimeMax = 60;
            musicToPlayID = 36;
            canvasPrefab = Plugin.gameObjects[0];
        }

        private void ClampUsageOnServerMax()
        {
            var nb = Effects.NbOfPlayers();
            if (nb < usageOnServerMax)
                usageOnServerMax = nb;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ClampUsageOnServerMax();
        }

        public override void ResetDeathNote()
        {
            base.ResetDeathNote();
            ClampUsageOnServerMax();
        }

        public override void SpecialEventOnServerUsage()
        {
            base.SpecialEventOnServerUsage();
            ClampUsageOnServerMax();
        }

        public override void ActivateDeathNote(GameObject objectToKill)
        {
            bool flag = true;
            CloseDeathNote();
            if (objectToKill.transform.TryGetComponent(out PlayerControllerB player) && player != null && !player.isPlayerDead && player.IsSpawned && player.isPlayerControlled)
            {
                KillPlayerDeathNoteServerRpc(player.playerClientId, player.OwnerClientId);
                if (playerHeldBy != null && Effects.IsUnlucky(playerHeldBy.playerSteamId) && Random.Range(0, 10) < 8)  // unlucky 80%
                    KillPlayerDeathNoteServerRpc(playerHeldBy.playerClientId, playerHeldBy.OwnerClientId);
            }
            else if (objectToKill.transform.TryGetComponent(out EnemyAI enemy) && enemy != null && !enemy.isEnemyDead && enemy.IsSpawned)
            {
                enemy.KillEnemyServerRpc(false);
            }
            else
                flag = false;
            if (flag)
            {
                canUseDeathNote = false;
                UsedServerRpc();
                SetControlTips();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void KillPlayerDeathNoteServerRpc(ulong playerId, ulong clientId)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            KillPlayerDeathNoteClientRpc(player.playerClientId, clientRpcParams);
        }

        [ClientRpc]
        private void KillPlayerDeathNoteClientRpc(ulong playerId, ClientRpcParams clientRpcParams = default)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            StartCoroutine(DeathNoteKill(player));
        }

        private IEnumerator DeathNoteKill(PlayerControllerB player)
        {
            if (player.IsOwner)
                Effects.Audio(1, 2.5f);
            yield return new WaitForSeconds(3);
            Effects.Damage(player, 100, CauseOfDeath.Unknown, (int)Effects.DeathAnimation.Haunted);
        }
    }
}
