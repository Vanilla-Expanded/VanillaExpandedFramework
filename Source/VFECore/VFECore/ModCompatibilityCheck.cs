using System.Linq;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class ModCompatibilityCheck
    {
        public static bool DualWield;

        public static bool FacialStuff;

        public static bool ResearchTree;

        public static bool ResearchPal;

        public static bool RimCities;

        public static bool RPGStyleInventory;
        public static bool RPGStyleInventoryRevamped;

        public static bool RunAndGun;

        public static bool FactionDiscovery;

        public static bool WhatTheHack;

        public static bool CombatExtended;

        public static bool HumanAlienRace;
        public static bool VFEMechanoids;

        static ModCompatibilityCheck()
        {
            var allMods = ModsConfig.ActiveModsInLoadOrder.ToList();
            for (var i = allMods.Count; i-- > 0;)
            {
                var curModName = allMods[i].Name;

                if (curModName == "Dual Wield")
                    DualWield = true;
                else if (curModName == "Facial Stuff 1.1")
                    FacialStuff = true;
                else if (curModName == "Research Tree")
                    ResearchTree = true;
                else if (curModName == "ResearchPal")
                    ResearchPal = true;
                else if (curModName == "RimCities")
                    RimCities = true;
                else if (curModName == "RPG Style Inventory Revamped")
                    RPGStyleInventoryRevamped = true;
                // Uncomment this once RPG Style Inventory updates to 1.3:
                // else if (curMod.Name == "RPG Style Inventory")
                //     RPGStyleInventory = true;
                else if (curModName == "RunAndGun")
                    RunAndGun = true;
                else if (curModName == "Faction Discovery")
                    FactionDiscovery = true;
                else if (curModName == "What the hack?!")
                    WhatTheHack = true;
                else if (curModName == "Combat Extended")
                    CombatExtended = true;
                else if (curModName == "Humanoid Alien Races" || curModName == "Humanoid Alien Races ~ Dev")
                    HumanAlienRace = true;
                else if (curModName == "Vanilla Factions Expanded - Mechanoids")
                    VFEMechanoids = true;
            }
        }
    }
}