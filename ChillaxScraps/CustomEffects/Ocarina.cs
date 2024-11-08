using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Ocarina : NoisemakerProp
    {
        public int selectedAudioID = 0;
        public bool isOcarinaUnique = false;
        public Vector3 originalPosition = new Vector3(0.03f, 0.22f, -0.09f);
        public Vector3 originalRotation = new Vector3(180, 5, -5);
        public Vector3 animationPosition = new Vector3(-0.12f, 0.15f, 0.01f);
        public Vector3 animationRotation = new Vector3(60, 0, -50);
        private Vector3 position;
        private Vector3 rotation;
        private Coroutine? ocarinaCoroutine;
        private readonly string[] audioTitles = new string[14] {
            "", "Zelda's Lullaby", "Epona's Song", "Sun's Song", "Saria's Song", "Song of Time", "Song of Storms",
            "Song of Healing", "Song of Soaring", "Sonata of Awakening", "Goron Lullaby", "New Wave Bossa Nova",
            "Elegy of Emptiness", "Oath of Order"
        };

        public Ocarina()
        {
            position = originalPosition;
            rotation = originalRotation;
            isOcarinaUnique = Plugin.config.ocarinaUniqueSongs.Value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (isOcarinaUnique)
                selectedAudioID = Random.Range(0, audioTitles.Length);
        }

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
            string[] allLines = (isOcarinaUnique ? new string[2] { "Play sound : [RMB]", audioTitles[selectedAudioID] } :
                new string[3] { "Play sound : [RMB]", "Select audio : [Q]", audioTitles[selectedAudioID] });
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (buttonDown && playerHeldBy != null && !playerHeldBy.activatingItem && !noiseAudio.isPlaying && !noiseAudioFar.isPlaying)
            {
                UpdatePosRotServerRpc(animationPosition, animationRotation);
                playerHeldBy.activatingItem = buttonDown;
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);  // start playing music animation
                ocarinaCoroutine = StartCoroutine(PlayOcarina(playerHeldBy));
            }
            else if (!buttonDown && playerHeldBy != null && playerHeldBy.activatingItem && ocarinaCoroutine != null)
            {
                StopOcarinaAudioServerRpc();
                StopCoroutine(ocarinaCoroutine);
                UpdatePosRotServerRpc(originalPosition, originalRotation);
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);  // stop playing music animation
                playerHeldBy.activatingItem = false;
            }
        }

        private IEnumerator PlayOcarina(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.9f);
            OcarinaAudioServerRpc(selectedAudioID);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => !noiseAudio.isPlaying && !noiseAudioFar.isPlaying);  // stop playing music when song ends
            UpdatePosRotServerRpc(originalPosition, originalRotation);
            player.playerBodyAnimator.SetBool("useTZPItem", false);
            player.activatingItem = false;
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (!right && playerHeldBy != null && !isOcarinaUnique && !playerHeldBy.activatingItem && !noiseAudio.isPlaying && !noiseAudioFar.isPlaying)
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
            if (playerHeldBy != null && !isPocketed)
                playerHeldBy.equippedUsableItemQE = false;
            base.DiscardItem();
        }

        public override void OnNetworkDespawn()
        {
            if (playerHeldBy != null && !isPocketed)
                playerHeldBy.equippedUsableItemQE = false;
            base.OnNetworkDespawn();
        }

        public override void LateUpdate()
        {
            if (parentObject != null)
            {
                base.transform.rotation = parentObject.rotation;
                base.transform.Rotate(rotation);
                base.transform.position = parentObject.position;
                Vector3 positionOffset = position;
                positionOffset = parentObject.rotation * positionOffset;
                base.transform.position += positionOffset;
            }
            if (radarIcon != null)
            {
                radarIcon.position = base.transform.position;
            }
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
        private void StopOcarinaAudioServerRpc()
        {
            StopOcarinaAudioClientRpc();
        }

        [ClientRpc]
        private void StopOcarinaAudioClientRpc()
        {
            if (noiseAudio.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudio, 0.1f));
            if (noiseAudioFar.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudioFar, 0.1f));
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePosRotServerRpc(Vector3 newPos, Vector3 newRot)
        {
            UpdatePosRotClientRpc(newPos, newRot);
        }

        [ClientRpc]
        private void UpdatePosRotClientRpc(Vector3 newPos, Vector3 newRot)
        {
            position = newPos;
            rotation = newRot;
        }
    }
}
