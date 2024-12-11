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
        public int musicToPlayID = -1;
        public AudioSource musicSource;
        public List<PlayerControllerB> playerList;
        public List<EnemyAI> enemyList;
        public DarkBookCanvas canvas;
        public GameObject canvasPrefab;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            musicSource = GetComponent<AudioSource>();
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
                            enemyList = Effects.GetEnemies(excludeDaytime: true);
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

        public override void PocketItem()
        {
            base.PocketItem();
            if (canvas != null)
                canvas.Close();
        }

        public override void DiscardItem()
        {
            if (canvas != null)
                canvas.Close();
            base.DiscardItem();
        }

        public virtual void ActivateDeathNote(GameObject objectToKill)
        {
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
            string[] allLines = (canUseDeathNote ? new string[1] { "Write a name : [RMB]" } : new string[1] { "" });
            if (IsOwner)
            {
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
    }
}
