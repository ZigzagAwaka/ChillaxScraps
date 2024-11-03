using ChillaxScraps.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class TotemOfUndying : PhysicsProp
    {
        public bool used = false;

        public TotemOfUndying() { }

        public static void TrySavePlayer(PlayerControllerB player)  // used by harmony patch
        {
            if (player == null || GameNetworkManager.Instance.localPlayerController.playerClientId != player.playerClientId)
            {
                return;
            }
            var currentlyHeldObjectServer = player.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer != null && currentlyHeldObjectServer.itemProperties.name == "TotemOfUndyingItem" && currentlyHeldObjectServer is TotemOfUndying totem && !totem.used)
            {
                totem.UndyingEffect(player);
            }
        }

        private void UndyingEffect(PlayerControllerB player)
        {
            StartOfRound.Instance.allowLocalPlayerDeath = false;
            AudioServerRpc(9, player.transform.position, 1.2f, 1.6f);
            used = true;
        }

        public static void TryDestroyItem(PlayerControllerB player)  // used by harmony patch
        {
            if (player == null || GameNetworkManager.Instance.localPlayerController.playerClientId != player.playerClientId)
            {
                return;
            }
            var currentlyHeldObjectServer = player.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer != null && currentlyHeldObjectServer.itemProperties.name == "TotemOfUndyingItem" && currentlyHeldObjectServer is TotemOfUndying totem && totem.used)
            {
                totem.DestroyTotem(player);
            }
        }

        private void DestroyTotem(PlayerControllerB player)
        {
            StartOfRound.Instance.allowLocalPlayerDeath = true;
            DestroyObjectServerRpc(player.playerClientId);
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

        [ServerRpc(RequireOwnership = false)]
        private void DestroyObjectServerRpc(ulong playerID)
        {
            DestroyObjectClientRpc(playerID);
        }

        [ClientRpc]
        private void DestroyObjectClientRpc(ulong playerID)
        {
            DestroyObjectInHand(StartOfRound.Instance.allPlayerScripts[playerID]);
        }
    }
}
