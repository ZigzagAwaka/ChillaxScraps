using ChillaxScraps.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Moyai : PhysicsProp
    {
        public AudioSource? itemAudio;
        public AudioSource? jiggleAudio;
        public Coroutine? audioCoroutine;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            itemAudio = transform.GetComponent<AudioSource>();
            jiggleAudio = transform.GetChild(1).GetComponent<AudioSource>();
        }

        public override void JiggleItemEffect(float audioVolume)
        {
            if (itemProperties.clinkAudios != null && itemProperties.clinkAudios.Length != 0)
            {
                RoundManager.PlayRandomClip(jiggleAudio, itemProperties.clinkAudios, randomize: true, 0.8f, -1);
            }
        }

        public override void DiscardItem()
        {
            if (playerHeldBy != null && Effects.IsUnlucky(playerHeldBy.playerSteamId) && Random.Range(0, 10) <= 4)  // 50% unlucky
            {
                AudioServerRpc();
            }
            base.DiscardItem();
        }

        [ServerRpc(RequireOwnership = false)]
        private void AudioServerRpc()
        {
            AudioClientRpc();
        }

        [ClientRpc]
        private void AudioClientRpc()
        {
            audioCoroutine ??= StartCoroutine(PlaySpecialAudio());
        }

        private IEnumerator PlaySpecialAudio()
        {
            yield return new WaitForSeconds(7f);
            if (!isHeld && !isHeldByEnemy && itemAudio != null)
            {
                itemAudio.PlayOneShot(Plugin.audioClips[52], 1.0f);
                WalkieTalkie.TransmitOneShotAudio(itemAudio, Plugin.audioClips[52]);
                RoundManager.Instance.PlayAudibleNoise(transform.position, 20, 0.5f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            }
            audioCoroutine = null;
        }
    }
}