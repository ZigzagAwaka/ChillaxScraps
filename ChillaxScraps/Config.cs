using BepInEx.Configuration;
using ChillaxScraps.Utils;
using System.Collections.Generic;

namespace ChillaxScraps
{
    class Config
    {
        public readonly ConfigEntry<int> masterSwordDmg;
        public readonly ConfigEntry<bool> evilBoink;
        public readonly ConfigEntry<bool> ocarinaUniqueSongs;
        //public readonly ConfigEntry<int> ocarinaNbOfUse;
        public readonly List<ConfigEntry<int>> entries = new List<ConfigEntry<int>>();

        public Config(ConfigFile cfg, List<Scrap> scraps)
        {
            cfg.SaveOnConfigSet = false;
            masterSwordDmg = cfg.Bind("Items", "Master Sword damage", 4, "Only the chosen hero can grab this sword, so it's supposed to be strong.");
            evilBoink = cfg.Bind("Items", "Evil Boink", false, "Activate this to turn Boink into an evil bird, can have negative consequences.");
            ocarinaUniqueSongs = cfg.Bind("Items", "Ocarina unique songs", false, "Activate this if you want every connected player to have a randomly selected Ocarina song assigned to them.");
            //ocarinaNbOfUse = cfg.Bind("Items", "Ocarina number of use", -1, "Change the total number of usage for the Ocarina songs (each song triggers a special effect). Set this to 0 if don't want the Ocarina to have special song effects, or -1 if you want infinite usage.");
            foreach (Scrap scrap in scraps)
            {
                entries.Add(cfg.Bind("Spawn chance", scrap.asset.Split("/")[0], scrap.rarity));
            }
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }
    }
}
