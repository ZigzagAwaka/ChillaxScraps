﻿using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Ocarina : NoisemakerProp
    {
        public int selectedAudioID = 0;
        public Vector3? originalPosition = null;
        public Vector3? originalRotation = null;
        private readonly string[] audioTitles = new string[14] {
            "", "Zelda's Lullaby", "Epona's Song", "Sun's Song", "Saria's Song", "Song of Time", "Song of Storms",
            "Song of Healing", "Song of Soaring", "Sonata of Awakening", "Goron Lullaby", "New Wave Bossa Nova",
            "Elegy of Emptiness", "Oath of Order"
        };

        public Ocarina() { }

        public override void EquipItem()
        {
            SetControlTips();
            EnableItemMeshes(enable: true);
            playerHeldBy.equippedUsableItemQE = true;
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
            string[] allLines = new string[3] { "Play sound : [RMB]", "Select audio : [Q]", audioTitles[selectedAudioID] };
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (buttonDown && playerHeldBy != null && !playerHeldBy.activatingItem && !noiseAudio.isPlaying && !noiseAudioFar.isPlaying)
            {
                originalPosition = itemProperties.positionOffset;
                originalRotation = itemProperties.rotationOffset;
                UpdatePosRotServerRpc(new Vector3(-0.12f, 0.15f, 0.01f), new Vector3(60, 0, -50));
                playerHeldBy.activatingItem = buttonDown;
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);  // start playing music animation
                StartCoroutine(PlayOcarina(playerHeldBy));
            }
        }

        private IEnumerator PlayOcarina(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.9f);
            OcarinaAudioServerRpc(selectedAudioID);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => !noiseAudio.isPlaying && !noiseAudio.isPlaying);
            UpdatePosRotServerRpc(originalPosition != null ? originalPosition.Value : default, originalRotation != null ? originalRotation.Value : default);
            player.playerBodyAnimator.SetBool("useTZPItem", false);  // stop playing music animation
            player.activatingItem = false;
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (!right && playerHeldBy != null && !playerHeldBy.activatingItem && !noiseAudio.isPlaying && !noiseAudioFar.isPlaying)
            {
                if (selectedAudioID < audioTitles.Length - 1)
                    selectedAudioID++;
                else
                    selectedAudioID = 0;
                SetControlTips();
            }
        }

        public override void PocketItem()
        {
            if (playerHeldBy != null)
                playerHeldBy.equippedUsableItemQE = false;
            base.PocketItem();
        }

        public override void DiscardItem()
        {
            if (playerHeldBy != null)
                playerHeldBy.equippedUsableItemQE = false;
            base.DiscardItem();
        }

        public override void OnNetworkDespawn()
        {
            if (playerHeldBy != null)
                playerHeldBy.equippedUsableItemQE = false;
            base.OnNetworkDespawn();
        }

        [ServerRpc(RequireOwnership = false)]
        private void OcarinaAudioServerRpc(int audioClientID)
        {
            int audioID = audioClientID;
            if (audioClientID == 0)
                audioID = Random.Range(0, 5);
            else
                audioID += 4;
            OcarinaAudioClientRpc(audioID);
        }

        [ClientRpc]
        private void OcarinaAudioClientRpc(int audioID)
        {
            noiseAudio.PlayOneShot(noiseSFX[audioID], 1f);
            if (noiseAudioFar != null)
            {
                noiseAudioFar.PlayOneShot(noiseSFXFar[audioID], 1f);
            }
            WalkieTalkie.TransmitOneShotAudio(noiseAudio, noiseSFX[audioID], 1f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            if (minLoudness >= 0.6f && playerHeldBy != null)
            {
                playerHeldBy.timeSinceMakingLoudNoise = 0f;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePosRotServerRpc(Vector3 newPos, Vector3 newRot)
        {
            UpdatePosRotClientRpc(newPos, newRot);
        }

        [ClientRpc]
        private void UpdatePosRotClientRpc(Vector3 newPos, Vector3 newRot)
        {
            itemProperties.positionOffset = newPos;
            itemProperties.rotationOffset = newRot;
        }
    }
}