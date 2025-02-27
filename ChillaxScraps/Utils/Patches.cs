using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ChillaxScraps.Utils
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class ChillaxPlayerControllerBPatch
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

        [HarmonyPrefix]
        [HarmonyPatch("ScrollMouse_performed")]
        public static bool ScrollMouse_performedDarkBookPatch(PlayerControllerB __instance)
        {
            return CustomEffects.DarkBook.PreventPocket(__instance);
        }
    }

    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("UseNestSpawnObject")]
        public static void UseNestSpawnObjectPatch(EnemyAI __instance, ref EnemyAINestSpawnObject nestSpawnObject)
        {
            if (CustomEffects.Ocarina.WakeOldBirdFlag && __instance.enemyType == GetEnemies.OldBird.enemyType)
            {
                EnemyAINestSpawnObject[] array = Object.FindObjectsByType<EnemyAINestSpawnObject>(FindObjectsSortMode.None);
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].enemyType == GetEnemies.OldBird.enemyType && array[i].transform.position == CustomEffects.Ocarina.WakeOldBirdPosition)
                    {
                        nestSpawnObject = array[i];
                    }
                }
                CustomEffects.Ocarina.WakeOldBirdFlag = false;
            }
        }
    }
}
