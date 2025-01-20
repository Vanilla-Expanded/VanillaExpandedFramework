using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaMemesExpanded
{

    [HarmonyPatch(typeof(Dialog_ChooseMemes))]
    [HarmonyPatch("DoAcceptChanges")]
    public static class VanillaMemesExpanded_Dialog_ChooseMemes_DoAcceptChanges_Patch
    {
        [HarmonyPostfix]
        static void ForceTraitAndAbilitiesOnChooseMemeDialog(List<MemeDef> ___newMemes, Ideo ___ideo)
        {
            foreach (MemeDef meme in ___newMemes)
            {
                ExtendedMemeProperties extendedMemeProps = meme.GetModExtension<ExtendedMemeProperties>();
                if (extendedMemeProps != null)
                {
                    if (extendedMemeProps.forcedTrait != null)
                    {
                        foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
                        {
                            if (pawn.Ideo == ___ideo)
                            {
                                Trait trait = new Trait(extendedMemeProps.forcedTrait, 0, true);
                                pawn.story?.traits?.GainTrait(trait);
                            }
                        }                     
                    }
                    if (extendedMemeProps.abilitiesGiven != null)
                    {
                        foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
                        {
                            if (pawn.Ideo == ___ideo)
                            {
                                foreach (AbilityDef ability in extendedMemeProps.abilitiesGiven)
                                {
                                    pawn.abilities?.GainAbility(ability);
                                }
                            }
                        }                     
                    }
                }
            }
        }
    }
}
