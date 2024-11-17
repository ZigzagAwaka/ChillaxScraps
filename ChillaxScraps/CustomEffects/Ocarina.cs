using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Ocarina : NoisemakerProp
    {
        internal class OcarinaSong
        {
            public string title;
            public int usage = 0;
            public int maxUsage;
            public Color color;
            public static float colorMultiplicator = 5f;
            public Condition[] conditions;

            public OcarinaSong(string title, int maxUsage, Color color, params Condition[] conditions)
            {
                this.title = title;
                this.maxUsage = maxUsage;
                this.color = color;
                this.conditions = conditions;
            }
        }

        internal enum Condition
        {
            None,
            IsPlayerInsideFactory,
            IsPlayerOutsideFactory,
            IsPlayerFacingDoor,
            IsTimeAfternoon  //360
        }

        public int selectedSongID = 0;
        public float selectedSongLength = 0f;
        public bool isOcarinaUnique = false;
        public bool isOcarinaRestricted = true;
        public bool isPlaying = false;
        public bool isHoldingButton = false;
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
            new OcarinaSong("", -1, new Color(0, 0, 0), Condition.None),
            new OcarinaSong("Zelda's Lullaby", 2, new Color(0.87f, 0.36f, 1f), Condition.IsPlayerFacingDoor, Condition.None),
            new OcarinaSong("Epona's Song", 3, new Color(1f, 0.56f, 0.16f), Condition.IsPlayerOutsideFactory),
            new OcarinaSong("Sun's Song", 1, new Color(1f, 0.92f, 0.1f), Condition.None),
            new OcarinaSong("Saria's Song", 2, new Color(0.11f, 0.98f, 0.17f), Condition.None),
            new OcarinaSong("Song of Time", 1, new Color(0.18f, 0.76f, 1f), Condition.IsTimeAfternoon),
            new OcarinaSong("Song of Storms", 1, new Color(0.85f, 0.76f, 0.45f), Condition.IsPlayerOutsideFactory),
            new OcarinaSong("Song of Healing", 1, new Color(1f, 0.2f, 0), Condition.None),
            new OcarinaSong("Song of Soaring", 5, new Color(0.69f, 0.97f, 1f), Condition.None),
            new OcarinaSong("Sonata of Awakening", 1, new Color(0.12f, 1f, 0.45f), Condition.None),
            new OcarinaSong("Goron Lullaby", 1, new Color(0.72f, 0.45f, 0), Condition.None),
            new OcarinaSong("New Wave Bossa Nova", 1, new Color(0.03f, 0.09f, 1f), Condition.None),
            new OcarinaSong("Elegy of Emptiness", 3, new Color(0.94f, 0.63f, 0.49f), Condition.None),
            new OcarinaSong("Oath to Order", 1, new Color(1f, 1f, 1f), Condition.IsPlayerOutsideFactory),
        };

        public Ocarina()
        {
            position = originalPosition;
            rotation = originalRotation;
            isOcarinaUnique = Plugin.config.ocarinaUniqueSongs.Value;
            isOcarinaRestricted = Plugin.config.ocarinaRestrictUsage.Value;
        }

        private int GetValidID(int audioID)
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
            particleSystem = transform.GetChild(3).GetComponent<ParticleSystem>();
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
                OcarinaAudioServerRpc(selectedSongID);
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
        private void OcarinaAudioServerRpc(int audioClientID)
        {
            OcarinaAudioClientRpc(GetValidID(audioClientID));
        }

        [ClientRpc]
        private void OcarinaAudioClientRpc(int audioID)
        {
            if (particleRenderer != null && particleSystem != null && selectedSongID != 0)
            {
                particleRenderer.sharedMaterial.SetColor("_Color", ocarinaSongs[selectedSongID].color * OcarinaSong.colorMultiplicator);
                particleSystem.Play();
            }
            if (noiseAudio != null)
            {
                noiseAudio.PlayOneShot(noiseSFX[audioID], 1f);
            }
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
            if (particleSystem != null && particleSystem.isPlaying)
                particleSystem.Stop();
            if (noiseAudio != null && noiseAudio.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudio, 0.1f));
            if (noiseAudioFar != null && noiseAudioFar.isPlaying)
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
