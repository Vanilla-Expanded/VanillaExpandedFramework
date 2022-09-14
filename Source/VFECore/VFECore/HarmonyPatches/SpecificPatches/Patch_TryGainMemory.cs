using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[]
    {
                typeof(Thought_Memory),
                typeof(Pawn)
    })]
    public static class TryGainMemory_Patch
    {
        private static void Postfix(MemoryThoughtHandler __instance, ref Thought_Memory newThought, Pawn otherPawn)
        {
            var options = newThought.def.GetModExtension<ThoughtExtensions>();
            if (options != null)
            {
                if (options.removeThoughtsWhenAdded != null)
                {
                    foreach (var thoughtDef in options.removeThoughtsWhenAdded)
                    {
                        __instance.pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thoughtDef);
                    }
                }
            }
        }
    }
}
