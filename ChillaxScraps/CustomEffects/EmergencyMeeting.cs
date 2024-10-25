using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class EmergencyMeeting : PhysicsProp
    {
        public bool hasBeenUsed = false;
        public List<PlayerControllerB> playerList;
        private readonly GameObject canvasPrefab;

        public EmergencyMeeting()
        {
            useCooldown = 1;
            canvasPrefab = Plugin.gameObjects[1];
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null)
            {
                if (!StartOfRound.Instance.inShipPhase && StartOfRound.Instance.shipHasLanded && playerHeldBy.isInsideFactory)
                {
                    playerList = Effects.GetPlayers(excludeOutsideFactory: true);
                    playerList.Remove(playerHeldBy);
                    if (playerList.Count <= 0)
                    {
                        Effects.Message("?", "The button has been pushed, yet your crewmates are nowhere to be seen.");
                        return;
                    }
                    hasBeenUsed = true;
                    MeetingEffect();
                    EmergencyMeetingServerRpc(playerHeldBy.transform.position, playerHeldBy.playerClientId,
                                              playerList.Select(p => p.OwnerClientId).ToArray());
                    DestroyObjectServerRpc(StartOfRound.Instance.localPlayerController.playerClientId);
                }
                else
                    Effects.Message("Meetings can only be held inside the facility", "");
            }
        }

        private void MeetingEffect()  // display emergency meeting canvas
        {
            Effects.Audio(5, 1.5f);
            var meetingCanvas = Instantiate(canvasPrefab);
            Destroy(meetingCanvas, 5f);
        }

        [ServerRpc(RequireOwnership = false)]
        private void EmergencyMeetingServerRpc(Vector3 position, ulong originalPlayerId, ulong[] targetedClientIds)
        {
            ClientRpcParams clientParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = targetedClientIds } };
            var player = StartOfRound.Instance.allPlayerScripts[originalPlayerId];
            bool ship = player.isInHangarShipRoom && player.isInElevator;
            bool interior = player.isInsideFactory;
            bool exterior = !ship && !interior;  // should not be needed but just in case
            EmergencyMeetingClientRpc(position, ship, exterior, interior, clientParams);  // teleport every valid players to the user
        }

        [ClientRpc]
        private void EmergencyMeetingClientRpc(Vector3 position, bool ship, bool exterior, bool interior, ClientRpcParams clientRpcParams = default)
        {
            MeetingEffect();
            Effects.Teleportation(StartOfRound.Instance.localPlayerController, position, ship, exterior, interior);
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
