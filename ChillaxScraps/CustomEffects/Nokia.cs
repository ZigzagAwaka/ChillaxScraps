using ChillaxScraps.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Nokia : WalkieTalkie
    {
        public bool isPlayingMusic = false;
        public bool hasBeenThrown = false;
        public AudioSource? audio;
        public AudioSource? farAudio;

        public Nokia() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            audio = transform.GetChild(7).GetComponent<AudioSource>();
            farAudio = transform.GetChild(2).GetComponent<AudioSource>();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (isPlayingMusic)
                return;
            if (playerHeldBy != null && isBeingUsed && buttonDown && !isHoldingButton && Random.Range(0, 10) == 0)  // 10% ringtone
            {
                var audioID = 44;
                var farAudioID = 46;
                if (Random.Range(0, 10) <= 1 || Effects.IsUnlucky(playerHeldBy.playerSteamId))  // 20% arabic ringtone or 100% if unlucky
                {
                    audioID++;
                    farAudioID++;
                }
                MusicServerRpc(audioID, farAudioID);
            }
            else
                base.ItemActivate(used, buttonDown);  // 90% walkietalkie
        }

        public override void ItemInteractLeftRight(bool right)
        {
            if (!right && !isPlayingMusic)
            {
                base.ItemInteractLeftRight(right);
            }
            if (right && IsOwner)
            {
                Effects.DropItem(ThrowableItem.GetItemThrowDestination(playerHeldBy));
                HasBeenThrownServerRpc(true);
            }
        }

        public override void FallWithCurve()
        {
            ThrowableItem.ItemFall(this);
        }

        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            if (audio == null || farAudio == null)
                return;
            if (isPlayingMusic)
            {
                audio.Stop();
                farAudio.Stop();
                isPlayingMusic = false;
            }
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            if (hasBeenThrown)
            {
                Landmine.SpawnExplosion(transform.position, false, 0, 1.5f, 10, 5);
                hasBeenThrown = false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void MusicServerRpc(int audioID, int farAudioID)
        {
            MusicClientRpc(audioID, farAudioID);
        }

        [ClientRpc]
        private void MusicClientRpc(int audioID, int farAudioID)
        {
            StartCoroutine(PlayMusic(audioID, farAudioID));
        }

        private IEnumerator PlayMusic(int audioID, int farAudioID)
        {
            if (audio == null || farAudio == null)
                yield break;
            if (audio.isPlaying)
                audio.Stop();
            if (farAudio.isPlaying)
                farAudio.Stop();
            isPlayingMusic = true;
            audio.PlayOneShot(Plugin.audioClips[audioID]);
            farAudio.PlayOneShot(Plugin.audioClips[farAudioID], 1.3f);
            TransmitOneShotAudio(audio, Plugin.audioClips[audioID], 1f);
            yield return new WaitForSeconds(0.1f);
            while (audio.isPlaying || farAudio.isPlaying)
            {
                RoundManager.Instance.PlayAudibleNoise(transform.position, 40, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
                if (playerHeldBy != null)
                    playerHeldBy.timeSinceMakingLoudNoise = 0f;
                yield return new WaitForSeconds(0.1f);
            }
            isPlayingMusic = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void HasBeenThrownServerRpc(bool value)
        {
            HasBeenThrownClientRpc(value);
        }

        [ClientRpc]
        private void HasBeenThrownClientRpc(bool value)
        {
            hasBeenThrown = value;
        }
    }
}
