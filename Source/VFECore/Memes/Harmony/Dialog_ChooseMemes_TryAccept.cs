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
    [HarmonyPatch("TryAccept")]
    public static class VanillaMemesExpanded_Dialog_ChooseMemes_TryAccept_Patch
    {
        [HarmonyPrefix]
        public static bool DetectIfPairedMeme(ref List<MemeDef> ___newMemes)

        {
           
            for (int i = 0; i < ___newMemes.Count; i++)
            {
                ExtendedMemeProperties extendedMemeProps = ___newMemes[i].GetModExtension<ExtendedMemeProperties>();
                if (extendedMemeProps != null && extendedMemeProps.pairedMeme!=null)
                {
                    if (!___newMemes.Contains(DefDatabase<MemeDef>.GetNamedSilentFail(extendedMemeProps.pairedMeme)))
                    {
                        Messages.Message("VME_MessageNeedsThePairedMeme".Translate(___newMemes[i].label, DefDatabase<MemeDef>.GetNamedSilentFail(extendedMemeProps.pairedMeme)?.label), MessageTypeDefOf.RejectInput, false);
                        return false;
                    }
                   
                }





            }
            return true;


        }
    }
}
