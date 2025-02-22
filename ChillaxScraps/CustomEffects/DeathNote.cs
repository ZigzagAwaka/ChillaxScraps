using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DeathNote : DarkBook
    {
        public bool noLimit;

        public DeathNote()
        {
            useCooldown = 2;
            canRechargeInOrbit = Plugin.config.deathnoteRechargeOrbit.Value;
            noLimit = Plugin.config.deathnoteNoLimit.Value;
            punishInOrbit = !noLimit;
            usageOnServerMax = noLimit ? 3 : 5;
            rechargeTimeMin = noLimit ? 120 : 30;
            rechargeTimeMax = noLimit ? 130 : 60;
            musicToPlayID = 36;
            canvasPrefab = Plugin.gameObjects[0];
        }

        private void ClampUsageOnServerMax()
        {
            if (noLimit)
                return;
            var nb = Effects.NbOfPlayers();
            if (nb < usageOnServerMax || nb <= 5)
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
                if (!noLimit)
                    canUseDeathNote = false;
                UsedServerRpc();
                SetControlTips();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void KillPlayerDeathNoteServerRpc(ulong playerId, ulong clientId)
        {
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            KillPlayerDeathNoteClientRpc(playerId, clientRpcParams);
        }

        [ClientRpc]
        private void KillPlayerDeathNoteClientRpc(ulong playerId, ClientRpcParams clientRpcParams = default)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerId];
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
