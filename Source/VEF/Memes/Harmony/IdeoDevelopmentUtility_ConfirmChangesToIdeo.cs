using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Memes
{

    [HarmonyPatch(typeof(IdeoDevelopmentUtility))]
    [HarmonyPatch("ConfirmChangesToIdeo")]
    public static class VanillaExpandedFramework_IdeoDevelopmentUtility_ConfirmChangesToIdeo_Patch
    {
        [HarmonyPostfix]
        public static void ForceTraitAndAbilitiesOnReformIdeoDialog(Ideo newIdeo)
        {

            foreach (MemeDef meme in newIdeo.memes)
            {
                ExtendedMemeProperties extendedMemeProps = meme.GetModExtension<ExtendedMemeProperties>();
                if (extendedMemeProps != null)
                {
                    if (extendedMemeProps.forcedTrait != null)
                    {
                        foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
                        {
                            
                            if (pawn.Ideo?.memes.Contains(meme) == true)
                            {
                                Trait trait = new Trait(extendedMemeProps.forcedTrait, 0, true);
                                if (pawn.story?.traits?.HasTrait(trait.def) == false)
                                {
                                    pawn.story?.traits?.GainTrait(trait);
                                }
                                
                            }
                        }
                    }
                    if (extendedMemeProps.abilitiesGiven != null)
                    {
                        foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
                        {
                           
                            if (pawn.Ideo?.memes.Contains(meme)==true)
                            {
                                foreach (AbilityDef ability in extendedMemeProps.abilitiesGiven)
                                {
                                    if (pawn.abilities!=null && pawn.abilities.GetAbility(ability) is null)
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
}
