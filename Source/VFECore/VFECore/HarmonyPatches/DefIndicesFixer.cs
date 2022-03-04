using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace VFECore
{
	[HarmonyPatch(typeof(PlayerKnowledgeDatabase), "ReloadAndRebind")]
	public static class DefIndicesFixer
    {
        public static bool ranAlready;
		public static void Prefix()
        {
            if (!ranAlready)
            {
                FixIndices();
                ranAlready = true;
            }
        }

        private static void FixIndices() // this method fixes Tynan's bug with index assigning where modded defs derived from vanilla would get repeating indices
                                         // thus breaking some logic, i.e terrain wealth calculation was broken when adding mods with modded terrain def types
        {
            Dictionary<Type, HashSet<int>> idsByDefTypes = new Dictionary<Type, HashSet<int>>();
            foreach (Type item in typeof(Def).AllSubclasses())
            {
                if (item != typeof(BuildableDef))
                {
                    var defsList = GenDefDatabase.GetAllDefsInDatabaseForDef(item).ToList();
                    foreach (var def in defsList)
                    {
                        if (!idsByDefTypes.TryGetValue(item, out var list))
                        {
                            idsByDefTypes[item] = list = new HashSet<int>();
                        }
                        if (list.Contains(def.index))
                        {
                            def.index = (ushort)(list.Max() + 1);
                        }
                        if (!list.Add(def.index))
                        {
                            Log.Error("Failed to assign non duplicate index to " + def + " - " + def.index);
                        }
                    }
                }
            }
        }
    }
}
