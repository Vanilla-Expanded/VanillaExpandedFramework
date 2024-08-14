using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using static Verse.PawnCapacityUtility;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public static class Pawn_ApparelTracker_Notify_ApparelAdded_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var extension = apparel?.def.GetModExtension<ApparelExtension>();
            if (extension != null && !extension.equippedStatFactors.NullOrEmpty())
            {
                __instance.pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public static class Pawn_ApparelTracker_Notify_ApparelRemoved_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var extension = apparel?.def.GetModExtension<ApparelExtension>();
            if (extension != null && !extension.equippedStatFactors.NullOrEmpty())
            {
                __instance.pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class StatWorker_GetValueUnfinalized_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var statOffsetFromGearMethod = AccessTools.Method(typeof(StatWorker), nameof(StatWorker.StatOffsetFromGear));
            var getItemMethod = AccessTools.Method(typeof(List<Apparel>), "get_Item");
            bool found = false;
            object apparelIdx = 20;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (found)
                    continue;
                if (codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].Calls(getItemMethod))
                {
                    apparelIdx = codes[i].operand;
                }
                else if (codes[i].opcode == OpCodes.Stloc_0 && codes[i - 1].opcode == OpCodes.Add && codes[i - 2].Calls(statOffsetFromGearMethod))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), "apparel"));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_ApparelTracker), "get_WornApparel"));
                    yield return new CodeInstruction(OpCodes.Ldloc_S, apparelIdx);
                    yield return new CodeInstruction(OpCodes.Callvirt, getItemMethod);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Transpiler), nameof(StatFactorFromGear)));
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static float StatFactorFromGear(Apparel gear, StatDef stat)
        {
            var extension = gear?.def.GetModExtension<ApparelExtension>();
            if (extension != null)
            {
                if (!extension.equippedStatFactors.NullOrEmpty())
                {
                    return extension.equippedStatFactors.GetStatFactorFromList(stat);
                }
            }
            return 1f;
        }
    }

    [HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
    public static class StatWorker_GetExplanationUnfinalized_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var appendLineMethod = AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder lb && lb.LocalIndex == 24
                    && codes[i + 1].opcode == OpCodes.Ldloc_S && codes[i + 1].operand is LocalBuilder lb2 && lb2.LocalIndex == 24)
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 24);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatWorker_GetExplanationUnfinalized_Transpiler), nameof(ModifyValue)));
                }
            }
        }

        public static void ModifyValue(ref StringBuilder stringBuilder, Apparel apparel, StatDef stat)
        {
            if (GearAffectsStat(apparel.def, stat))
            {
                stringBuilder.AppendLine(InfoTextLineFromGear(apparel, stat));
            }
        }

        private static bool GearAffectsStat(ThingDef gearDef, StatDef stat)
        {
            var extension = gearDef.GetModExtension<ApparelExtension>();
            if (extension != null && extension.equippedStatFactors != null)
            {
                for (int i = 0; i < extension.equippedStatFactors.Count; i++)
                {
                    if (extension.equippedStatFactors[i].stat == stat && extension.equippedStatFactors[i].value != 1f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string InfoTextLineFromGear(Thing gear, StatDef stat)
        {
            var extension = gear.def.GetModExtension<ApparelExtension>();
            float f = extension.equippedStatFactors.GetStatFactorFromList(stat); ;
            return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.finalizeEquippedStatOffset ? stat.toStringStyle : stat.ToStringStyleUnfinalized, ToStringNumberSense.Factor);
        }
    }



    [HarmonyPatch(typeof(ThingDef), "DescriptionDetailed", MethodType.Getter)]
    public static class ThingDef_StatOffsetFromGear_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var appendLineMethod = AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Pop && codes[i - 1].Calls(appendLineMethod) && codes[i + 1].opcode == OpCodes.Ldc_I4_0 && codes[i + 2].opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ThingDef_StatOffsetFromGear_Transpiler), nameof(ModifyValue)));
                }
            }
        }
        public static void ModifyValue(ref StringBuilder stringBuilder, ThingDef thingDef)
        {
            var extension = thingDef.GetModExtension<ApparelExtension>();
            if (extension != null)
            {
                if (extension.equippedStatFactors != null && extension.equippedStatFactors.Count > 0)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    for (int i = 0; i < extension.equippedStatFactors.Count; i++)
                    {
                        if (i > 0)
                        {
                            stringBuilder.AppendLine();
                        }
                        var statModifier = extension.equippedStatFactors[i];
                        stringBuilder.Append($"{statModifier.stat.LabelCap}: {statModifier.stat.Worker.ValueToString(statModifier.value, finalized: false, ToStringNumberSense.Factor)}");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
    public static class ThingDef_SpecialDisplayStats_Patch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingDef __instance, StatRequest req)
        {
            foreach (var r in __result)
            {
                yield return r;
            }
            var apparelExtension = __instance.GetModExtension<ApparelExtension>();
            if (apparelExtension != null && !apparelExtension.equippedStatFactors.NullOrEmpty())
            {
                if (!apparelExtension.equippedStatFactors.NullOrEmpty())
                {
                    for (int k = 0; k < apparelExtension.equippedStatFactors.Count; k++)
                    {
                        var stat = apparelExtension.equippedStatFactors[k].stat;
                        float num3 = apparelExtension.equippedStatFactors[k].value;
                        var stringBuilder5 = new StringBuilder(stat.description);
                        if (req.HasThing && stat.Worker != null)
                        {
                            stringBuilder5.AppendLine();
                            stringBuilder5.AppendLine();
                            stringBuilder5.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(num3, ToStringNumberSense.Factor, stat.finalizeEquippedStatOffset));
                            num3 = StatWorker_GetValueUnfinalized_Transpiler.StatFactorFromGear(req.Thing as Apparel, stat);
                            if (!stat.parts.NullOrEmpty())
                            {
                                stringBuilder5.AppendLine();
                                for (int l = 0; l < stat.parts.Count; l++)
                                {
                                    string text = stat.parts[l].ExplanationPart(req);
                                    if (!text.NullOrEmpty())
                                    {
                                        stringBuilder5.AppendLine(text);
                                    }
                                }
                            }
                            stringBuilder5.AppendLine();
                            stringBuilder5.AppendLine("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(num3, ToStringNumberSense.Factor, !stat.formatString.NullOrEmpty()));
                        }
                        yield return new StatDrawEntry(VFEDefOf.VFE_EquippedStatFactors, apparelExtension.equippedStatFactors[k].stat, num3, StatRequest.ForEmpty(), ToStringNumberSense.Factor, null, forceUnfinalizedMode: true).SetReportText(stringBuilder5.ToString());
                    }
                }
            }

            var thingExtension = __instance.GetModExtension<ThingDefExtension>();
            if (thingExtension?.constructionSkillRequirement != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics,
                    "SkillRequiredToBuild".Translate(thingExtension.constructionSkillRequirement.skill.LabelCap),
                    thingExtension.constructionSkillRequirement.level.ToString(),
                    "SkillRequiredToBuildExplanation".Translate(thingExtension.constructionSkillRequirement.skill.LabelCap), 1100);
            }
        }
    }

    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
    public static class PawnCapacityUtility_CalculateCapacityLevel
    {
        public static void Postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity, List<CapacityImpactor> impactors = null, bool forTradePrice = false)
        {
            if (diffSet.pawn?.apparel != null)
            {
                var minLevels = new List<float>();
                foreach (var apparel in diffSet.pawn.apparel.WornApparel)
                {
                    var extension = apparel.def.GetModExtension<ApparelExtension>();
                    if (extension?.pawnCapacityMinLevels != null)
                    {
                        var minCapacity = extension.pawnCapacityMinLevels.FirstOrDefault(x => x.capacity == capacity);
                        if (minCapacity != null)
                        {
                            minLevels.Add(minCapacity.minLevel);
                        }
                    }
                }

                if (minLevels.Any())
                {
                    float maxLevel = minLevels.Max();
                    __result = Mathf.Max(__result, maxLevel);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "TryDrop")]
    [HarmonyPatch(new Type[]
{
            typeof(Apparel),
            typeof(Apparel),
            typeof(IntVec3),
            typeof(bool)
}, new ArgumentType[]
{
            0,
            ArgumentType.Ref,
            0,
            0
})]
    public static class Pawn_ApparelTracker_TryDrop_Patch
    {
        public static void Postfix(Pawn ___pawn, bool __result, Apparel ap, ref Apparel resultingAp, IntVec3 pos, bool forbid = true)
        {
            if (__result && ___pawn != null)
            {
                if (resultingAp is Apparel_Shield newShield)
                {
                    newShield.CompShield.equippedOffHand = false;
                    var comp = newShield.GetComp<CompEquippable>();
                    if (comp != null)
                    {
                        foreach (var verb in comp.AllVerbs)
                        {
                            verb.caster = null;
                            verb.Reset();
                        }
                    }
                }
                var extension = resultingAp?.def.GetModExtension<ApparelExtension>();
                if (extension != null)
                {
                    if (___pawn.story?.traits != null)
                    {
                        if (extension.traitsOnEquip != null)
                        {
                            foreach (var traitDef in extension.traitsOnEquip)
                            {
                                var trait = ___pawn.story.traits.GetTrait(traitDef);
                                if (trait != null)
                                {
                                    ___pawn.story.traits.RemoveTrait(trait);
                                }
                            }
                        }
                        if (extension.traitsOnUnequip != null)
                        {
                            foreach (var traitDef in extension.traitsOnUnequip)
                            {
                                if (!___pawn.story.traits.HasTrait(traitDef))
                                {
                                    var trait = new Trait(traitDef);
                                    ___pawn.story.traits.GainTrait(trait);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove))]
    public class Pawn_ApparelTracker_Remove_Patch
    {
        public static void Postfix(Pawn ___pawn, Apparel ap)
        {
            if (!Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch)
            {
                if (___pawn != null)
                {
                    var extension = ap?.def.GetModExtension<ApparelExtension>();
                    if (extension != null)
                    {
                        if (___pawn.story?.traits != null)
                        {
                            if (___pawn.story?.traits != null)
                            {
                                if (extension.traitsOnEquip != null)
                                {
                                    foreach (var traitDef in extension.traitsOnEquip)
                                    {
                                        var trait = ___pawn.story.traits.GetTrait(traitDef);
                                        if (trait != null)
                                        {
                                            ___pawn.story.traits.RemoveTrait(trait);
                                        }
                                    }
                                }

                                if (extension.traitsOnUnequip != null)
                                {
                                    foreach (var traitDef in extension.traitsOnUnequip)
                                    {
                                        if (!___pawn.story.traits.HasTrait(traitDef))
                                        {
                                            var trait = new Trait(traitDef);
                                            ___pawn.story.traits.GainTrait(trait);
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

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
    public static class Pawn_ApparelTracker_Wear_Patch
    {
        public static bool doNotRunTraitsPatch;
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
        {
            VerbUtility.TryModifyThingsVerbs(newApparel);
            if (newApparel is Apparel_Shield newShield)
            {
                newShield.CompShield.equippedOffHand = true;
                var comp = newShield.GetComp<CompEquippable>();
                if (comp != null)
                {
                    foreach (var verb in comp.AllVerbs)
                    {
                        verb.caster = newShield.Wearer;
                        verb.Reset();
                    }
                }
            }
            if (!doNotRunTraitsPatch)
            {
                var extension = newApparel?.def.GetModExtension<ApparelExtension>();
                if (extension != null)
                {
                    if (__instance.pawn.story?.traits != null)
                    {
                        if (extension.traitsOnEquip != null)
                        {
                            foreach (var traitDef in extension.traitsOnEquip)
                            {
                                if (!__instance.pawn.story.traits.HasTrait(traitDef))
                                {
                                    __instance.pawn.story.traits.GainTrait(new Trait(traitDef));
                                }
                            }
                        }
                        if (extension.traitsOnUnequip != null)
                        {
                            foreach (var traitDef in extension.traitsOnUnequip)
                            {
                                var trait = __instance.pawn.story.traits.GetTrait(traitDef);
                                if (trait != null)
                                {
                                    __instance.pawn.story.traits.RemoveTrait(trait);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class TraitSet_GainTrait_Patch
    {
        public static bool Prefix(Pawn ___pawn, Trait trait)
        {
            var traitExtension = trait?.def?.GetModExtension<TraitExtension>();
            if (traitExtension != null && traitExtension.apparelExclusiveTrait)
            {
                var apparels = ___pawn.apparel?.WornApparel;
                if (apparels != null)
                {
                    foreach (var apparel in apparels)
                    {
                        var apparelExtension = apparel.def.GetModExtension<ApparelExtension>();
                        if (apparelExtension?.traitsOnEquip != null && apparelExtension.traitsOnEquip.Contains(trait.def))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}
