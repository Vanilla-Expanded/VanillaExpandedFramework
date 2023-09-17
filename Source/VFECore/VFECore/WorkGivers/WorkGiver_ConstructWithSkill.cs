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
            var extension = t?.def?.entityDefToBuild.GetModExtension<ThingDefExtension>();
            if (extension?.constructionSkillRequirement != null) 
            {
                if (__instance.def.workType != extension.constructionSkillRequirement.workType)
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
            var job = base.JobOnThing(pawn, t, forced);
            if (job != null && job.def == JobDefOf.PlaceNoCostFrame)
            {
                var extension = t.def.entityDefToBuild.GetModExtension<ThingDefExtension>();
                if (extension.constructionSkillRequirement.reportStringOverride.NullOrEmpty() is false)
                {
                    job.reportStringOverride = extension.constructionSkillRequirement.reportStringOverride;
                }
            }
            return job;
        }
    }

    public class WorkGiver_ConstructionSkill_FinishFrames : WorkGiver_ConstructFinishFrames
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var job = base.JobOnThing(pawn, t, forced);
            if (job != null)
            {
                var extension = t.def.entityDefToBuild.GetModExtension<ThingDefExtension>();
                if (extension.constructionSkillRequirement.reportStringOverride.NullOrEmpty() is false)
                {
                    job.reportStringOverride = extension.constructionSkillRequirement.reportStringOverride;
                }
            }
            return job;
        }
    }

    [HarmonyPatch(typeof(GenConstruct), "CanConstruct", new Type[] { typeof(Thing), typeof(Pawn), typeof(WorkTypeDef), typeof(bool) })]
    public static class GenConstruct_CanConstruct_Patch
    {
        public static void Prefix(Thing t, Pawn pawn, ref WorkTypeDef workType, bool forced = false)
        {
            var extenstion = t.def.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extenstion != null)
            {
                if (extenstion.constructionSkillRequirement != null)
                {
                    workType = extenstion.constructionSkillRequirement.workType;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(GenConstruct), "CanConstruct", new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })]
    public static class GenConstruct_CanConstruct_Patch2
    {
        public static bool Prefix(ref bool __result, Thing t, Pawn p, ref bool checkSkills, bool forced)
        {
            var extenstion = t.def.entityDefToBuild?.GetModExtension<ThingDefExtension>();
            if (extenstion != null)
            {
                if (extenstion.constructionSkillRequirement != null)
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
}
