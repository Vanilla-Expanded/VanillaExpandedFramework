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

        public VEF_Mod(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VEF");
            VEF_HarmonyCategories.TryPatchAll(harmonyInstance);
        }

        
    }

}
