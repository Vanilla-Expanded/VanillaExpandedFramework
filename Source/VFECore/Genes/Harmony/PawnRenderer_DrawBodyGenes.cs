using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyGenes")]
    public static class PawnRenderer_DrawBodyGenes_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo drawOffsetAtMI = typeof(GeneGraphicData).GetMethod(nameof(GeneGraphicData.DrawOffsetAt));
            MethodInfo postProcessMI =
                typeof(PawnRenderer_DrawBodyGenes_Patch).GetMethod(nameof(PawnRenderer_DrawBodyGenes_Patch.DisableScaling));
            List<CodeInstruction> codes = instructions.ToList();
            int index = codes.FindIndex((x) => x.Calls(drawOffsetAtMI));
            codes.InsertRange(index + 2, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                CodeInstruction.LoadField(typeof(GeneGraphicRecord),nameof(GeneGraphicRecord.sourceGene)),
                CodeInstruction.LoadField(typeof(Gene),nameof(Gene.def)),
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.LoadField(typeof(PawnRenderer),"pawn"),
                new CodeInstruction(OpCodes.Ldarg_S,4),
                new CodeInstruction(OpCodes.Ldloca,1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloca_S,5),
                new CodeInstruction(OpCodes.Call,postProcessMI)
            });
            return codes;
        }

        public static void DisableScaling(GeneDef geneDef, Pawn pawn, Rot4 bodyFacing, ref float num, Vector2 bodyGraphicScale, ref Vector3 v)
        {
            GeneExtension extension = geneDef.GetModExtension<GeneExtension>();
            if (extension != null)
            {
                if ((extension.disableAdultScaling && !PawnHasBabyOrChildBodyType())
                    || (extension.disableChildScaling && PawnHasBabyOrChildBodyType()))
                {
                    bodyGraphicScale = Vector2.one;
                    num = 1;
                }
                var bodyTypeOffset = AdjustOffsetsIfNeeded(pawn, bodyFacing, extension);
                v += bodyTypeOffset;
            }

            bool PawnHasBabyOrChildBodyType()
            {
                if (pawn.story?.bodyType != null)
                {
                    return pawn.story.bodyType == BodyTypeDefOf.Baby || pawn.story.bodyType == BodyTypeDefOf.Child;
                }
                return false;
            }
        }

        public static Vector3 AdjustOffsetsIfNeeded(Pawn pawn, Rot4 bodyFacing, GeneExtension extension)
        {
            switch (bodyFacing.AsInt)
            {
                case 0:
                    {
                        if (extension.bodyOffsetNorth != null)
                        {
                            return AdjustedOffset(pawn, extension.bodyOffsetNorth);
                        }

                        return Vector3.zero;
                    }
                case 1:
                    {
                        if (extension.bodyOffsetEast != null)
                        {
                            return AdjustedOffset(pawn, extension.bodyOffsetEast);
                        }

                        return Vector3.zero;
                    }
                case 2:
                    {
                        if (extension.bodyOffsetSouth != null)
                        {
                            return AdjustedOffset(pawn, extension.bodyOffsetSouth);
                        }
                        return Vector3.zero;
                    }

                case 3:
                    {
                        if (extension.bodyOffsetEast != null)
                        {
                            Vector3 result = AdjustedOffset(pawn, extension.bodyOffsetEast);
                            result.x *= -1f;
                            return result;
                        }
                        return Vector3.zero;
                    }
                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 AdjustedOffset(Pawn pawn, GeneBodyOffset bodyTypeOffset)
        {
            if (pawn.story != null)
            {
                if (pawn.story.bodyType == BodyTypeDefOf.Male)
                {
                    return bodyTypeOffset.Male ?? Vector3.zero;
                }
                if (pawn.story.bodyType == BodyTypeDefOf.Female)
                {
                    return bodyTypeOffset.Female ?? Vector3.zero;
                }
                if (pawn.story.bodyType == BodyTypeDefOf.Fat)
                {
                    return bodyTypeOffset.Fat ?? Vector3.zero;
                }
                if (pawn.story.bodyType == BodyTypeDefOf.Hulk)
                {
                    return bodyTypeOffset.Hulk ?? Vector3.zero;
                }
                if (pawn.story.bodyType == BodyTypeDefOf.Thin)
                {
                    return bodyTypeOffset.Thin ?? Vector3.zero;
                }
                if (pawn.story.bodyType == BodyTypeDefOf.Baby)
                {
                    return bodyTypeOffset.Baby ?? Vector3.zero;
                }
                if (pawn.story.bodyType == BodyTypeDefOf.Child)
                {
                    return bodyTypeOffset.Child ?? Vector3.zero;
                }
            }
            return Vector3.zero;
        }
    }
}
