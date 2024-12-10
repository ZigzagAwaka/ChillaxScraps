using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DarkBook : PhysicsProp
    {
        public bool canUseDeathNote = true;
        public bool isOpened = false;
        public List<PlayerControllerB> playerList;
        public List<EnemyAI> enemyList;
        public DarkBookCanvas canvas;
        public GameObject canvasPrefab;

        public virtual void ActivateDeathNote(GameObject objectToKill)
        {
        }

        public void SetControlTips()
        {
            string[] allLines = (canUseDeathNote ? new string[1] { "Write a name : [RMB]" } : new string[1] { "" });
            if (IsOwner)
            {
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public void CloseDeathNote()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            isOpened = false;
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

        [ServerRpc(RequireOwnership = false)]
        public void AudioServerRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume = default)
        {
            AudioClientRpc(audioID, clientPosition, hostVolume, clientVolume == default ? hostVolume : clientVolume);
        }

        [ClientRpc]
        public void AudioClientRpc(int audioID, Vector3 clientPosition, float hostVolume, float clientVolume)
        {
            Effects.Audio(audioID, clientPosition, hostVolume, clientVolume, playerHeldBy);
        }
    }
}
