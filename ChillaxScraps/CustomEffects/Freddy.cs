using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Freddy : PhysicsProp
    {
        public AudioSource? audio;

        public Freddy()
        {
            useCooldown = 1.5f;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            audio = GetComponent<AudioSource>();
            if (transform.position.y < -80f)
            {

            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (StartOfRound.Instance.inShipPhase || Random.Range(0, 10) <= 6)  // 70% audio
                AudioServerRpc(48, 1f);
            else if (playerHeldBy != null)  // 30% bad effect
            {
                var player = playerHeldBy;
                AudioServerRpc(49, 1.2f);
                StartCoroutine(DamagePlayer(player, player.health <= 20 ? 100 : player.health - 10));
                Effects.DropItem(player.transform.position);
            }
        }

        private IEnumerator DamagePlayer(PlayerControllerB player, int damage)
        {
            yield return new WaitForEndOfFrame();
            Effects.Damage(player, damage, CauseOfDeath.Mauling, (int)Effects.DeathAnimation.NoHead2);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AudioServerRpc(int audioID, float volume)
        {
            AudioClientRpc(audioID, volume);
        }

        [ClientRpc]
        private void AudioClientRpc(int audioID, float volume)
        {
            if (audio != null)
            {
                audio.PlayOneShot(Plugin.audioClips[audioID], volume);
            }
        }
    }
}
