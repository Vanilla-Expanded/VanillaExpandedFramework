using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using static Verse.PawnCapacityUtility;
using VEF.Pawns;
using VEF.Things;

namespace VEF.Apparels
{
    public static class ApparelExtensionUtilities
    {
        public static bool doNotRunTraitsPatch;

        public static bool GearAffectsStats(Thing gear, StatDef stat)
        {
            var extension = gear?.def.GetModExtension<ApparelExtension>();
            if (extension == null)
                return false;

            if (!extension.equippedStatFactors.NullOrEmpty())
            {
                var f = extension.equippedStatFactors.GetStatFactorFromList(stat);
                if (f != 1f)
                    return true;
            }

            return false;
        }

        internal static bool GearAffectsStatsWrapper(bool original, Thing gear, StatDef stat) => original || GearAffectsStats(gear, stat);

        public static void EquipGear(Pawn pawn, Thing gear)
        {
            if (pawn == null)
                return;
            var extension = gear?.def.GetModExtension<ApparelExtension>();
            if (extension == null)
                return;

            if ((!doNotRunTraitsPatch || !VanillaExpandedFramework_Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch) && pawn.story?.traits != null)
            {
                if (extension.traitsOnEquip != null)
                {
                    AddTraits(extension.traitsOnEquip, pawn);
                }
                if (extension.traitsOnUnequip != null)
                {
                    RemoveTraits(extension.traitsOnUnequip, pawn);
                }
            }

            if (extension.workDisables != WorkTags.None)
            {
                pawn.Notify_DisabledWorkTypesChanged();
            }

            if (!extension.equippedStatFactors.NullOrEmpty())
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        public static void UnequipGear(Pawn pawn, Thing gear)
        {
            if (pawn == null)
                return;
            var extension = gear?.def.GetModExtension<ApparelExtension>();
            if (extension == null)
                return;

            if ((!doNotRunTraitsPatch || !VanillaExpandedFramework_Pawn_ApparelTracker_Wear_Patch.doNotRunTraitsPatch) && pawn.story?.traits != null)
            {
                if (extension.traitsOnEquip != null)
                {
                    RemoveTraits(extension.traitsOnEquip, pawn);
                }
                if (extension.traitsOnUnequip != null)
                {
                    AddTraits(extension.traitsOnUnequip, pawn);
                }
            }

            if (extension.workDisables != WorkTags.None)
            {
                pawn.Notify_DisabledWorkTypesChanged();
            }

            if (!extension.equippedStatFactors.NullOrEmpty())
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        private static void AddTraits(List<TraitRequirement> traits, Pawn pawn)
        {
            foreach (var traitDef in traits)
            {
                var hasTrait = false;

                foreach (var trait in pawn.story.traits.allTraits)
                {
                    if (trait.sourceGene == null && trait.def == traitDef.def)
                    {
                        hasTrait = true;
                        break;
                    }
                }

                if (!hasTrait)
                {
                    pawn.story.traits.GainTrait(new Trait(traitDef.def, traitDef.degree.GetValueOrDefault()), true);
                }
            }
        }

        private static void RemoveTraits(List<TraitRequirement> traits, Pawn pawn)
        {
            foreach (var traitDef in traits)
            {
                for (var i = pawn.story.traits.allTraits.Count - 1; i >= 0; i--)
                {
                    var trait = pawn.story.traits.allTraits[i];
                    if (trait.sourceGene == null && trait.def == traitDef.def && (traitDef.degree == null || traitDef.degree == trait.Degree))
                    {
                        pawn.story.traits.RemoveTrait(trait, true);
                    }
                }
            }
        }

        public static float GetStatFactor(Thing gear, StatDef stat)
        {
            return (gear?.def.GetModExtension<ApparelExtension>()?.equippedStatFactors).GetStatFactorFromList(stat);
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
    public static class VanillaExpandedFramework_Pawn_ApparelTracker_Notify_ApparelAdded_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (apparel is Apparel_Shield newShield)
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

            ApparelExtensionUtilities.EquipGear(__instance.pawn, apparel);
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
    public static class VanillaExpandedFramework_Pawn_ApparelTracker_Notify_ApparelRemoved_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (__instance.pawn != null && apparel is Apparel_Shield newShield)
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

            ApparelExtensionUtilities.UnequipGear(__instance.pawn, apparel);
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_EquipmentAdded))]
    public static class VanillaExpandedFramework_Pawn_EquipmentTracker_Notify_EquipmentAdded_Patch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            ApparelExtensionUtilities.EquipGear(__instance.pawn, eq);
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_EquipmentRemoved))]
    public static class VanillaExpandedFramework_Pawn_EquipmentTracker_Notify_EquipmentRemoved_Patch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            ApparelExtensionUtilities.UnequipGear(__instance.pawn, eq);
        }
    }

    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class VanillaExpandedFramework_StatWorker_GetValueUnfinalized_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var statOffsetFromGearMethod = AccessTools.Method(typeof(StatWorker), nameof(StatWorker.StatOffsetFromGear));
            var getItemMethod = typeof(List<Apparel>).DeclaredIndexerGetter([typeof(int)]);
            var getPrimaryMethod = typeof(Pawn_EquipmentTracker).DeclaredPropertyGetter(nameof(Pawn_EquipmentTracker.Primary));
            var statField = typeof(StatWorker).DeclaredField("stat");
            var foundApparel = false;
            var foundGear = false;
            object apparelIdx = 20;
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (!foundApparel && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].Calls(getItemMethod))
                {
                    apparelIdx = codes[i].operand;
                }
                else if (codes[i].opcode == OpCodes.Stloc_0 && codes[i - 1].opcode == OpCodes.Add && codes[i - 2].Calls(statOffsetFromGearMethod))
                {
                    if (!foundApparel && codes[i - 5].Calls(getItemMethod))
                    {
                        foundApparel = true;
                        // Load the current value as reference
                        yield return CodeInstruction.LoadLocal(0, true);
                        // Load pawn.apparel.WornApparel
                        yield return CodeInstruction.LoadLocal(1);
                        yield return CodeInstruction.LoadField(typeof(Pawn), nameof(Pawn.apparel));
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(Pawn_ApparelTracker).DeclaredPropertyGetter(nameof(Pawn_ApparelTracker.WornApparel)));
                        yield return new CodeInstruction(OpCodes.Ldloc_S, apparelIdx);
                        yield return new CodeInstruction(OpCodes.Callvirt, getItemMethod);
                        // Load this.stat
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, statField);
                        // Call our method
                        yield return CodeInstruction.Call(() => ModifyStatsForGear);
                    }
                    else if (!foundGear && codes[i - 5].Calls(getPrimaryMethod))
                    {
                        foundGear = true;
                        // Load the current value as reference
                        yield return CodeInstruction.LoadLocal(0, true);
                        // Load pawn.equipment
                        yield return CodeInstruction.LoadLocal(1);
                        yield return CodeInstruction.LoadField(typeof(Pawn), nameof(Pawn.equipment));
                        // Skip the call to "Pawn_EquipmentTracker.Primary" in here so we can grab all equipment (should handle dual wield sidearms)
                        // Load this.stat
                        yield return CodeInstruction.LoadArgument(0);
                        yield return new CodeInstruction(OpCodes.Ldfld, statField);
                        // Call our method
                        yield return CodeInstruction.Call(() => ModifyStatsForAllEquipment);
                    }
                }
            }

            if (!foundApparel)
                Log.Error("[VEF] Failed patching stat factors for apparel.");
            if (!foundGear)
                Log.Error("[VEF] Failed patching stat factors for gear.");
        }

        private static void ModifyStatsForAllEquipment(ref float value, Pawn_EquipmentTracker equipment, StatDef stat)
        {
            foreach (var eq in equipment.AllEquipmentListForReading)
                ModifyStatsForGear(ref value, eq, stat);
        }

        public static void ModifyStatsForGear(ref float value, Thing gear, StatDef stat)
        {
            var extension = gear?.def.GetModExtension<ApparelExtension>();
            if (extension != null)
            {
                if (!extension.equippedStatFactors.NullOrEmpty())
                {
                    value *= extension.equippedStatFactors.GetStatFactorFromList(stat);
                }
            }
        }

        public static float StatFactorFromGear(Apparel gear, StatDef stat)
        {
            // TODO: Used by Vanilla Gravships Expanded. Patch it out and remove here.
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

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetOffsetsAndFactorsExplanation))]
    public static class VanillaExpandedFramework_StatWorker_GetOffsetsAndFactorsExplanation_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var infoTextLineFromGearMethod = typeof(StatWorker).DeclaredMethod("InfoTextLineFromGear");
            var appendLineMethod = typeof(StringBuilder).DeclaredMethod(nameof(StringBuilder.AppendLine), [typeof(string)]);
            var gearAffectsStatMethod = typeof(StatWorker).DeclaredMethod("GearAffectsStat");

            var matcher = new CodeMatcher(instructions);

            // We need to insert our factors into the method
            var matches = new List<CodeMatch>
            {
                // Loads the StringBuilder
                CodeMatch.LoadsArgument(),
                // Loads the "whitespace" string argument
                CodeMatch.LoadsArgument(),
                // Loads the "apparel" local
                CodeMatch.LoadsLocal(),
                // Loads this.stat
                CodeMatch.LoadsArgument(),
                new(OpCodes.Ldfld),
                // Calls StatWorker.InfoTextLineFromGear with the 2 prior arguments
                CodeMatch.Calls(infoTextLineFromGearMethod),
                // Concats the result from the prior method with whitespace string argument
                CodeMatch.Calls((MethodInfo)null),
                // Calls StringBuilder.AppendLine
                CodeMatch.Calls(appendLineMethod),
                // Pops the result from prior method (call to StringBuilder.AppendLine returns the StringBuilder for chained calls)
                new(OpCodes.Pop)
            };

            var gearAffectStatsMatches = new List<CodeMatch>
            {
                // Loads apparel/equipment's def
                CodeMatch.LoadsLocal(),
                new(OpCodes.Ldfld),
                // Loads "this.stat"
                CodeMatch.LoadsArgument(),
                new(OpCodes.Ldfld),
                // Calls the GearAffectsStat method
                CodeMatch.Calls(gearAffectsStatMethod),
                // Jumps over the "yield return" if false or jumps towards it if true
                CodeMatch.Branches()
            };

            var gearAffectStatsOffset = -5;
            var gearAffectStatsRemoveOffset = 1;
            var appendLineOffsets = -6;

            foreach (var match in new[] { "apparel", "gear" })
            {
                // Go back to start
                matcher.Reset();

                // Insert 2 extra search instructions for gear specifically.
                if (match == "gear")
                {
                    // Replace "pawn" (ldloc) with "pawn.equipment.Primary" (load field and call a getter)
                    matches.InsertRange(3, [new CodeMatch(OpCodes.Ldfld), CodeMatch.Calls((MethodInfo)null)]);
                    // Replace "pawn.equipment" (ldloc/ldfld) with "pawn.equipment.Primary.def (call a getter and get a field)
                    gearAffectStatsMatches.InsertRange(2, [CodeMatch.Calls((MethodInfo)null), new CodeMatch(OpCodes.Ldfld)]);
                    gearAffectStatsOffset = -7;
                    appendLineOffsets = -8;
                    gearAffectStatsRemoveOffset = 3;
                }

                matcher.MatchEndForward(matches.ToArray());

                if (matcher.IsValid)
                {
                    // Replace the previous instruction (AppendLine call) with our own.
                    // Don't remove/insert in case something adds labels here or whatever.
                    matcher.Advance(-1);
                    matcher.Instruction.opcode = OpCodes.Call;
                    matcher.Operand = SymbolExtensions.GetMethodInfo(() => AppendOffsetsAndFactors);
                    // Copy loading the whitespace, gear, and stat instructions.
                    matcher.Insert(matcher.InstructionsWithOffsets(appendLineOffsets, -3));
                }
                else
                    Log.Error($"[VEF] Failed patching stat explanations for {match}. Equipped {match} stat factors won't be displayed for pawns.");

                matcher.Reset();

                // Look for apparel
                matcher.MatchEndForward(gearAffectStatsMatches.ToArray());

                if (matcher.IsValid)
                {
                    // Insert before the branch instruction cloned instructions to load the ThingDef and StatDef.
                    // After that call our method, which will grab the result from the original method and the 2 values we've loaded in.
                    matcher.Insert(matcher
                        .InstructionsWithOffsets(gearAffectStatsOffset, -2)
                        .Select(x => x.Clone())
                        .Concat(CodeInstruction.Call(() => ApparelExtensionUtilities.GearAffectsStatsWrapper))
                    );

                    // Advance 1 position (loading the "def" field from Thing) and remove it (we want a Thing, not its def).
                    matcher.Advance(gearAffectStatsRemoveOffset);
                    matcher.RemoveInstruction();
                }
                else
                    Log.Error($"[VEF] Failed patching stat explanations for {match}. Equipped stat factors may not be displayed on pawns, and hyperlinks to relevant gear not included.");
            }

            return matcher.Instructions();
        }

        public static StringBuilder AppendOffsetsAndFactors(StringBuilder sb, string baseText, string whitespace, Thing gear, StatDef stat)
        {
            // We need to include offset, since we've replaced this call.
            // Call the original method in case some other mod patches InfoTextLineFromGear method.
            var baseOffset = StatWorker.StatOffsetFromGear(gear, stat);
            if (baseOffset != 0f)
                sb.AppendLine(baseText);

            var extension = gear.def.GetModExtension<ApparelExtension>();
            if (extension == null)
                return sb;

            if (!extension.equippedStatFactors.NullOrEmpty())
            {
                var baseFactor = extension.equippedStatFactors.GetStatFactorFromList(stat);
                if (baseFactor != 1f)
                    sb.AppendLine($"{whitespace}    {gear.LabelCap}: {baseFactor.ToStringByStyle(stat.finalizeEquippedStatOffset ? stat.toStringStyle : stat.ToStringStyleUnfinalized, ToStringNumberSense.Factor)}");
            }

            return sb;
        }
    }

    [HarmonyPatch(typeof(StatWorker), "RelevantGear", MethodType.Enumerator)]
    public static class VanillaExpandedFramework_StatWorker_RelevantGear_Transpiler
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var gearAffectsStatMethod = typeof(StatWorker).DeclaredMethod("GearAffectsStat");
    
            var matcher = new CodeMatcher(instr);
            matcher.Start();

            foreach (var match in new[] { "apparel", "gear" })
            {
                // Look for apparel
                matcher.MatchEndForward(
                    // Loads apparel/equipment's def
                    CodeMatch.LoadsLocal(),
                    new CodeMatch(OpCodes.Ldfld),
                    // Loads "this.stat"
                    CodeMatch.LoadsArgument(),
                    new CodeMatch(OpCodes.Ldfld),
                    // Calls the GearAffectsStat method
                    CodeMatch.Calls(gearAffectsStatMethod),
                    // Jumps over the "yield return" if false or jumps towards it if true
                    CodeMatch.Branches()
                );

                if (matcher.IsValid)
                {
                    // Insert before the branch instruction cloned instructions to load the ThingDef and StatDef.
                    // After that call our method, which will grab the result from the original method and the 2 values we've loaded in.
                    matcher.Insert(matcher
                        .InstructionsWithOffsets(-5, -2)
                        .Select(x => x.Clone())
                        .Concat(CodeInstruction.Call(() => ApparelExtensionUtilities.GearAffectsStatsWrapper))
                    );

                    // Advance 1 position (loading the "def" field from Thing) and remove it (we want a Thing, not its def).
                    matcher.Advance();
                    matcher.RemoveInstruction();
                }
                else
                    Log.Error($"[VEF] Failed patching stat explanations for {match}. Equipped stat factors may not be displayed on pawns, and hyperlinks to relevant gear not included.");
            }
    
            return matcher.Instructions();
        }
    }



    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.DescriptionDetailed), MethodType.Getter)]
    public static class VanillaExpandedFramework_ThingDef_DescriptionDetailed_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var stringBuilderConstructor = typeof(StringBuilder).Constructor([]);
            var toStringMethod = typeof(object).Method(nameof(ToString));

            var matcher = new CodeMatcher(instructions);

            // Find local index for the string builder
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Newobj, stringBuilderConstructor),
                CodeMatch.IsStloc()
            );
            var stringBuilderIndex = matcher.Instruction.LocalIndex();

            // Look right before descriptionDetailedCached is assigned a value
            matcher.MatchStartForward(
                CodeMatch.IsLdarg(),
                CodeMatch.IsLdloc(),
                CodeMatch.Calls(toStringMethod),
                new CodeMatch(OpCodes.Stfld)
            );

            matcher.Insert(
                // Load StringBuilder
                CodeInstruction.LoadLocal(stringBuilderIndex).MoveLabelsFrom(matcher.Instruction),
                // Load "this" argument
                CodeInstruction.LoadArgument(0),
                CodeInstruction.Call(() => ModifyValue)
            );

            return matcher.Instructions();
        }

        public static void ModifyValue(StringBuilder stringBuilder, ThingDef thingDef)
        {
            // Vanilla (for whatever reason) only supports apparel.
            // We can either do the same, or we'd have to include normal stat offsets
            // as well, as it would be weird to include factors without offsets.
            if (!thingDef.IsApparel) return;

            var extension = thingDef.GetModExtension<ApparelExtension>();
            if (extension == null) return;

            if (!extension.equippedStatFactors.NullOrEmpty())
            {
                var linesAppended = !thingDef.equippedStatOffsets.NullOrEmpty();

                if (!linesAppended)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }

                for (int i = 0; i < extension.equippedStatFactors.Count; i++)
                {
                    if (i > 0 || linesAppended)
                    {
                        stringBuilder.AppendLine();
                    }
                    var statModifier = extension.equippedStatFactors[i];
                    stringBuilder.Append($"{statModifier.stat.LabelCap}: {statModifier.stat.Worker.ValueToString(statModifier.value, finalized: false, ToStringNumberSense.Factor)}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
    public static class VanillaExpandedFramework_ThingDef_SpecialDisplayStats_Patch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingDef __instance, StatRequest req)
        {
            foreach (var r in __result)
            {
                yield return r;
            }

            var apparelExtension = __instance.GetModExtension<ApparelExtension>();
            if (apparelExtension != null)
            {
                if (!apparelExtension.equippedStatFactors.NullOrEmpty())
                {
                    for (var i = 0; i < apparelExtension.equippedStatFactors.Count; i++)
                    {
                        var stat = apparelExtension.equippedStatFactors[i].stat;
                        var factor = apparelExtension.equippedStatFactors[i].value;
                        var sb = new StringBuilder(stat.description);

                        if (req.HasThing && stat.Worker != null)
                        {
                            sb.AppendLine();
                            sb.AppendLine();
                            sb.AppendLine($"{"StatsReport_BaseValue".Translate()}: {stat.ValueToString(factor, ToStringNumberSense.Factor, stat.finalizeEquippedStatOffset)}");
                            factor = ApparelExtensionUtilities.GetStatFactor(req.Thing, stat);
                            if (!stat.parts.NullOrEmpty())
                            {
                                sb.AppendLine();
                                for (var k = 0; k < stat.parts.Count; k++)
                                {
                                    var text = stat.parts[k].ExplanationPart(req);
                                    if (!text.NullOrEmpty())
                                    {
                                        sb.AppendLine(text);
                                    }
                                }
                            }
                            sb.AppendLine();
                            sb.AppendLine($"{"StatsReport_FinalValue".Translate()}: {stat.ValueToString(factor, ToStringNumberSense.Factor, !stat.formatString.NullOrEmpty())}");
                        }

                        yield return new StatDrawEntry(VEFDefOf.VFE_EquippedStatFactors, apparelExtension.equippedStatFactors[i].stat, factor, StatRequest.ForEmpty(), ToStringNumberSense.Factor, forceUnfinalizedMode: true).SetReportText(sb.ToString());
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
    public static class VanillaExpandedFramework_PawnCapacityUtility_CalculateCapacityLevel
    {
        public static void Postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity, List<CapacityImpactor> impactors = null, bool forTradePrice = false)
        {
            if (diffSet?.pawn == null)
                return;

            var minLevel = float.NegativeInfinity;
            if (diffSet.pawn.apparel != null)
            {
                foreach (var apparel in diffSet.pawn.apparel.WornApparel)
                {
                    var extension = apparel.def.GetModExtension<ApparelExtension>();
                    var minCapacity = extension?.pawnCapacityMinLevels?.FirstOrDefault(x => x.capacity == capacity);
                    if (minCapacity != null)
                    {
                        if (minCapacity.minLevel > minLevel)
                            minLevel = minCapacity.minLevel;
                        impactors?.Add(new CapacityImpactorGearMinLevel { gear = apparel, extension = extension, capacity = capacity });
                    }
                }
            }

            if (diffSet.pawn.equipment != null)
            {
                foreach (var equipment in diffSet.pawn.equipment.AllEquipmentListForReading)
                {
                    var extension = equipment.def.GetModExtension<ApparelExtension>();
                    var minCapacity = extension?.pawnCapacityMinLevels?.FirstOrDefault(x => x.capacity == capacity);
                    if (minCapacity != null)
                    {
                        if (minCapacity.minLevel > minLevel)
                            minLevel = minCapacity.minLevel;
                        impactors?.Add(new CapacityImpactorGearMinLevel { gear = equipment, extension = extension, capacity = capacity });
                    }
                }
            }

            if (!float.IsInfinity(minLevel) && !float.IsNaN(minLevel) && minLevel > __result)
            {
                __result = GenMath.RoundedHundredth(minLevel);
            }
        }
    }

    public static class VanillaExpandedFramework_Pawn_ApparelTracker_Wear_Patch
    {
        // Used by VFE-Pirates. Keep around until patched out in Pirates and remove in the future.
        // As far as I'm aware, unused by other mods.
        // TODO: Remove in the near future.
        public static bool doNotRunTraitsPatch;
    }

    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class VanillaExpandedFramework_TraitSet_GainTrait_Patch
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
                        var apparelExtension = apparel?.def.GetModExtension<ApparelExtension>();
                        if (apparelExtension?.traitsOnEquip != null && apparelExtension.traitsOnEquip.Any(t => t.def == trait.def && (t.degree == null || t.degree == trait.Degree)))
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

    [HarmonyPatch(typeof(DynamicPawnRenderNodeSetup_Apparel), "ProcessApparel")]
    public static class VanillaExpandedFramework_DynamicPawnRenderNodeSetup_Apparel_ProcessApparel_Patch
    {
        public delegate IEnumerable<ValueTuple<PawnRenderNode, PawnRenderNode>> ProcessApparel(Pawn pawn, PawnRenderTree tree, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode, Dictionary<PawnRenderNode, int> layerOffsets);
        public static readonly ProcessApparel processApparel = AccessTools.MethodDelegate<ProcessApparel>
            (AccessTools.Method(typeof(DynamicPawnRenderNodeSetup_Apparel), "ProcessApparel"));

        public static IEnumerable<ValueTuple<PawnRenderNode, PawnRenderNode>> Postfix(IEnumerable<ValueTuple<PawnRenderNode, PawnRenderNode>> result, Pawn pawn, PawnRenderTree tree, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode, Dictionary<PawnRenderNode, int> layerOffsets)
        {
            var extension = ap.def.GetModExtension<ApparelExtension>();
            if (extension?.secondaryApparelGraphics != null)
            {
                foreach (var thingDef in extension.secondaryApparelGraphics)
                {
                    var item = ThingMaker.MakeThing(thingDef) as Apparel;
                    if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, pawn.story.bodyType, false, out _))
                        result = result.Concat(processApparel(pawn, tree, item, headApparelNode, bodyApparelNode, layerOffsets));
                }
            }

            return result;
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class VanillaExpandedFramework_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var renderAsPackMethod = AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.RenderAsPack));
            var codes = codeInstructions.ToList();
            bool found = false;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(renderAsPackMethod))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VanillaExpandedFramework_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler), nameof(IsUnifiedApparel)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand);
                }
            }
            if (!found)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on ApparelGraphicRecordGetter:TryGetGraphicApparel failed.");
            }
        }

        public static bool IsUnifiedApparel(Apparel apparel)
        {
            var extension = apparel.def.GetModExtension<ApparelExtension>();
            return extension != null && extension.isUnifiedApparel;
        }
    }
    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    public static class VanillaExpandedFramework_PawnRenderer_GetBodyPos_Patch
    {
        public static void Postfix(Pawn ___pawn, Vector3 drawLoc, ref bool showBody)
        {
            if (!showBody)
            {
                var pawn = ___pawn;
                if (pawn.apparel != null && ___pawn.CurrentBed() != null)
                {
                    if (pawn.apparel.WornApparel.Any(x => x.def.GetModExtension<ApparelExtension>()?.showBodyInBedAlways ?? false))
                    {
                        showBody = true;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Body), "CanDrawNow")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_Body_CanDrawNow_Patch
    {
        public static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
        {
            if (__result is false && parms.bed != null && parms.pawn.apparel != null)
            {
                if (parms.pawn.apparel.WornApparel.Any(x => x.def.GetModExtension<ApparelExtension>()?.showBodyInBedAlways ?? false))
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker), "AppendDrawRequests")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_AppendDrawRequests_Patch
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            if ((node is PawnRenderNode_Head || node.parent is PawnRenderNode_Head) && parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    requests.Add(new PawnGraphicDrawRequest(node)); // adds an empty draw request to not draw head
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "CanDrawNow")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_Apparel_Head_CanDrawNow_Patch
    {
        public static void Prefix(PawnDrawParms parms, out bool __state)
        {
            __state = Prefs.HatsOnlyOnMap;
            if (parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    Prefs.HatsOnlyOnMap = false;
                }
            }
        }

        public static void Finalizer(bool __state)
        {
            Prefs.HatsOnlyOnMap = __state;
        }
    }
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "HeadgearVisible")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var get_HatsOnlyOnMap = AccessTools.PropertyGetter(typeof(Prefs), nameof(Prefs.HatsOnlyOnMap));
            foreach (var codeInstruction in codeInstructions)
            {
                yield return codeInstruction;
                if (codeInstruction.Calls(get_HatsOnlyOnMap))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch),
                        "TryOverrideHatsOnlyOnMap"));
                }
            }
        }

        public static bool TryOverrideHatsOnlyOnMap(bool result, PawnDrawParms parms)
        {
            if (result is true && parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    return false;
                }
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), nameof(HealthCardUtility.GetPawnCapacityTip))]
    public static class VanillaExpandedFramework_HealthCardUtility_GetPawnCapacityTip_Patch
    {
        private static readonly List<Thing> tmpGearImpactors = [];

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        {
            var listConstructor = typeof(List<CapacityImpactor>).DeclaredConstructor([]);
            var stringBuilderConstructor = typeof(StringBuilder).DeclaredConstructor([]);

            var matcher = new CodeMatcher(instr);

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Newobj, listConstructor),
                CodeMatch.IsStloc()
            );

            var listIndex = matcher.Instruction.LocalIndex();
            matcher.Reset();

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Newobj, stringBuilderConstructor),
                CodeMatch.IsStloc()
            );

            var stringBuilderIndex = matcher.Instruction.LocalIndex();

            matcher.End();
            matcher.Advance(-2);

            matcher.Insert(
                CodeInstruction.LoadArgument(baseMethod.GetParameters().FirstIndexOf(x => x.ParameterType == typeof(Pawn))),
                CodeInstruction.LoadLocal(listIndex),
                CodeInstruction.LoadLocal(stringBuilderIndex),
                CodeInstruction.Call(() =>  InsertCustomImpactors)
            );

            return matcher.Instructions();
        }

        private static void InsertCustomImpactors(Pawn pawn, List<CapacityImpactor> list, StringBuilder sb)
        {
            // Should be inserted in a place where all of those are false, but let's be safe anyway
            if (pawn == null || sb == null || list is not { Count: > 0 })
                return;

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] is CapacityImpactorGearMinLevel gearImpactor && !tmpGearImpactors.Contains(gearImpactor.gear))
                {
                    sb.AppendLine($"  {gearImpactor.Readable(pawn)}");
                    tmpGearImpactors.Add(gearImpactor.gear);
                }
            }

            tmpGearImpactors.Clear();
        }
    }
}
