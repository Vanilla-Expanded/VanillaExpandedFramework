using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
    public static class VanillaGenesExpanded_HealthCardUtility_DrawHediffRow_Patch
    {
        public static Pawn curPawn;
        public static string bloodIcon;
        public static void Prefix(Pawn pawn)
        {
            curPawn = pawn;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            var codes =  codeInstructions.ToList();
            FieldInfo bleedingIconStaticField = AccessTools.Field(typeof(HealthCardUtility), "BleedingIcon");
            var label = ilg.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Stfld && codes[i - 1].LoadsField(bleedingIconStaticField))
                {
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VanillaGenesExpanded_HealthCardUtility_DrawHediffRow_Patch), 
                        nameof(VanillaGenesExpanded_HealthCardUtility_DrawHediffRow_Patch.HasBloodIconChangingGene), new Type[] { typeof(Pawn) }));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VanillaGenesExpanded_HealthCardUtility_DrawHediffRow_Patch), "ChangeIconForThisPawn", null, null));
                    yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(HealthCardUtility).GetNestedTypes(AccessTools.all).First(x => x.Name.Contains("c__DisplayClass32_1")), "bleedingIcon"));
                }
            }
        }

        public static Texture2D ChangeIconForThisPawn()
        {
           
            return ContentFinder<Texture2D>.Get(bloodIcon, true);
        }
        public static bool HasBloodIconChangingGene(Pawn pawn)
        {
            if (pawn!=null&&StaticCollectionsClass.bloodIcon_gene_pawns.ContainsKey(pawn))
            {
               
                bloodIcon = StaticCollectionsClass.bloodIcon_gene_pawns[pawn];
                return true;
            }
            return false;
        }

        public static void Postfix()
        {

            curPawn = null;
        }
    }
}