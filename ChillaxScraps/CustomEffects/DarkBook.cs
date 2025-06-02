using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DarkBook : PhysicsProp
    {
        public bool canUseDeathNote = true;
        public bool isOpened = false;
        public bool canKillEnemies = true;
        public bool punishInOrbit = true;
        public bool canRechargeInOrbit = false;
        public bool rechargeOrbitReady = false;
        public int usageOnServer = 0;
        public int usageOnServerMax = 1;
        public bool inRechargeMode = false;
        public float rechargeTime = 0f;
        public float timeNeededForRecharching = 0f;
        public float rechargeTimeMin = 10;
        public float rechargeTimeMax = 20;
        public int musicToPlayID = -1;
        public List<PlayerControllerB> playerList;
        public List<EnemyAI> enemyList;
        public List<string> keepEnemiesList;
        public DarkBookCanvas canvas;
        public GameObject canvasPrefab;
        public AudioSource? musicSource;
        public MeshRenderer? meshRenderer;
        public ScanNodeProperties? scanNode;
        private readonly float smoothTime = 5f;
        private float velocity;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            musicSource = GetComponent<AudioSource>();
            meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            scanNode = transform.GetChild(1).GetComponent<ScanNodeProperties>();
            if (canKillEnemies)
                keepEnemiesList = new List<string> { "GiantKiwi", "Ogopogo" };
            if (!IsHost && !IsServer)
                SyncBookStateServerRpc();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && IsOwner && !isOpened)
            {
                if (canUseDeathNote)
                {
                    if (!StartOfRound.Instance.inShipPhase)
                    {
                        AudioServerRpc(0, playerHeldBy.transform.position, 1f, 0.75f);  // page audio
                        playerList = Effects.GetPlayers();
                        if (canKillEnemies)
                            enemyList = Effects.GetEnemies(excludeDaytime: true, keepSpecificEnemiesList: keepEnemiesList);
                        canvas = Instantiate(canvasPrefab, transform).GetComponent<DarkBookCanvas>();
                        canvas.Initialize(this);  // open death note
                        isOpened = true;
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                        canvas.onExit += CloseDeathNote;
                        if (musicToPlayID != -1)
                            MusicServerRpc(musicToPlayID, 0.6f);
                    }
                    else
                    {
                        if (punishInOrbit)
                        {
                            Effects.Message("Wow...", "That's one way of wasting death's powers.", true);
                            canUseDeathNote = false;
                            SetControlTips();
                            UsedServerRpc();
                        }
                    }
                }
                else
                    Effects.Message("?", "The book doesn't acknowledge you as one of its owners anymore.");
            }
        }

        public void CloseDeathNote()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            isOpened = false;
            if (musicToPlayID != -1)
                StopMusicServerRpc();
        }

        public override void DiscardItem()
        {
            if (canvas != null)
                canvas.Close();
            base.DiscardItem();
        }

        public static bool PreventPocket(PlayerControllerB player)  // used by harmony patch
        {
            if (player != null && !player.isPlayerDead && player.currentlyHeldObjectServer != null)
            {
                var heldObject = player.currentlyHeldObjectServer;
                if ((heldObject.itemProperties.name == "DeathNoteItem" || heldObject.itemProperties.name == "DanceNoteItem")
                    && heldObject is DarkBook darkBook && darkBook.isOpened)
                    return false;
            }
            return true;
        }

        public virtual void ActivateDeathNote(GameObject objectToKill)
        {
        }

        public override void Update()
        {
            base.Update();
            if (inRechargeMode && timeNeededForRecharching != 0f)
            {
                rechargeTime += Time.deltaTime;
                if (rechargeTime >= timeNeededForRecharching && meshRenderer != null)
                {
                    meshRenderer.material.SetFloat("_TextureLerp",
                        Mathf.SmoothDamp(meshRenderer.material.GetFloat("_TextureLerp"), 1f, ref velocity, smoothTime));
                    if (meshRenderer.material.GetFloat("_TextureLerp") + 0.03 >= 1f)
                    {
                        ResetDeathNote();
                    }
                }
            }
            else if ((IsHost || IsServer) && canRechargeInOrbit && !inRechargeMode && rechargeOrbitReady && StartOfRound.Instance.inShipPhase)
            {
                rechargeOrbitReady = false;
                usageOnServer = 100;
                UsedServerRpc();
            }
            else if ((IsHost || IsServer) && canRechargeInOrbit && !rechargeOrbitReady && !StartOfRound.Instance.inShipPhase)
                rechargeOrbitReady = true;
        }

        public virtual void ResetDeathNote()
        {
            inRechargeMode = false;
            meshRenderer?.material.SetFloat("_TextureLerp", 1f);
            if (scanNode != null)
                scanNode.headerText = itemProperties.itemName;
            velocity = 0f;
            rechargeTime = 0f;
            timeNeededForRecharching = 0f;
            usageOnServer = 0;
            canUseDeathNote = true;
            if (IsOwner && isHeld && !isPocketed)
                SetControlTips();
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

        public void SetControlTips()
        {
            string[] allLines;
            if (inRechargeMode)
                allLines = new string[1] { "Recharging..." };
            else if (canUseDeathNote)
                allLines = new string[1] { "Write a name : [RMB]" };
            else
                allLines = new string[1] { "" };
            if (IsOwner)
            {
                HUDManager.Instance.ClearControlTips();
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AudioServerRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume = default)
        {
            AudioClientRpc(audioID, clientPosition, hostVolume, clientVolume == default ? hostVolume : clientVolume);
        }

        [ClientRpc]
        public void AudioClientRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume)
        {
            Effects.Audio(audioID, clientPosition, hostVolume, clientVolume, playerHeldBy);
        }

        [ServerRpc(RequireOwnership = false)]
        public void MusicServerRpc(int audioID, float volume)
        {
            MusicClientRpc(audioID, volume);
        }

        [ClientRpc]
        public void MusicClientRpc(int audioID, float volume)
        {
            if (musicSource != null)
            {
                musicSource.loop = true;
                musicSource.volume = volume;
                musicSource.clip = Plugin.audioClips[audioID];
                musicSource.PlayDelayed(0.3f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopMusicServerRpc()
        {
            StopMusicClientRpc();
        }

        [ClientRpc]
        public void StopMusicClientRpc()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                StartCoroutine(Effects.FadeOutAudio(musicSource, 0.1f));
                StartCoroutine(ResetAudioSourceAfterTime(musicSource, 0.15f));
            }
        }

        private IEnumerator ResetAudioSourceAfterTime(AudioSource audio, float time)
        {
            yield return new WaitForSeconds(time);
            audio.loop = false;
            audio.volume = 1f;
            audio.clip = null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UsedServerRpc()
        {
            SpecialEventOnServerUsage();
            usageOnServer++;
            if (usageOnServer >= usageOnServerMax && !inRechargeMode)
            {
                StartRechargeClientRpc(Random.Range(rechargeTimeMin, rechargeTimeMax));
            }
        }

        public virtual void SpecialEventOnServerUsage()
        {
        }

        [ClientRpc]
        public void StartRechargeClientRpc(float rechargeTimer)
        {
            if (scanNode != null)
                scanNode.headerText = itemProperties.itemName + " (notebook)";
            meshRenderer?.material.SetFloat("_TextureLerp", 0f);
            canUseDeathNote = false;
            inRechargeMode = true;
            timeNeededForRecharching = rechargeTimer;
            if (IsOwner && isHeld && !isPocketed)
                SetControlTips();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncBookStateServerRpc()
        {
            if (!inRechargeMode || meshRenderer == null)
                return;
            SyncBookStateClientRpc(timeNeededForRecharching, rechargeTime, meshRenderer.material.GetFloat("_TextureLerp"), velocity);
        }

        [ClientRpc]
        public void SyncBookStateClientRpc(float rechargeTimer, float actualRechargeTime, float lerp, float vel)
        {
            rechargeTime = actualRechargeTime;
            velocity = vel;
            canUseDeathNote = false;
            timeNeededForRecharching = rechargeTimer;
            meshRenderer?.material.SetFloat("_TextureLerp", lerp);
            if (scanNode != null)
                scanNode.headerText = itemProperties.itemName + " (notebook)";
            inRechargeMode = true;
        }
    }
}
