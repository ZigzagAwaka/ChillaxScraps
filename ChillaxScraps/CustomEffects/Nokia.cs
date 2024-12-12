using ChillaxScraps.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Nokia : WalkieTalkie
    {
        public bool isPlayingMusic = false;
        public AudioSource? audio;
        public AudioSource? farAudio;

        public Nokia() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            audio = GetComponent<AudioSource>();
            farAudio = transform.GetChild(2).GetComponent<AudioSource>();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (isPlayingMusic)
                return;
            if (playerHeldBy != null && buttonDown && !isHoldingButton && Random.Range(0, 10) == 0)  // 10% ringtone
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

        public override void FallWithCurve()
        {
            ThrowableItem.ItemFall(this);
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (right && IsOwner)
                Effects.DropItem(ThrowableItem.GetItemThrowDestination(playerHeldBy));
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
            farAudio.PlayOneShot(Plugin.audioClips[farAudioID]);
            yield return new WaitForSeconds(0.1f);
            while (audio.isPlaying || farAudio.isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }
            isPlayingMusic = false;
        }
    }
}
