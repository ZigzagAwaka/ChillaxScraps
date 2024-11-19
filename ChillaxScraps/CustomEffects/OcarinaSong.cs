using ChillaxScraps.Utils;
using GameNetcodeStuff;
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
        public int usage = 0;
        public int maxUsage;
        public Color color;
        public static float colorMultiplicator = 2f;
        public Condition[] conditions;

        public OcarinaSong(string title, int maxUsage, Color color, params Condition[] conditions)
        {
            this.title = title;
            this.maxUsage = maxUsage;
            this.color = color;
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
                        case Condition.IsPlayerFacingDoor: result = (Effects.IsPlayerFacingDoor(player, out _), i); break;
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

        public void StartEffect(Ocarina ocarina, PlayerControllerB player, int variationId)
        {
            if (player == null)
                return;
            switch (title)
            {
                case "Zelda's Lullaby": ZeldaLullaby(ocarina, player, variationId); break;
                case "Epona's Song": EponaSong(ocarina, player); break;
                case "Sun's Song": SunSong(ocarina, player); break;
                case "Saria's Song": SariaSong(ocarina, player); break;
                case "Song of Time": SongOfTime(ocarina, player); break;
                case "Song of Storms": SongOfStorms(ocarina, player); break;
                case "Song of Healing": SongOfHealing(ocarina, player); break;
                case "Song of Soaring": SongOfSoaring(ocarina, player); break;
                case "Sonata of Awakening": SonataOfAwakening(ocarina, player); break;
                case "Goron Lullaby": GoronLullaby(ocarina, player); break;
                case "New Wave Bossa Nova": NewWaveBossaNova(ocarina, player); break;
                case "Elegy of Emptiness": ElegyOfEmptiness(ocarina, player); break;
                case "Oath to Order": OathToOrder(ocarina, player); break;
                default: break;
            }
            UnityEngine.Debug.LogError("Played " + title + " : usage " + usage);
        }

        public void ZeldaLullaby(Ocarina ocarina, PlayerControllerB player, int effectId)
        {

        }

        public void EponaSong(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SunSong(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SariaSong(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SongOfTime(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SongOfStorms(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SongOfHealing(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SongOfSoaring(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void SonataOfAwakening(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void GoronLullaby(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void NewWaveBossaNova(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void ElegyOfEmptiness(Ocarina ocarina, PlayerControllerB player)
        {

        }

        public void OathToOrder(Ocarina ocarina, PlayerControllerB player)
        {

        }
    }
}
