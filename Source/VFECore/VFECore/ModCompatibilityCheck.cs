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

        static ModCompatibilityCheck()
        {
            var allMods = ModsConfig.ActiveModsInLoadOrder.ToList();
            for (var i = 0; i < allMods.Count; i++)
            {
                var curMod = allMods[i];

                if (curMod.Name == "Dual Wield")
                    DualWield = true;
                else if (curMod.Name == "Facial Stuff 1.1")
                    FacialStuff = true;
                else if (curMod.Name == "Research Tree")
                    ResearchTree = true;
                else if (curMod.Name == "ResearchPal")
                    ResearchPal = true;
                else if (curMod.Name == "RimCities")
                    RimCities = true;
                else if (curMod.Name == "RPG Style Inventory Revamped")
                    RPGStyleInventoryRevamped = true;
                // Uncomment this once RPG Style Inventory updates to 1.3:
                // else if (curMod.Name == "RPG Style Inventory")
                //     RPGStyleInventory = true;
                else if (curMod.Name == "RunAndGun")
                    RunAndGun = true;
                else if (curMod.Name == "Faction Discovery")
                    FactionDiscovery = true;
                else if (curMod.Name == "What the hack?!")
                    WhatTheHack = true;
                else if (curMod.Name == "Combat Extended")
                    CombatExtended = true;
            }
        }
    }
}