using BepInEx.Configuration;
using ChillaxScraps.Utils;
using System.Collections.Generic;

namespace ChillaxScraps
{
    class Config
    {
        public readonly List<ulong> unluckyPlayersID = new List<ulong>();
        public readonly ConfigEntry<int> masterSwordDmg;
        public readonly ConfigEntry<bool> evilBoink;
        public readonly ConfigEntry<bool> ocarinaUniqueSongs;
        public readonly ConfigEntry<bool> ocarinaRestrictUsage;
        public readonly List<ConfigEntry<int>> entries = new List<ConfigEntry<int>>();

        public Config(ConfigFile cfg, List<Scrap> scraps)
        {
            cfg.SaveOnConfigSet = false;
            masterSwordDmg = cfg.Bind("Items", "Master Sword damage", 4, "Only the chosen hero can grab this sword, so it's supposed to be strong.");
            evilBoink = cfg.Bind("Items", "Evil Boink", false, "Activate this to turn Boink into an evil bird, can have negative consequences.");
            ocarinaUniqueSongs = cfg.Bind("Items", "Ocarina unique songs", false, "Activate this if you want every connected player to have a randomly selected Ocarina song assigned to them.");
            ocarinaRestrictUsage = cfg.Bind("Items", "Ocarina restrict usage", true, "Restrict the usage of Ocarina songs effects to 1, 2 or more times per moons. Setting this to false allows infinite usage of the songs effects but it's recommanded to not change this as it will be unbalanced.");
            foreach (Scrap scrap in scraps)
            {
                entries.Add(cfg.Bind("Spawn chance", scrap.asset.Split("/")[0], scrap.rarity));
            }
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupUnluckyPlayersConfig()
        {
            if (PremiumScraps.Plugin.config != null && PremiumScraps.Plugin.config.unluckyPlayersID != null)
            {
                foreach (var id in PremiumScraps.Plugin.config.unluckyPlayersID)
                    unluckyPlayersID.Add(id);
            }
        }
    }
}
