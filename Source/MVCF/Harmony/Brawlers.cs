using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Alert_BrawlerHasRangedWeapon), "GetReport")]
    public class Alert_BrawlerHasRangedWeapon_GetReport
    {
        // ReSharper disable once RedundantAssignment
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(ref AlertReport __result)
        {
            var brawlersWithNonEquipmentRanged = new List<Pawn>();
            foreach (var pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                if (pawn.story.traits.HasTrait(TraitDefOf.Brawler) && pawn.AllRangedVerbsPawn().Any())
                    brawlersWithNonEquipmentRanged.Add(pawn);

            __result = AlertReport.CulpritsAre(brawlersWithNonEquipmentRanged);

            return false;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_IsCarryingRangedWeapon), "CurrentStateInternal")]
    public class ThoughtWorker_IsCarryingRangedWeapon_CurrentStateInternal
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once RedundantAssignment
        public static bool Prefix(ref ThoughtState __result, Pawn p)
        {
            __result = p.AllRangedVerbsPawn().Any();
            return false;
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "GenerateSurgeryOption")]
    public class HealthCardUtility_GenerateSurgeryOption
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref FloatMenuOption __result, Thing thingForMedBills, RecipeDef recipe)
        {
//            Log.Message("HealthCardUtility.GenerateSurgeryOption:");
//            Log.Message("  __result: " + __result);
//            Log.Message("  thing: " + thingForMedBills);
//            Log.Message("  recipe: " + recipe);
            var pawn = thingForMedBills as Pawn;
            if (!(pawn?.story?.traits?.HasTrait(TraitDefOf.Brawler) ?? false)) return;
//            Log.Message("Pawn is brawler");
            if (recipe?.addsHediff == null) return;
            var hediff = recipe.addsHediff;
//            Log.Message("Found hediff: " + hediff.label);
            if (!hediff.HasComp(typeof(HediffComp_VerbGiver))) return;
            var comp = hediff.CompPropsFor(typeof(HediffComp_VerbGiver)) as HediffCompProperties_VerbGiver;
            var verbs = comp?.verbs;
            if (verbs == null) return;
            if (!verbs.Any(verb => !verb.IsMeleeAttack)) return;
//            Log.Message("Hediff adds ranged verbs");
            __result.Label = __result.Label + " " + "EquipWarningBrawler".Translate();
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public class FloatMenuMakerMap_AddHumanlikeOrders
    {
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static void Postfix(List<FloatMenuOption> opts, Vector3 clickPos, Pawn pawn)
        {
//            Log.Message("FloatMenuMakerMap.AddHumanlikeOrders:");
//            Log.Message("  Opts: " + opts.Count);
//            Log.Message("  clickPos: " + clickPos);
//            Log.Message("  pawn: " + pawn);
            if (pawn?.apparel == null) return;
            var apparel = pawn.Map.thingGrid.ThingAt<Apparel>(IntVec3.FromVector3(clickPos));
            if (apparel == null) return;
//            Log.Message("Found apparel: " + apparel.Label);
            var str = "ForceWear";
            if (apparel.def.apparel.LastLayer.IsUtilityLayer) str = "ForceEquipApparel";
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp == null) return;
//            Log.Message("Found comp");
            if (!comp.VerbTracker.AllVerbs.Any(verb => !verb.IsMeleeAttack)) return;
//            Log.Message("Apparel has ranged attacks");
            var traits = pawn.story?.traits;
            if (traits == null) return;
//            Log.Message("Found traits");
            if (!traits.HasTrait(TraitDefOf.Brawler)) return;
//            Log.Message("Pawn is brawler");
//            Log.Message("Looking for: " + str.Translate((NamedArgument) apparel.LabelShort, (NamedArgument) apparel));
            foreach (var opt in opts)
                //                Log.Message("  Found: " + opt.Label);
                if (opt.Label.Contains(str.Translate((NamedArgument) apparel.LabelShort, (NamedArgument) apparel)))
                    //                    Log.Message("Adding to label");
                    opt.Label += " " + "EquipWarningBrawler".Translate();
        }
    }
}