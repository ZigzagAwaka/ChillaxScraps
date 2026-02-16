using ChillaxScraps.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class UnoReverseDX : PhysicsProp
    {
        public bool rechargeState = false;
        public bool canBeUsed = true;
        public float rechargeTime = 0f;
        public float timeNeededForRecharching = 0f;
        public readonly float rechargeTimeMin = 60;
        public readonly float rechargeTimeMax = 180;
        public readonly float maxLerp = 1f;
        public readonly float maxEmissive = 9f;
        public readonly float maxLightPower = 0.05f;
        public readonly float maxHolographic = 1.5f;
        public readonly float maxLightIntensity = 1.5f;
        public MeshRenderer? meshRenderer;
        public Light? light;
        private readonly float smoothTime = 10f;
        private float velocity0, velocity1, velocity2, velocity3, velocity4;

        public UnoReverseDX() { }

        internal class SwapInfo
        {
            public Vector3 position;
            public (bool, bool, bool) positionFlags;
            public ClientRpcParams rpcParams;

            public SwapInfo(Vector3 pos, bool ship, bool exterior, bool interior, ClientRpcParams client)
            {
                position = pos;
                positionFlags = (ship, exterior, interior);
                rpcParams = client;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            light = transform.GetChild(2).GetComponent<Light>();
            if (!IsHost && !IsServer)
                SyncCardStateServerRpc();
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
                HUDManager.Instance.ClearControlTips();
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
            if (rechargeState && timeNeededForRecharching != 0f)
            {
                rechargeTime += Time.deltaTime;
                if (rechargeTime >= timeNeededForRecharching && meshRenderer != null && light != null)
                {
                    UpdateCardClientRpc(false, rechargeState, timeNeededForRecharching,
                        Mathf.SmoothDamp(meshRenderer.material.GetFloat("_TextureLerp"), maxLerp, ref velocity0, smoothTime),
                        Mathf.SmoothDamp(meshRenderer.material.GetFloat("_EmissivePower"), maxEmissive, ref velocity1, smoothTime),
                        Mathf.SmoothDamp(meshRenderer.material.GetFloat("_LightPower"), maxLightPower, ref velocity2, smoothTime),
                        Mathf.SmoothDamp(meshRenderer.material.GetFloat("_HolographicPower"), maxHolographic, ref velocity3, smoothTime),
                        Mathf.SmoothDamp(light.intensity, maxLightIntensity, ref velocity4, smoothTime));
                    if (light.intensity + 0.1 >= maxLightIntensity)
                    {
                        UpdateCardClientRpc(true, false, 0f, maxLerp, maxEmissive, maxLightPower, maxHolographic, maxLightIntensity);
                        velocity0 = 0f; velocity1 = 0f; velocity2 = 0f; velocity3 = 0f; velocity4 = 0f;
                        rechargeTime = 0f;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartSwapServerRpc()
        {
            var playerList = Effects.GetPlayers();
            if (playerList.Count <= 1)
                return;
            var swapInfo = new List<SwapInfo>();
            for (var i = 0; i < playerList.Count; i++)
            {
                var player = playerList[i];
                var destination = playerList[i + 1 == playerList.Count ? 0 : i + 1];
                var position = UnoReverse.GetPosition(destination);
                ClientRpcParams clientParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { player.actualClientId } } };
                bool ship = destination.isInHangarShipRoom && destination.isInElevator;
                bool interior = destination.isInsideFactory;
                bool exterior = !ship && !interior;
                swapInfo.Add(new SwapInfo(position, ship, exterior, interior, clientParams));
            }
            foreach (var swap in swapInfo)
                StartSwapClientRpc(swap.position, swap.positionFlags.Item1, swap.positionFlags.Item2, swap.positionFlags.Item3, swap.rpcParams);
            UpdateCardClientRpc(false, true, Random.Range(rechargeTimeMin, rechargeTimeMax), 0f, 0f, 0f, 0f, 0f);
        }

        [ClientRpc]
        private void StartSwapClientRpc(Vector3 positionToSwap, bool ship, bool exterior, bool interior, ClientRpcParams clientRpcParams = default)
        {
            Effects.Audio(4, 1f);
            TeleportationServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, positionToSwap, ship, exterior, interior);
        }

        [ClientRpc]
        private void UpdateCardClientRpc(bool usable, bool recharching, float rechargeTimer, float lerpValue, float emissiveValue, float lightValue, float holographicValue, float lightIntensity)
        {
            var originalUsable = canBeUsed;
            canBeUsed = usable;
            rechargeState = recharching;
            timeNeededForRecharching = rechargeTimer;
            if (meshRenderer != null)
            {
                meshRenderer.material.SetFloat("_TextureLerp", lerpValue);
                meshRenderer.material.SetFloat("_EmissivePower", emissiveValue);
                meshRenderer.material.SetFloat("_LightPower", lightValue);
                meshRenderer.material.SetFloat("_HolographicPower", holographicValue);
            }
            if (light != null)
                light.intensity = lightIntensity;
            if (IsOwner && isHeld && !isPocketed && originalUsable != canBeUsed)
                SetControlTips();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SyncCardStateServerRpc()
        {
            if (canBeUsed || meshRenderer == null || light == null)
                return;
            SyncCardStateClientRpc(canBeUsed, rechargeState, timeNeededForRecharching, rechargeTime, meshRenderer.material.GetFloat("_TextureLerp"),
                meshRenderer.material.GetFloat("_EmissivePower"), meshRenderer.material.GetFloat("_LightPower"), meshRenderer.material.GetFloat("_HolographicPower"),
                light.intensity, velocity0, velocity1, velocity2, velocity3, velocity4);
        }

        [ClientRpc]
        private void SyncCardStateClientRpc(bool usable, bool recharching, float rechargeTimer, float actualRechargeTime,
            float lerpValue, float emissiveValue, float lightValue, float holographicValue, float lightIntensity,
            float vel0, float vel1, float vel2, float vel3, float vel4)
        {
            rechargeTime = actualRechargeTime;
            velocity0 = vel0; velocity1 = vel1; velocity2 = vel2; velocity3 = vel3; velocity4 = vel4;
            UpdateCardClientRpc(usable, recharching, rechargeTimer, lerpValue, emissiveValue, lightValue, holographicValue, lightIntensity);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TeleportationServerRpc(ulong playerID, Vector3 position, bool ship, bool exterior, bool interior)
        {
            TeleportationClientRpc(playerID, position, ship, exterior, interior);
        }

        [ClientRpc]
        private void TeleportationClientRpc(ulong playerID, Vector3 position, bool ship, bool exterior, bool interior)
        {
            Effects.TeleportationLocal(StartOfRound.Instance.allPlayerScripts[playerID], position);
            Effects.SetPosFlags(playerID, ship, exterior, interior);
        }
    }
}
