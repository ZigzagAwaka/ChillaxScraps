using ChillaxScraps.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class MasterSword : Shovel
    {
        public bool heroIsSelected = false;
        public ulong heroSteamId = 0;
        private bool firstTimeGrab = false;

        public MasterSword() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            shovelHitForce = Plugin.config.masterSwordDmg.Value;
            if (!heroIsSelected)
                SelectHeroServerRpc();
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (heroIsSelected && GameNetworkManager.Instance.localPlayerController.playerSteamId != heroSteamId)
            {
                StartCoroutine(DropSword());
            }
            else if (!firstTimeGrab)
            {
                firstTimeGrab = true;
                AudioServerRpc(8, playerHeldBy.transform.position, 0.8f, 1.2f);
            }
        }

        private IEnumerator DropSword()
        {
            yield return new WaitForEndOfFrame();
            Effects.DropItem(playerHeldBy.transform.position);
            Effects.Message("You aren't the one who's worthy of holding that blade", "");
        }

        [ServerRpc(RequireOwnership = false)]
        private void SelectHeroServerRpc()
        {
            if (!heroIsSelected)
            {
                var playersList = Effects.GetPlayers(includeDead: true);
                if (playersList.Count == 0) return;
                var hero = playersList[Random.Range(0, playersList.Count)];
                SelectHeroClientRpc(hero.playerSteamId);
            }
            else
                SelectHeroClientRpc(heroSteamId);
        }

        [ClientRpc]
        private void SelectHeroClientRpc(ulong heroId)
        {
            heroIsSelected = true;
            heroSteamId = heroId;
        }

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
    }
}
