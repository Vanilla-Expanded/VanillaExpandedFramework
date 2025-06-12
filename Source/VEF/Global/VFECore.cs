using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF
{

    public class VEF_Mod : Mod
    {
        public static Harmony harmonyInstance;
        public const string LateHarmonyPatchCategory = "LateHarmonyPatch";

        public VEF_Mod(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VEF");
            harmonyInstance.PatchAllUncategorized();
            // Delay running patches that still need to wait, for example ones that rely on
            // some condition to run (like defs having a comp or def mod extension).
            LongEventHandler.ExecuteWhenFinished(() => harmonyInstance.PatchCategory(LateHarmonyPatchCategory));
        }

        
    }

}
