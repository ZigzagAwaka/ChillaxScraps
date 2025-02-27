using BepInEx;
using ChillaxScraps.CustomEffects;
using GameNetcodeStuff;

namespace ChillaxScraps.Utils
{
    internal class ShipInventoryConditions
    {
        public static void Setup(BepInPlugin inventoryMetadata)
        {
            if (new System.Version("1.2.2").CompareTo(inventoryMetadata.Version) <= 0)
            {
                ShipInventory.Helpers.InteractionHelper.AddCondition(ChillaxScrapsCondition, "[Item not allowed]");
            }
        }

        public static bool ChillaxScrapsCondition(PlayerControllerB player)
        {
            var item = player.currentlyHeldObjectServer;
            if (((item.itemProperties.name == "DeathNoteItem" || item.itemProperties.name == "DanceNoteItem") && item is DarkBook) ||
                (item.itemProperties.name == "MasterSwordItem" && item is MasterSword) ||
                (item.itemProperties.name == "NokiaItem" && item is Nokia) ||
                (item.itemProperties.name == "OcarinaItem" && item is Ocarina ocarina && (ocarina.isPlaying || !StartOfRound.Instance.inShipPhase)) ||
                (item.itemProperties.name == "TotemOfUndyingItem" && item is TotemOfUndying totemofundying && totemofundying.used) ||
                (item.itemProperties.name == "UnoReverseCardDXItem" && item is UnoReverseDX unoreversedx && !unoreversedx.canBeUsed))
                return false;
            return true;
        }
    }
}
