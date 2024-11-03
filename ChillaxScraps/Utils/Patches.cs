using GameNetcodeStuff;
using HarmonyLib;

namespace ChillaxScraps.Utils
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class TotemItemPlayerControllerBPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("DamagePlayer")]
        public static void DamagePlayerTotemPrePatch(PlayerControllerB __instance, int damageNumber)
        {
            if (__instance.health - damageNumber <= 0)
                CustomEffects.TotemOfUndying.TrySavePlayer(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch("KillPlayer")]
        public static void KillPlayerTotemPrePatch(PlayerControllerB __instance)
        {
            CustomEffects.TotemOfUndying.TrySavePlayer(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("DamagePlayer")]
        public static void DamagePlayerTotemPostPatch(PlayerControllerB __instance, int damageNumber)
        {
            if (__instance.health - damageNumber <= 0)
                CustomEffects.TotemOfUndying.TryDestroyItem(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("KillPlayer")]
        public static void KillPlayerTotemPostPatch(PlayerControllerB __instance)
        {
            CustomEffects.TotemOfUndying.TryDestroyItem(__instance);
        }
    }
}
