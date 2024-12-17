using ChillaxScraps.Utils;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class UnoReverseDX : PhysicsProp
    {
        public bool rechargeState = false;
        public bool canBeUsed = true;
        public readonly float rechargeTimeMin = 60;
        public readonly float rechargeTimeMax = 180;
        public readonly float maxLight = 1.5f;
        public MeshRenderer? meshRenderer;
        public Light? light;

        public UnoReverseDX() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            light = transform.GetChild(2).GetComponent<Light>();
        }

        public override void EquipItem()
        {
            SetControlTips();
            EnableItemMeshes(enable: true);
            isPocketed = false;
            if (!hasBeenHeld)
            {
                hasBeenHeld = true;
                if (!isInShipRoom && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
                {
                    RoundManager.Instance.valueOfFoundScrapItems += scrapValue;
                }
            }
        }

        public override void SetControlTipsForItem()
        {
            SetControlTips();
        }

        public void SetControlTips()
        {
            string[] allLines = (canBeUsed ? new string[2] { "Use the card : [RMB]", "Inspect: [Z]" } : new string[2] { "Recharging...", "Inspect: [Z]" });
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null && canBeUsed)
            {
                if (StartOfRound.Instance.inShipPhase)
                {
                    Effects.Message("Can't be used at the moment", "");
                    return;
                }
                var playerList = Effects.GetPlayers();
                if (playerList.Count <= 1)
                {
                    Effects.Message("Huh ?", "No players to swap with...");
                    return;
                }
                StartSwapServerRpc();
            }
        }

        public override void Update()
        {
            base.Update();


        }

        [ServerRpc(RequireOwnership = false)]
        private void StartSwapServerRpc()
        {
            var playerList = Effects.GetPlayers();
            if (playerList.Count <= 1)
                return;
            for (var i = 0; i < playerList.Count; i++)
            {
                var player = playerList[i];
                var destination = playerList[i + 1 == playerList.Count ? 0 : i + 1];
                var position = UnoReverse.GetPosition(destination);
                ClientRpcParams clientParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { player.actualClientId } } };
                bool ship = destination.isInHangarShipRoom && destination.isInElevator;
                bool interior = destination.isInsideFactory;
                bool exterior = !ship && !interior;
                StartSwapClientRpc(position, ship, exterior, interior, clientParams);
            }
            UpdateCardClientRpc(false, true, 0f, 0f);
        }

        [ClientRpc]
        private void StartSwapClientRpc(Vector3 positionToSwap, bool ship, bool exterior, bool interior, ClientRpcParams clientRpcParams = default)
        {
            Effects.Audio(4, 1f);
            Effects.Teleportation(StartOfRound.Instance.localPlayerController, positionToSwap);
            SetPosFlagsServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, ship, exterior, interior);
        }

        [ClientRpc]
        private void UpdateCardClientRpc(bool usable, bool recharching, float lerpValue, float lightIntensity)
        {
            canBeUsed = usable;
            rechargeState = recharching;
            meshRenderer?.material.SetFloat("_TextureLerp", lerpValue);
            if (light != null)
                light.intensity = lightIntensity;
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
    }
}
