using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFECore
{
    public class ConstructionSkillRequirement
    {
        public WorkTypeDef workType;
        public SkillDef skill;
        public int level;
        public string reportStringOverride;
    }

    [HarmonyPatch]
    public static class Workgiver_Patches
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(WorkGiver_ConstructDeliverResourcesToBlueprints), "JobOnThing");
            yield return AccessTools.Method(typeof(WorkGiver_ConstructDeliverResourcesToFrames), "JobOnThing");
            yield return AccessTools.Method(typeof(WorkGiver_ConstructFinishFrames), "JobOnThing");
        }

        public static bool Prefix(WorkGiver __instance, Pawn pawn, Thing t)
        {
            var extension = t?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement != null) 
            {
                if (__instance.def?.workType != null && __instance.def.workType != extension.constructionSkillRequirement.workType)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class WorkGiver_ConstructionSkill_DeliverResourcesToBlueprints : WorkGiver_ConstructDeliverResourcesToBlueprints
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var extension = t.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement is null)
            {
                return null;
            }
            var job = base.JobOnThing(pawn, t, forced);
            if (job != null && job.def == JobDefOf.PlaceNoCostFrame)
            {
                if (extension.constructionSkillRequirement.reportStringOverride.NullOrEmpty() is false)
                {
                    job.reportStringOverride = extension.constructionSkillRequirement.reportStringOverride;
                }
            }
            return job;
        }
    }

    public class WorkGiver_ConstructionSkill_DeliverResourcesToFrames : WorkGiver_ConstructDeliverResourcesToFrames
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var extension = t.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement is null)
            {
                return null;
            }
            return base.JobOnThing(pawn, t, forced);
        }
    }

    public class WorkGiver_ConstructionSkill_FinishFrames : WorkGiver_ConstructFinishFrames
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var extension = t.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement is null)
            {
                return null;
            }
            var job = base.JobOnThing(pawn, t, forced);
            if (job != null)
            {
                if (extension.constructionSkillRequirement.reportStringOverride.NullOrEmpty() is false)
                {
                    job.reportStringOverride = extension.constructionSkillRequirement.reportStringOverride;
                }
            }
            return job;
        }
    }

    [HarmonyPatch(typeof(GenConstruct), "CanConstruct", new Type[] { typeof(Thing), typeof(Pawn), typeof(WorkTypeDef), 
        typeof(bool), typeof(JobDef) })]
    public static class GenConstruct_CanConstruct_Patch
    {
        public static void Prefix(Thing t, Pawn pawn, ref WorkTypeDef workType, bool forced = false)
        {
            var extenstion = t?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extenstion?.constructionSkillRequirement != null)
            {
                workType = extenstion.constructionSkillRequirement.workType;
            }
        }
    }
    [HarmonyPatch(typeof(GenConstruct), "CanConstruct", new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool), typeof(JobDef) })]
    public static class GenConstruct_CanConstruct_Patch2
    {
        public static bool Prefix(ref bool __result, Thing t, Pawn p, ref bool checkSkills, bool forced)
        {
            var extenstion = t?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extenstion?.constructionSkillRequirement != null)
            {
                if (p.skills != null)
                {
                    if (p.skills.GetSkill(extenstion.constructionSkillRequirement.skill).Level
                        < extenstion.constructionSkillRequirement.level)
                    {
                        JobFailReason.Is("SkillTooLowForConstruction".Translate()
                            .Formatted(extenstion.constructionSkillRequirement.skill.LabelCap));
                        __result = false;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Designator_Build), "DrawPanelReadout")]
    public static class Designator_Build_DrawPanelReadout_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool patched = false;
            foreach (CodeInstruction instruction in codeInstructions)
            {
                yield return instruction;
                if (!patched && instruction.opcode == OpCodes.Stloc_3)
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Designator_Build_DrawPanelReadout_Patch), "DrawSkillRequirementIfNeeded"));
                }
            }
        }

        public delegate void DrawSkillRequirement(Designator_Build __instance, SkillDef skillDef, int requirement, float width, ref float curY);
        public static readonly DrawSkillRequirement drawSkillRequirement = AccessTools.MethodDelegate<DrawSkillRequirement>
            (AccessTools.Method(typeof(Designator_Build), "DrawSkillRequirement"));

        public static void DrawSkillRequirementIfNeeded(Designator_Build instance, ref bool flag, float width, ref float curY)
        {
            var extension = instance.PlacingDef?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement != null)
            {
                drawSkillRequirement(instance, extension.constructionSkillRequirement.skill,
                    extension.constructionSkillRequirement.level, width, ref curY);
                foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
                {
                    if (freeColonist.skills.GetSkill(extension.constructionSkillRequirement.skill).Level
                        >= extension.constructionSkillRequirement.level)
                    {
                        flag = true;
                        break;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Frame), "CompleteConstruction")]
    public static class Frame_CompleteConstruction_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var constructionField = AccessTools.Field(typeof(SkillDefOf), nameof(SkillDefOf.Construction));
            var interceptSkillInfo = AccessTools.Method(typeof(Frame_CompleteConstruction_Patch), "InterceptSkill");
            foreach (CodeInstruction instruction in codeInstructions)
            {
                yield return instruction;
                if (instruction.LoadsField(constructionField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, interceptSkillInfo);
                }
            }
        }

        public static SkillDef InterceptSkill(SkillDef skillDef, Frame frame)
        {
            var extension = frame?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement != null)
            {
                return extension.constructionSkillRequirement.skill;
            }
            return skillDef;
        }
    }

    [HarmonyPatch(typeof(JobDriver_ConstructFinishFrame), "MakeNewToils")]
    public static class JobDriver_ConstructFinishFrame_MakeNewToils_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_ConstructFinishFrame __instance)
        {
            foreach (var toil in __result)
            {
                yield return toil;
                if (toil.debugName == "MakeNewToils" && toil.activeSkill != null)
                {
                    toil.activeSkill = delegate 
                    {
                        var frame = __instance.job.GetTarget(TargetIndex.A).Thing;
                        var extension = frame?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
                        if (extension?.constructionSkillRequirement != null)
                        {
                            return extension.constructionSkillRequirement.skill;
                        }
                        return SkillDefOf.Construction;
                    };
                }
            }
        }
    }

    [HarmonyPatch]
    public static class JobDriver_ConstructFinishFrame_MakeNewToils_TickAction_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(JobDriver_ConstructFinishFrame).GetNestedTypes(AccessTools.all).SelectMany(x => x.GetMethods(AccessTools.all)
                            .Where(x => x.Name.Contains("<MakeNewToils>") && x.ReturnType == typeof(void))).ToList()[1];
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, MethodBase method)
        {
            var constructionField = AccessTools.Field(typeof(SkillDefOf), nameof(SkillDefOf.Construction));
            var interceptSkillInfo = AccessTools.Method(typeof(JobDriver_ConstructFinishFrame_MakeNewToils_TickAction_Patch),
                "InterceptSkill");
            var shouldSkipCheckInfo = AccessTools.Method(typeof(JobDriver_ConstructFinishFrame_MakeNewToils_TickAction_Patch),
    "ShouldSkipCheck");
            var thisField = method.DeclaringType.GetField("<>4__this");
            var codes = codeInstructions.ToList();
            bool patched = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (code.LoadsField(constructionField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thisField);
                    yield return new CodeInstruction(OpCodes.Call, interceptSkillInfo);
                }
                if (patched is false && code.opcode == OpCodes.Bge_Un_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thisField);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkipCheckInfo);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, code.operand);
                    patched = true;
                }
            }
        }

        public static bool ShouldSkipCheck(JobDriver_ConstructFinishFrame jobDriver)
        {
            var extension = jobDriver.job.targetA.Thing?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement != null)
            {
                return true;
            }
            return false;
        }

        public static SkillDef InterceptSkill(SkillDef skillDef, JobDriver_ConstructFinishFrame jobDriver)
        {
            var extension = jobDriver.job.targetA.Thing?.def?.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement != null)
            {
                return extension.constructionSkillRequirement.skill;
            }
            return skillDef;
        }
    }

    [HarmonyPatch]
    public static class QualityBuilder_WorkGiver_ConstructFinishFrames_Patch
    {
        public static MethodBase targetMethod;

        public static bool Prepare()
        {
            var type = AccessTools.TypeByName("QualityBuilder._WorkGiver_ConstructFinishFrames");
            if (type != null)
            {
                targetMethod = AccessTools.Method(type, "Postfix");
                return targetMethod != null;
            }
            return false;
        }

        public static MethodBase TargetMethod() => targetMethod;

        public static bool Prefix(Job __0)
        {
            if (__0?.workGiverDef?.giverClass != null && typeof(WorkGiver_ConstructionSkill_FinishFrames)
                .IsAssignableFrom(__0.workGiverDef.giverClass))
            {
                return false;
            }
            return true;
        }
    }
}
