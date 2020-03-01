using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class ModCompatibilityCheck
    {

        static ModCompatibilityCheck()
        {
            var allMods = ModsConfig.ActiveModsInLoadOrder.ToList();
            for (int i = 0; i < allMods.Count; i++)
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
                else if (curMod.Name == "[1.1] RPG Style Inventory")
                    RPGStyleInventory = true;
                else if (curMod.Name == "RunAndGun")
                    RunAndGun = true;
            }
        }

        public static bool DualWield;

        public static bool FacialStuff;

        public static bool ResearchTree;

        public static bool ResearchPal;

        public static bool RimCities;

        public static bool RPGStyleInventory;

        public static bool RunAndGun;

    }

}
