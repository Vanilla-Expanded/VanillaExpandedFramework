using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;
using static RimWorld.FleshTypeDef;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(FleshTypeDef), "ChooseWoundOverlay")]
    public static class VanillaGenesExpanded_FleshTypeDef_ChooseWoundOverlay_Patch
    {
        public static void Postfix(Hediff hediff, ref ResolvedWound __result)
        {
            if (StaticCollectionsClass.woundsFromFleshtype_gene_pawns.ContainsKey(hediff.pawn))
            {
                var other = ChooseWoundOverlay(StaticCollectionsClass.woundsFromFleshtype_gene_pawns[hediff.pawn], hediff);
                if (other != null)
                {
                    __result = other;
                }
            }
        }

        public static ResolvedWound ChooseWoundOverlay(FleshTypeDef def, Hediff hediff)
        {
            if (def.genericWounds == null)
            {
                return null;
            }
            if (def.hediffWounds != null)
            {
                foreach (HediffWound hediffWound in def.hediffWounds)
                {
                    if (hediffWound.hediff != hediff.def)
                    {
                        continue;
                    }
                    ResolvedWound resolvedWound = hediffWound.ChooseWoundOverlay(hediff);
                    if (resolvedWound != null)
                    {
                        if (hediff.IsTended())
                        {
                            return def.ChooseBandagedOverlay();
                        }
                        return resolvedWound;
                    }
                }
            }
            Hediff_MissingPart hediff_MissingPart;
            if (hediff is Hediff_Injury || ((hediff_MissingPart = hediff as Hediff_MissingPart) != null && hediff_MissingPart.IsFresh))
            {
                if (hediff.IsTended())
                {
                    return def.ChooseBandagedOverlay();
                }
               
                if (ReflectionCache.woundsResolved(def) == null)
                {
                    ReflectionCache.woundsResolved(def)= def.genericWounds.Select((Wound wound) => wound.Resolve()).ToList();
                   
                }
                return ReflectionCache.woundsResolved(def).RandomElement();
            }
            return null;
        }
    }
}
