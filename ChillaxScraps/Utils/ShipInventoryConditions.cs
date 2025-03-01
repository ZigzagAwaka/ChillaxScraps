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

        private static bool ChillaxScrapsCondition(PlayerControllerB player)
        {
            var item = player.currentlyHeldObjectServer;
            if (((item.itemProperties.name == "DeathNoteItem" || item.itemProperties.name == "DanceNoteItem") && item is DarkBook) ||
                (item.itemProperties.name == "MasterSwordItem" && item is MasterSword) ||
                (item.itemProperties.name == "NokiaItem" && item is Nokia) ||
                (item.itemProperties.name == "OcarinaItem" && item is Ocarina) ||
                (item.itemProperties.name == "TotemOfUndyingItem" && item is TotemOfUndying) ||
                (item.itemProperties.name == "UnoReverseCardDXItem" && item is UnoReverseDX) ||
                (item.itemProperties.name == "FreddyFazbearItem" && item is Freddy) ||
                (item.itemProperties.name == "UnoReverseCardItem" && item is UnoReverse))
                return false;
            return true;
        }
    }
}
