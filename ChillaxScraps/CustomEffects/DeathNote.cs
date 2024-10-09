using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DeathNote : PhysicsProp
    {
        public bool canUseDeathNote = true;
        public bool isOpened = false;
        public List<PlayerControllerB> playerList;
        public List<EnemyAI> enemyList;
        private DeathNoteCanvas canvas;
        private readonly GameObject canvasPrefab;

        public DeathNote()
        {
            useCooldown = 2;
            canvasPrefab = Plugin.gameObjects[0];
        }

        [ServerRpc(RequireOwnership = false)]
        public void AudioServerRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume = default)
        {
            AudioClientRpc(audioID, clientPosition, hostVolume, clientVolume == default ? hostVolume : clientVolume);
        }

        [ClientRpc]
        public void AudioClientRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume)
        {
            if (IsHost)
                Effects.Audio(audioID, hostVolume);
            else
                Effects.Audio(audioID, clientPosition, clientVolume);
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

        public override void GrabItem()
        {
            base.GrabItem();
            if (canUseDeathNote && !isOpened && itemProperties.toolTips[0] == "")
            {
                itemProperties.toolTips[0] = "Write a name : [RMB]";
                base.SetControlTipsForItem();
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && canUseDeathNote && !isOpened
                && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.shipHasLanded)
            {
                AudioServerRpc(0, playerHeldBy.transform.position, 1f);  // page audio
                playerList = Effects.GetPlayers();
                enemyList = Effects.GetEnemies();
                canvas = Instantiate(canvasPrefab, transform).GetComponent<DeathNoteCanvas>();
                canvas.Initialize(this);  // open death note
                isOpened = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                canvas.onExit += CloseDeathNote;
            }
            else if (buttonDown && playerHeldBy != null && canUseDeathNote && !isOpened)
                Effects.Message("Can't be used at the moment", "");
        }

        private void CloseDeathNote()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            isOpened = false;
        }

        public override void PocketItem()
        {
            base.PocketItem();
            if (canvas != null)
                canvas.Close();
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            if (canvas != null)
                canvas.Close();
        }

        public void ActivateDeathNote(GameObject objectToKill)
        {
            bool flag = true;
            CloseDeathNote();
            if (objectToKill.transform.TryGetComponent(out PlayerControllerB player))
            {
                KillPlayerDeathNoteServerRpc(player.playerClientId, player.OwnerClientId);
            }
            else if (objectToKill.transform.TryGetComponent(out EnemyAI enemy))
            {
                enemy.KillEnemyServerRpc(false);
            }
            else
                flag = false;
            if (flag)
            {
                canUseDeathNote = false;
                itemProperties.toolTips[0] = "";
                base.SetControlTipsForItem();
            }
        }
    }
}
