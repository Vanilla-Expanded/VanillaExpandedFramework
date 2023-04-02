using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_Brawlers : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(Alert_BrawlerHasRangedWeapon), nameof(Alert_BrawlerHasRangedWeapon.GetReport)),
            AccessTools.Method(GetType(), nameof(GetReport_Prefix)));
        yield return Patch.Prefix(AccessTools.Method(typeof(ThoughtWorker_IsCarryingRangedWeapon), "CurrentStateInternal"),
            AccessTools.Method(GetType(), nameof(CurrentStateInternal_Prefix)));
        yield return Patch.Postfix(AccessTools.Method(typeof(HealthCardUtility), "GenerateSurgeryOption"),
            AccessTools.Method(GetType(), nameof(GenerateSurgeryOption_Postfix)));
        yield return Patch.Postfix(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
            AccessTools.Method(GetType(), nameof(AddHumanlikeOrders_Postfix)));
    }

    public static bool GetReport_Prefix(ref AlertReport __result)
    {
        var brawlersWithNonEquipmentRanged = PawnsFinder.AllMaps_FreeColonistsSpawned.Where(pawn =>
                pawn.story.traits.HasTrait(TraitDefOf.Brawler) && pawn.Manager().ShouldBrawlerUpset)
           .ToList();

        __result = AlertReport.CulpritsAre(brawlersWithNonEquipmentRanged);

        return false;
    }

    public static bool CurrentStateInternal_Prefix(ref ThoughtState __result, Pawn p)
    {
        __result = p.Manager().ShouldBrawlerUpset;
        return false;
    }

    public static void GenerateSurgeryOption_Postfix(ref FloatMenuOption __result, Thing thingForMedBills,
        RecipeDef recipe)
    {
        var pawn = thingForMedBills as Pawn;
        if (!(pawn?.story?.traits?.HasTrait(TraitDefOf.Brawler) ?? false)) return;
        if (recipe?.addsHediff == null) return;
        var hediff = recipe.addsHediff;
        if (!hediff.HasComp(typeof(HediffComp_VerbGiver))) return;
        var comp = hediff.CompPropsFor(typeof(HediffComp_VerbGiver)) as HediffCompProperties_VerbGiver;
        var verbs = comp?.verbs;
        if (verbs == null) return;
        if (!verbs.Any(verb => !verb.IsMeleeAttack)) return;
        __result.Label = __result.Label + " " + "EquipWarningBrawler".Translate();
    }

    public static void AddHumanlikeOrders_Postfix(List<FloatMenuOption> opts, Vector3 clickPos, Pawn pawn)
    {
        if (pawn?.apparel == null) return;
        var apparel = pawn.Map.thingGrid.ThingAt<Apparel>(IntVec3.FromVector3(clickPos));
        if (apparel == null) return;
        var str = "ForceWear";
        if (apparel.def.apparel.LastLayer.IsUtilityLayer) str = "ForceEquipApparel";
        var comp = apparel.TryGetComp<Comp_VerbGiver>();
        if (comp == null) return;
        if (!comp.VerbTracker.AllVerbs.Any(verb => !verb.IsMeleeAttack)) return;
        var traits = pawn.story?.traits;
        if (traits == null) return;
        if (!traits.HasTrait(TraitDefOf.Brawler)) return;
        foreach (var opt in opts.Where(opt =>
                     opt.Label.Contains(str.Translate((NamedArgument)apparel.LabelShort, (NamedArgument)apparel))))
            opt.Label += " " + "EquipWarningBrawler".Translate();
    }
}
