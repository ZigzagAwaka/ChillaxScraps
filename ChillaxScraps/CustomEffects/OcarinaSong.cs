using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal enum Condition
    {
        None,
        Invalid,
        IsPlayerInsideFactory,
        IsPlayerOutsideFactory,
        IsPlayerFacingDoor,
        IsTimeAfternoon,
        IsTimeNight
    }

    internal class OcarinaSong
    {
        public string title;
        public Color color;
        public static float colorMultiplicator = 2f;
        public Func<Ocarina, PlayerControllerB, int, bool>? effect;
        public int usage = 0;
        public int maxUsage;
        public Condition[] conditions;

        public OcarinaSong(string title, Color color, Func<Ocarina, PlayerControllerB, int, bool>? effect,
                            int maxUsage, params Condition[] conditions)
        {
            this.title = title;
            this.color = color;
            this.effect = effect;
            this.maxUsage = maxUsage;
            this.conditions = conditions;
        }

        public (bool, int) IsValid(PlayerControllerB player, bool isOcarinaRestricted)
        {
            var result = (false, 0);
            if (player != null && (usage < maxUsage || !isOcarinaRestricted))
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    switch (conditions[i])
                    {
                        case Condition.None: result = (true, i); break;
                        case Condition.Invalid: result = (false, i); break;
                        case Condition.IsPlayerInsideFactory: result = (player.isInsideFactory, i); break;
                        case Condition.IsPlayerOutsideFactory: result = (!player.isInsideFactory, i); break;
                        case Condition.IsPlayerFacingDoor: result = (Effects.IsPlayerFacingObject<DoorLock>(player, out var door, 3f) && door.isLocked && !door.isPickingLock, i); break;
                        case Condition.IsTimeAfternoon: result = (TimeOfDay.Instance.currentDayTime >= 360, i); break;
                        case Condition.IsTimeNight: result = (TimeOfDay.Instance.currentDayTime >= 720, i); break;
                        default: break;
                    }
                    if (result.Item1 == true)
                        return result;
                }
            }
            return result;
        }

        public bool StartEffect(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (ocarina == null || player == null || effect == null)
                return false;
            return effect(ocarina, player, variationId);
        }

        public static bool ZeldaLullaby(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
                if (Effects.IsPlayerFacingObject<DoorLock>(player, out var door, 3f) && door.isLocked && !door.isPickingLock)
                    ocarina.StartCoroutine(ocarina.OpenDoorZeldaStyle(door));
                else
                    return false;
            }
            else
            {
                //var p = player.beamUpParticle; //3.2
            }
            return true;
        }

        public static bool EponaSong(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SunSong(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SariaSong(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SongOfTime(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SongOfStorms(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SongOfHealing(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SongOfSoaring(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool SonataOfAwakening(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool GoronLullaby(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool NewWaveBossaNova(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool ElegyOfEmptiness(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }

        public static bool OathToOrder(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (variationId == 0)
            {
            }
            return true;
        }
    }
}
