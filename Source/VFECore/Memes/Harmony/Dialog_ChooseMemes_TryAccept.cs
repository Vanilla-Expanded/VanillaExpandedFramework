using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

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
                if (extendedMemeProps != null && extendedMemeProps.requiredMemes!=null)
                {
                    bool flagAnyFound = false;
                    List<string> memeNamesList = new List<string>();
                    foreach (string requiredMeme in extendedMemeProps.requiredMemes)
                    {
                        MemeDef meme = DefDatabase<MemeDef>.GetNamedSilentFail(requiredMeme);

                        if (meme != null)
                        {
                            memeNamesList.Add(meme.LabelCap);
                            if (___newMemes.Contains(meme))
                            {
                                flagAnyFound = true;
                            }
                        }
                    }
                    if (!flagAnyFound) {
                        
                     
                        
                        string memeNames = string.Join(", ", memeNamesList);
                        Messages.Message("VME_MessageNeedsThePairedMeme".Translate(___newMemes[i].label, memeNames), MessageTypeDefOf.RejectInput, false);
                        return false;
                        

                    }
                    
                   
                }





            }
            return true;


        }
    }
}
