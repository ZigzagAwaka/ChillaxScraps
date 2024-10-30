using BepInEx.Configuration;
using ChillaxScraps.Utils;
using System.Collections.Generic;

namespace ChillaxScraps
{
    class Config
    {
        public readonly ConfigEntry<int> masterSwordDmg;
        public readonly List<ConfigEntry<int>> entries = new List<ConfigEntry<int>>();

        public Config(ConfigFile cfg, List<Scrap> scraps)
        {
            cfg.SaveOnConfigSet = false;
            masterSwordDmg = cfg.Bind("Items", "Master Sword damage", 4);
            foreach (Scrap scrap in scraps)
            {
                entries.Add(cfg.Bind("Spawn chance", scrap.asset.Split("/")[0], scrap.rarity));
            }
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }
    }
}
