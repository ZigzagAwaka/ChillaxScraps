using BepInEx;
using ChillaxScraps.CustomEffects;
using GameNetcodeStuff;

namespace ChillaxScraps.Utils
{
    internal class SSSConditions
    {
        public static void Setup(BepInPlugin sssMetaData)
        {
            if (new System.Version("1.0.0").CompareTo(sssMetaData.Version) <= 0)
            {
                SelfSortingStorage.Cupboard.SmartCupboard.AddTriggerValidation(ChillaxScrapsCondition, "[Item not allowed]");
            }
        }

        private static bool ChillaxScrapsCondition(PlayerControllerB player)
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
