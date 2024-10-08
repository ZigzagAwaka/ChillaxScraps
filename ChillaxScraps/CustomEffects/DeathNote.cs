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
        private GameObject canvasPrefab;  // to be loaded with asset.spawnPrefab

        public DeathNote()
        {
            useCooldown = 2;
        }

        [ServerRpc(RequireOwnership = false)]
        private void AudioServerRpc()
        {
            AudioClientRpc();
        }

        [ClientRpc]
        private void AudioClientRpc()
        {
            if (IsHost)
                Effects.Audio(0, 1f);
            else
                Effects.Audio(0, playerHeldBy.transform.position, 1f);
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
                Effects.Audio(1, 3f);
            yield return new WaitForSeconds(3);
            Effects.Damage(player, 100, CauseOfDeath.Unknown, (int)Effects.DeathAnimation.Haunted);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && canUseDeathNote && !isOpened
                && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.shipHasLanded)
            {
                AudioServerRpc();  // page audio
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
            CloseDeathNote();
            if (objectToKill.transform.TryGetComponent(out IHittable component))
            {
                if (component.GetType() == typeof(PlayerControllerB))
                {
                    KillPlayerDeathNoteServerRpc(((PlayerControllerB)component).playerClientId, ((PlayerControllerB)component).OwnerClientId);
                }
                else if (component.GetType() == typeof(EnemyAI))
                {
                    ((EnemyAI)component).KillEnemyServerRpc(false);
                }
                canUseDeathNote = false;
                itemProperties.toolTips[0] = "";
                base.SetControlTipsForItem();
            }
        }
    }
}
