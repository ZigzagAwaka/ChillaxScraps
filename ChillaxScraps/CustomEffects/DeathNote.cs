﻿using ChillaxScraps.Utils;
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

        public override void EquipItem()
        {
            SetControlTips();
            EnableItemMeshes(enable: true);
            isPocketed = false;
            if (!hasBeenHeld)
            {
                hasBeenHeld = true;
                if (!isInShipRoom && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
                {
                    RoundManager.Instance.valueOfFoundScrapItems += scrapValue;
                }
            }
        }

        public override void SetControlTipsForItem()
        {
            SetControlTips();
        }

        private void SetControlTips()
        {
            string[] allLines = (canUseDeathNote ? new string[1] { "Write a name : [RMB]" } : new string[1] { "" });
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && !isOpened)
            {
                if (canUseDeathNote)
                {
                    if (!StartOfRound.Instance.inShipPhase)
                    {
                        AudioServerRpc(0, playerHeldBy.transform.position, 1f);  // page audio
                        playerList = Effects.GetPlayers();
                        enemyList = Effects.GetEnemies(excludeDaytime: true);
                        canvas = Instantiate(canvasPrefab, transform).GetComponent<DeathNoteCanvas>();
                        canvas.Initialize(this);  // open death note
                        isOpened = true;
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                        canvas.onExit += CloseDeathNote;
                    }
                    else
                    {
                        Effects.Message("Wow...", "That's one way of wasting death's powers", true);
                        canUseDeathNote = false;
                        SetControlTips();
                    }
                }
                else
                    Effects.Message("?", "The book doesn't acknowledge you as one of its owners anymore");
            }
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
                SetControlTips();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void AudioServerRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume = default)
        {
            AudioClientRpc(audioID, clientPosition, hostVolume, clientVolume == default ? hostVolume : clientVolume);
        }

        [ClientRpc]
        private void AudioClientRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume)
        {
            Effects.Audio(audioID, clientPosition, hostVolume, clientVolume, playerHeldBy);
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
