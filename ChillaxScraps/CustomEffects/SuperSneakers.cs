﻿using ChillaxScraps.Utils;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class SuperSneakers : PhysicsProp
    {
        public bool jumpBoostActivated = false;
        private float originalJumpForce;

        public SuperSneakers()
        {
            useCooldown = 2;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (insertedBattery != null)
                insertedBattery.charge = 1;
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
            string[] allLines = ((!jumpBoostActivated) ? new string[1] { "Jump boost : [RMB]" } : new string[1] { "Deactivate : [RMB]" });
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null)
            {
                if (!jumpBoostActivated && insertedBattery != null && insertedBattery.charge > 0)
                {
                    AudioServerRpc(6, playerHeldBy.transform.position, 1.5f, 2f);
                    originalJumpForce = playerHeldBy.jumpForce;
                    playerHeldBy.jumpForce *= 2f;
                    jumpBoostActivated = true;
                    isBeingUsed = true;
                    if (!isPocketed)
                        SetControlTips();
                }
                else if (jumpBoostActivated)
                {
                    AudioServerRpc(7, playerHeldBy.transform.position, 1.5f, 2f);
                    playerHeldBy.jumpForce = originalJumpForce;
                    jumpBoostActivated = false;
                    isBeingUsed = false;
                    if (!isPocketed)
                        SetControlTips();
                }
            }
        }

        public override void UseUpBatteries()
        {
            if (jumpBoostActivated)
                ItemActivate(true, true);
            base.UseUpBatteries();
        }

        public override void DiscardItem()
        {
            if (jumpBoostActivated)
                ItemActivate(true, true);
            base.DiscardItem();
        }

        public override void OnNetworkDespawn()
        {
            if (jumpBoostActivated)
                ItemActivate(true, true);
            base.OnNetworkDespawn();
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
    }
}