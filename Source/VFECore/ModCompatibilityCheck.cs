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

        public static bool DualWield = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Dual Wield");

        public static bool FacialStuff = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Facial Stuff 1.0");

        public static bool ResearchTree = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Research Tree");

        public static bool ResearchPal = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "ResearchPal");

        public static bool RimCities = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "RimCities");

        public static bool RPGStyleInventory = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[1.0] RPG Style Inventory");

    }

}
