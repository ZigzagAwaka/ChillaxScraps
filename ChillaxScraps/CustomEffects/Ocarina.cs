using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Ocarina : NoisemakerProp
    {
        public int selectedSongID = 0;
        public float selectedSongLength = -1f;
        public float lastUsedTime = 0f;
        public bool isOcarinaUnique = false;
        public bool isOcarinaRestricted = true;
        public bool isPlaying = false;
        public bool isHoldingButton = false;
        public bool resetUsage = false;
        public Vector3 originalPosition = new Vector3(0.03f, 0.22f, -0.09f);
        public Vector3 originalRotation = new Vector3(180, 5, -5);
        public Vector3 animationPosition = new Vector3(-0.12f, 0.15f, 0.01f);
        public Vector3 animationRotation = new Vector3(60, 0, -50);
        private Vector3 position;
        private Vector3 rotation;
        private Coroutine? ocarinaCoroutine;
        private PlayerControllerB? previousPlayerHeldBy;
        private ParticleSystem? particleSystem;
        private Renderer? particleRenderer;

        private readonly OcarinaSong[] ocarinaSongs = new OcarinaSong[14] {
            new OcarinaSong("", new Color(0, 0, 0), null, 0, Condition.Invalid),
            new OcarinaSong("Zelda's Lullaby", new Color(0.87f, 0.36f, 1f), OcarinaSong.ZeldaLullaby, 2, Condition.IsPlayerFacingDoor, Condition.IsTimeAfternoon),
            new OcarinaSong("Epona's Song", new Color(0.94f, 0.44f, 0.01f), OcarinaSong.EponaSong, 3, Condition.IsPlayerOutsideFactory),
            new OcarinaSong("Sun's Song", new Color(1f, 0.92f, 0.1f), OcarinaSong.SunSong, 1, Condition.None),
            new OcarinaSong("Saria's Song", new Color(0.11f, 0.98f, 0.17f), OcarinaSong.SariaSong, 2, Condition.None),
            new OcarinaSong("Song of Time", new Color(0.18f, 0.76f, 1f), OcarinaSong.SongOfTime, 1, Condition.IsTimeAfternoon),
            new OcarinaSong("Song of Storms", new Color(0.87f, 0.76f, 0.42f), OcarinaSong.SongOfStorms, 1, Condition.IsPlayerOutsideFactory),
            new OcarinaSong("Song of Healing", new Color(1f, 0.22f, 0.09f), OcarinaSong.SongOfHealing, 1, Condition.None),
            new OcarinaSong("Song of Soaring", new Color(0.5f, 0.8f, 1f), OcarinaSong.SongOfSoaring, 5, Condition.None),
            new OcarinaSong("Sonata of Awakening", new Color(0.12f, 1f, 0.45f), OcarinaSong.SonataOfAwakening, 1, Condition.None),
            new OcarinaSong("Goron Lullaby", new Color(1f, 0.41f, 0.54f), OcarinaSong.GoronLullaby, 2, Condition.None),
            new OcarinaSong("New Wave Bossa Nova", new Color(0.03f, 0.09f, 1f), OcarinaSong.NewWaveBossaNova, 1, Condition.None),
            new OcarinaSong("Elegy of Emptiness", new Color(0.85f, 0.53f, 0.33f), OcarinaSong.ElegyOfEmptiness, 3, Condition.None),
            new OcarinaSong("Oath to Order", new Color(1f, 1f, 1f), OcarinaSong.OathToOrder, 1, Condition.IsPlayerOutsideFactory)
        };

        public Ocarina()
        {
            position = originalPosition;
            rotation = originalRotation;
            isOcarinaUnique = Plugin.config.ocarinaUniqueSongs.Value;
            isOcarinaRestricted = Plugin.config.ocarinaRestrictUsage.Value;
        }

        private int GetNoiseID(int audioID)
        {
            int Id = audioID;
            if (audioID == 0)
                Id = Random.Range(0, 5);
            else
                Id += 4;
            return Id;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (isOcarinaUnique)
                selectedSongID = Random.Range(0, ocarinaSongs.Length);
            selectedSongLength = noiseSFX[GetNoiseID(selectedSongID)].length;
            particleSystem = transform.GetChild(3).GetComponent<ParticleSystem>();
            if (particleSystem != null)
                particleRenderer = particleSystem.GetComponent<Renderer>();
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
            string[] allLines = (isOcarinaUnique ? new string[2] { "Play sound : [RMB]", ocarinaSongs[selectedSongID].title } :
                new string[3] { "Play sound : [RMB]", "Select audio : [Q]", ocarinaSongs[selectedSongID].title });
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (playerHeldBy == null)
                return;
            isHoldingButton = buttonDown;
            if (!isPlaying && buttonDown)
            {
                isPlaying = true;
                previousPlayerHeldBy = playerHeldBy;
                if (ocarinaCoroutine != null)
                    StopCoroutine(ocarinaCoroutine);
                ocarinaCoroutine = StartCoroutine(PlayOcarina());
            }
        }

        private IEnumerator PlayOcarina()
        {
            playerHeldBy.activatingItem = true;
            playerHeldBy.twoHanded = true;
            UpdatePosRotServerRpc(animationPosition, animationRotation);
            playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", true);  // start playing music animation
            yield return new WaitForSeconds(0.9f);
            if (isHoldingButton && isHeld)
            {
                var isValid = ocarinaSongs[selectedSongID].IsValid(previousPlayerHeldBy, isOcarinaRestricted);
                lastUsedTime = Time.realtimeSinceStartup;
                OcarinaAudioServerRpc(selectedSongID, isValid.Item1);
                StartCoroutine(OcarinaEffect(isValid.Item1, isValid.Item2));
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => !isHoldingButton || !isHeld);
            }
            previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);  // stop playing music animation
            UpdatePosRotServerRpc(originalPosition, originalRotation);
            StopOcarinaAudioServerRpc();
            yield return new WaitForSeconds(0.1f);
            previousPlayerHeldBy.activatingItem = false;
            previousPlayerHeldBy.twoHanded = false;
            yield return new WaitForEndOfFrame();
            isPlaying = false;
            ocarinaCoroutine = null;
        }

        private IEnumerator OcarinaEffect(bool isEffectValid, int variationId)
        {
            yield return new WaitForSeconds(0.1f);
            if (isEffectValid && !StartOfRound.Instance.inShipPhase)
            {
                yield return new WaitUntil(() => !isHoldingButton || !isHeld || Time.realtimeSinceStartup - lastUsedTime >= selectedSongLength);
                if (isHoldingButton && isHeld)
                {
                    StopOcarinaAudioServerRpc();
                    if (ocarinaSongs[selectedSongID].StartEffect(this, previousPlayerHeldBy, variationId))
                        UpdateUsageServerRpc(selectedSongID);
                }
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (!right && playerHeldBy != null && !isOcarinaUnique && !isPlaying)
            {
                if (selectedSongID < ocarinaSongs.Length - 1)
                    selectedSongID++;
                else
                    selectedSongID = 0;
                SetControlTips();
                selectedSongLength = noiseSFX[GetNoiseID(selectedSongID)].length;
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

        public override void Update()
        {
            base.Update();
            if (resetUsage && StartOfRound.Instance.inShipPhase)
            {
                resetUsage = false;
                for (int i = 0; i < ocarinaSongs.Length; i++)
                    ocarinaSongs[i].usage = 0;
            }
        }

        public override void LateUpdate()
        {
            if (parentObject != null)
            {
                transform.rotation = parentObject.rotation;
                transform.Rotate(rotation);
                transform.position = parentObject.position;
                Vector3 positionOffset = position;
                positionOffset = parentObject.rotation * positionOffset;
                transform.position += positionOffset;
            }
            if (radarIcon != null)
            {
                radarIcon.position = transform.position;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void OcarinaAudioServerRpc(int audioClientID, bool isEffectValid)
        {
            OcarinaAudioClientRpc(audioClientID, GetNoiseID(audioClientID), isEffectValid && !StartOfRound.Instance.inShipPhase);
        }

        [ClientRpc]
        private void OcarinaAudioClientRpc(int audioID, int noiseAudioID, bool isEffectValid)
        {
            if (particleRenderer != null && particleSystem != null && particleSystem.lights.enabled && isEffectValid)
            {
                particleSystem.lights.light.color = ocarinaSongs[audioID].color * OcarinaSong.colorMultiplicator;
                particleRenderer.sharedMaterial.SetColor("_Color", ocarinaSongs[audioID].color * OcarinaSong.colorMultiplicator);
                particleSystem.Play();
            }
            if (noiseAudio != null)
            {
                noiseAudio.PlayOneShot(noiseSFX[noiseAudioID], 1f);
            }
            if (noiseAudioFar != null)
            {
                noiseAudioFar.PlayOneShot(noiseSFXFar[noiseAudioID], 1f);
            }
            WalkieTalkie.TransmitOneShotAudio(noiseAudio, noiseSFX[noiseAudioID], 1f);
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
            if (particleSystem != null && particleSystem.isPlaying)
                particleSystem.Stop();
            if (noiseAudio != null && noiseAudio.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudio, 0.1f));
            if (noiseAudioFar != null && noiseAudioFar.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudioFar, 0.1f));
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateUsageServerRpc(int audioClientID)
        {
            UpdateUsageClientRpc(audioClientID);
        }

        [ClientRpc]
        private void UpdateUsageClientRpc(int audioID)
        {
            ocarinaSongs[audioID].usage++;
            resetUsage = true;
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

        // OCARINA EFFECTS // COROUTINE & RPCs

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
        private void AudioAtPositionServerRpc(int audioID, Vector3 position, float volume, bool adjust = true)
        {
            AudioAtPositionClientRpc(audioID, position, volume, adjust);
        }

        [ClientRpc]
        private void AudioAtPositionClientRpc(int audioID, Vector3 position, float volume, bool adjust)
        {
            Effects.Audio(audioID, position, volume, adjust);
        }

        public IEnumerator OpenDoorZeldaStyle(DoorLock door)
        {
            AudioAtPositionServerRpc(12, door.transform.position, 2f);
            yield return new WaitForSeconds(1.45f);
            if (door.isLocked && !door.isPickingLock)
            {
                door.UnlockDoorSyncWithServer();
                yield return new WaitForSeconds(0.5f);
                var animObjTrig = door.gameObject.GetComponent<AnimatedObjectTrigger>();
                if (animObjTrig != null)
                {
                    animObjTrig.TriggerAnimationNonPlayer(overrideBool: true);
                    door.OpenDoorAsEnemyServerRpc();
                }
            }
        }


    }
}
