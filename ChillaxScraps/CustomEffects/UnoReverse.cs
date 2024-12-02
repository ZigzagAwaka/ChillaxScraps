using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class UnoReverse : PhysicsProp
    {
        public bool hasBeenUsed = false;
        public List<PlayerControllerB> playerList;

        public UnoReverse() { }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && !hasBeenUsed)
            {
                if (StartOfRound.Instance.inShipPhase)
                {
                    Effects.Message("Can't be used at the moment", "");
                    return;
                }
                playerList = Effects.GetPlayers();
                playerList.Remove(playerHeldBy);
                if (playerList.Count <= 0)
                {
                    Effects.Message("Huh ?", "No players to swap with...");
                    return;
                }
                var playerToSwap = playerList[Random.Range(0, playerList.Count)];
                var playerHeldByPosition = GetPosition(playerHeldBy);
                var playerToSwapPosition = GetPosition(playerToSwap);
                hasBeenUsed = true;
                SwapPlayersServerRpc(playerHeldByPosition, playerHeldBy.playerClientId, playerHeldBy.OwnerClientId,
                                     playerToSwapPosition, playerToSwap.playerClientId, playerToSwap.actualClientId);
                DestroyObjectServerRpc(StartOfRound.Instance.localPlayerController.playerClientId);
            }
        }

        private Vector3 GetPosition(PlayerControllerB player)
        {
            Vector3 position = RoundManager.Instance.GetNavMeshPosition(player.transform.position, RoundManager.Instance.navHit, 2.7f);
            return new Vector3(player.transform.position.x, position.y, player.transform.position.z);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SwapPlayersServerRpc(Vector3 p1Position, ulong p1PlayerId, ulong p1ClientId, Vector3 p2Position, ulong p2PlayerId, ulong p2ClientId)
        {
            ClientRpcParams p1ClientParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { p1ClientId } } };
            ClientRpcParams p2ClientParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { p2ClientId } } };
            var p1 = StartOfRound.Instance.allPlayerScripts[p1PlayerId];
            var p2 = StartOfRound.Instance.allPlayerScripts[p2PlayerId];
            bool p1Ship = p1.isInHangarShipRoom && p1.isInElevator;
            bool p1Interior = p1.isInsideFactory;
            bool p1Exterior = !p1Ship && !p1Interior;
            bool p2Ship = p2.isInHangarShipRoom && p2.isInElevator;
            bool p2Interior = p2.isInsideFactory;
            bool p2Exterior = !p2Ship && !p2Interior;
            SwapPlayersClientRpc(p2Position, p2Ship, p2Exterior, p2Interior, p1ClientParams);  // teleport player1 to player2
            SwapPlayersClientRpc(p1Position, p1Ship, p1Exterior, p1Interior, p2ClientParams);  // teleport player2 to player1
        }

        [ClientRpc]
        private void SwapPlayersClientRpc(Vector3 positionToSwap, bool ship, bool exterior, bool interior, ClientRpcParams clientRpcParams = default)
        {
            Effects.Audio(4, 1f);
            Effects.Teleportation(StartOfRound.Instance.localPlayerController, positionToSwap);
            SetPosFlagsServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, ship, exterior, interior);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPosFlagsServerRpc(ulong playerID, bool ship, bool exterior, bool interior)
        {
            SetPosFlagsClientRpc(playerID, ship, exterior, interior);
        }

        [ClientRpc]
        private void SetPosFlagsClientRpc(ulong playerID, bool ship, bool exterior, bool interior)
        {
            Effects.SetPosFlags(playerID, ship, exterior, interior);
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
