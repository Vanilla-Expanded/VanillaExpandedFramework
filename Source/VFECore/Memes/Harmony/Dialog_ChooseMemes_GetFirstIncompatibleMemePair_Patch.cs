using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;

namespace VanillaMemesExpanded
{

    [HarmonyPatch(typeof(Dialog_ChooseMemes))]
    [HarmonyPatch("GetFirstIncompatibleMemePair")]
    public static class VanillaMemesExpanded_Dialog_ChooseMemes_GetFirstIncompatibleMemePair_Patch
    {
        [HarmonyPostfix]
        public static void DetectIfRequiredMeme(ref List<MemeDef> ___newMemes, ref Pair<MemeDef, MemeDef> __result)

        {
            List<MemeDef> memesTemp = ___newMemes;
            for (int i = 0; i < ___newMemes.Count; i++)
            {
                ExtendedMemeProperties extendedMemeProps = ___newMemes[i].GetModExtension<ExtendedMemeProperties>();
                if (extendedMemeProps != null && extendedMemeProps.neededMeme!=null)
                {

                    List<MemeDef> structureMemeDefs = (from k in DefDatabase<MemeDef>.AllDefsListForReading
                                                       where k.category == MemeCategory.Structure && memesTemp.Contains(k)
                                                       select k).ToList();

                    foreach (MemeDef memeDef in structureMemeDefs)
                    {
                        if (___newMemes[i].GetModExtension<ExtendedMemeProperties>().neededMeme != memeDef.defName)
                        {

                            __result = new Pair<MemeDef, MemeDef>(___newMemes[i], memeDef);
                        }
                    }
                }





            }


        }
    }
}
