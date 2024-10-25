using ChillaxScraps.Utils;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class MasterSword : Shovel
    {
        public bool heroIsSelected = false;
        public ulong heroClientId = 0;
        private bool firstTimeGrab = false;

        public MasterSword() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!heroIsSelected)
                SelectHeroServerRpc();
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (heroIsSelected && GameNetworkManager.Instance.localPlayerController.OwnerClientId != heroClientId)
            {
                Effects.DropItem(playerHeldBy.transform.position);
                Effects.Message("You aren't the one who's worthy of holding that blade", "");
            }
            else if (!firstTimeGrab)
            {
                firstTimeGrab = true;
                AudioServerRpc(8, playerHeldBy.transform.position, 0.8f, 1.2f);
            }
        }


        [ServerRpc(RequireOwnership = false)]
        private void SelectHeroServerRpc()
        {
            if (!heroIsSelected)
            {
                var playersList = Effects.GetPlayers(includeDead: true);
                if (playersList.Count == 0) return;
                var hero = playersList[Random.Range(0, playersList.Count)];
                SelectHeroClientRpc(hero.OwnerClientId);
            }
            else
                SelectHeroClientRpc(heroClientId);
        }

        [ClientRpc]
        private void SelectHeroClientRpc(ulong heroId)
        {
            heroIsSelected = true;
            heroClientId = heroId;
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
