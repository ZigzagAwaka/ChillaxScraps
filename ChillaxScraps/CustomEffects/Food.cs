using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Food : PhysicsProp
    {
        public int healPower = 50;
        public float staminaPower = 0.5f;
        public Vector3 originalPosition = new Vector3(-0.1f, 0.1f, -0.1f);
        public Vector3 originalRotation = new Vector3(90, 0, -90);

        public Food() { }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (playerHeldBy != null && !playerHeldBy.activatingItem)
            {
                UpdatePosRotServerRpc(new Vector3(0.1f, 0.1f, 0), new Vector3(90, 90, -90));
                playerHeldBy.activatingItem = buttonDown;
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);  // start eat animation
                StartCoroutine(FoodEffect(playerHeldBy));
            }
        }

        private IEnumerator FoodEffect(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.8f);
            AudioServerRpc(3, player.transform.position, 1f, 1.5f);
            UpdatePosRotServerRpc(originalPosition, originalRotation);
            player.playerBodyAnimator.SetBool("useTZPItem", false);  // stop eat animation
            player.activatingItem = false;
            if (!player.isPlayerDead && !StartOfRound.Instance.inShipPhase)
            {
                ulong playerID = StartOfRound.Instance.localPlayerController.playerClientId;
                if (player.sprintMeter + staminaPower > 1)
                    player.sprintMeter = 1;
                else
                    player.sprintMeter += staminaPower;
                if (player.health + healPower > 100)
                    HealPlayerServerRpc(playerID, 100);
                else
                    HealPlayerServerRpc(playerID, player.health + healPower);
                HUDManager.Instance.UpdateHealthUI(player.health, false);
                DestroyObjectServerRpc(playerID);
            }
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
        private void UpdatePosRotServerRpc(Vector3 newPos, Vector3 newRot)
        {
            UpdatePosRotClientRpc(newPos, newRot);
        }

        [ClientRpc]
        private void UpdatePosRotClientRpc(Vector3 newPos, Vector3 newRot)
        {
            itemProperties.positionOffset = newPos;
            itemProperties.rotationOffset = newRot;
        }

        [ServerRpc(RequireOwnership = false)]
        private void HealPlayerServerRpc(ulong playerID, int health)
        {
            HealPlayerClientRpc(playerID, health);
        }

        [ClientRpc]
        private void HealPlayerClientRpc(ulong playerID, int health)
        {
            Effects.Heal(playerID, health);
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
