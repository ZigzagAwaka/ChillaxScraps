﻿using BepInEx.Configuration;
using ChillaxScraps.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ChillaxScraps
{
    class Config
    {
        public bool StarlancerAIFix = false;
        public bool WeatherRegistery = false;
        public readonly List<ulong> unluckyPlayersID = new List<ulong>();
        public readonly List<(int, int)> scrapValues = new List<(int, int)>();
        public readonly ConfigEntry<int> masterSwordDmg;
        public readonly ConfigEntry<bool> evilBoink;
        public readonly ConfigEntry<bool> deathnoteRechargeOrbit;
        public readonly ConfigEntry<bool> deathnoteNoLimit;
        public readonly ConfigEntry<int> freddyInvisibilityChance;
        public readonly ConfigEntry<bool> ocarinaUniqueSongs;
        public readonly ConfigEntry<bool> ocarinaRestrictUsage;
        public readonly List<ConfigEntry<int>> entries = new List<ConfigEntry<int>>();
        public readonly List<ConfigEntry<string>> values = new List<ConfigEntry<string>>();

        public Config(ConfigFile cfg, List<Scrap> scraps)
        {
            cfg.SaveOnConfigSet = false;
            masterSwordDmg = cfg.Bind("Items", "Master Sword damage", 4, "Only the chosen hero can grab this sword, so it's supposed to be strong.");
            evilBoink = cfg.Bind("Items", "Evil Boink", false, "Activate this to turn Boink into an evil bird, can have negative consequences.");
            deathnoteRechargeOrbit = cfg.Bind("Items", "Death Note recharge in orbit", false, "Allows the Death Note to automatically enter the recharge state when in orbit, if it's not already recharging.");
            deathnoteNoLimit = cfg.Bind("Items", "Death Note no limit", false, "Removes the 'one user per player' condition for the Death Note, so there is 3 max usage until the recharge mode. With this config activated the recharge mode is going to be a bit slower than usual.");
            freddyInvisibilityChance = cfg.Bind("Items", "Freddy bad effect chance", 70, new ConfigDescription("Chance in % of Freddy Fazbear starting the invisibility phase when spawning inside the facility (do some bad things).", new AcceptableValueRange<int>(0, 100)));
            ocarinaUniqueSongs = cfg.Bind("Items", "Ocarina unique songs", false, "Activate this if you want every connected player to have a randomly selected Ocarina song assigned to them.");
            ocarinaRestrictUsage = cfg.Bind("Items", "Ocarina restrict usage", true, "Restrict the usage of Ocarina songs effects to 1, 2 or more times per moons. Setting this to false allows infinite usage of the songs effects but it's recommanded to not change this as it will be unbalanced.");
            foreach (Scrap scrap in scraps)
            {
                entries.Add(cfg.Bind("Spawn chance", scrap.asset.Split("/")[0], scrap.rarity));
                values.Add(cfg.Bind("Values", scrap.asset.Split("/")[0], "", "Min,max value of the item, follow the format 200,300 or empty for default.\nIn-game value will be randomized between these numbers and divided by 2.5."));
            }
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("AudioKnight.StarlancerAIFix"))
            {
                StarlancerAIFix = true;
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrov.WeatherRegistry"))
            {
                WeatherRegistery = true;
            }
            foreach (var value in values)
            {
                if (value.Value == "")
                { scrapValues.Add((-1, -1)); continue; }
                var valueTab = value.Value.Split(',').Select(s => s.Trim()).ToArray();
                if (valueTab.Count() != 2)
                { scrapValues.Add((-1, -1)); continue; }
                if (!int.TryParse(valueTab[0], out var minV) || !int.TryParse(valueTab[1], out var maxV))
                { scrapValues.Add((-1, -1)); continue; }
                if (minV > maxV)
                { scrapValues.Add((-1, -1)); continue; }
                scrapValues.Add((minV, maxV));
            }
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
