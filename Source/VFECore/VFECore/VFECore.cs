using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public class VFECore : Mod
    {
        public const string LateHarmonyPatchCategory = "LateHarmonyPatch";

        public VFECore(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFECore");
            harmonyInstance.PatchAllUncategorized();
            // Delay running patches that still need to wait, for example ones that rely on
            // some condition to run (like defs having a comp or def mod extension).
            LongEventHandler.ExecuteWhenFinished(() => harmonyInstance.PatchCategory(LateHarmonyPatchCategory));
        }

        public static Harmony harmonyInstance;
    }

}
