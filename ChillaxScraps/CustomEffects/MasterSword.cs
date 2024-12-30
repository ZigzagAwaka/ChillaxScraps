using ChillaxScraps.Utils;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class MasterSword : Shovel
    {
        public bool heroIsSelected = false;
        public ulong heroSteamId = 0;
        private bool firstTimeGrab = false;
        private readonly float unworthyWeight = 3f;

        public MasterSword() { }

        private bool IsUnworthy()
        {
            return heroIsSelected && GameNetworkManager.Instance.localPlayerController.playerSteamId != heroSteamId;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            shovelHitForce = Plugin.config.masterSwordDmg.Value;
            if (!heroIsSelected)
                SelectHeroServerRpc();
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

        private void SetControlTips()
        {
            string[] allLines = (IsUnworthy() ? new string[1] { "" } : new string[1] { "Swing sword : [RMB]" });
            if (IsOwner)
            {
                HUDManager.Instance.ClearControlTips();
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (IsUnworthy())
            {
                Effects.Message("You aren't the one who's worthy of holding that blade", "");
                playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight + (unworthyWeight - 1f), 1f, 10f);
            }
            else if (!firstTimeGrab)
            {
                firstTimeGrab = true;
                AudioServerRpc(8, playerHeldBy.transform.position, 0.8f, 0.55f);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (IsUnworthy())
                return;
            base.ItemActivate(used, buttonDown);
        }

        public override void DiscardItem()
        {
            if (IsUnworthy() && playerHeldBy != null)
                playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (unworthyWeight - 1f), 1f, 10f);
            base.DiscardItem();
        }

        public override void OnNetworkDespawn()
        {
            if (IsUnworthy() && playerHeldBy != null)
                playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (unworthyWeight - 1f), 1f, 10f);
            base.OnNetworkDespawn();
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
