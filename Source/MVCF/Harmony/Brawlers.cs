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
    public class Brawlers
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Alert_BrawlerHasRangedWeapon), "GetReport"),
                new HarmonyMethod(typeof(Brawlers), "GetReport_Prefix"));
            harm.Patch(AccessTools.Method(typeof(ThoughtWorker_IsCarryingRangedWeapon), "CurrentStateInternal"),
                new HarmonyMethod(typeof(Brawlers), "CurrentStateInternal_Prefix"));
            harm.Patch(AccessTools.Method(typeof(HealthCardUtility), "GenerateSurgeryOption"),
                postfix: new HarmonyMethod(typeof(Brawlers), "GenerateSurgeryOption_Postfix"));
            harm.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                postfix: new HarmonyMethod(typeof(Brawlers), "AddHumanlikeOrders_Postfix"));
        }

        public static bool GetReport_Prefix(ref AlertReport __result)
        {
            var brawlersWithNonEquipmentRanged = new List<Pawn>();
            foreach (var pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                if (pawn.story.traits.HasTrait(TraitDefOf.Brawler) && pawn.AllRangedVerbsPawn().Any())
                    brawlersWithNonEquipmentRanged.Add(pawn);

            __result = AlertReport.CulpritsAre(brawlersWithNonEquipmentRanged);

            return false;
        }

        public static bool CurrentStateInternal_Prefix(ref ThoughtState __result, Pawn p)
        {
            __result = p.AllRangedVerbsPawn().Any();
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
            foreach (var opt in opts)
                if (opt.Label.Contains(str.Translate((NamedArgument) apparel.LabelShort, (NamedArgument) apparel)))
                    opt.Label += " " + "EquipWarningBrawler".Translate();
        }
    }
}