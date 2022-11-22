using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.QuestGen;

namespace VanillaGenesExpanded
{


    [HarmonyPatch(typeof(JobDriver_Vomit))]
    [HarmonyPatch("MakeNewToils")]
    public static class VanillaGenesExpanded_JobDriver_Vomit_MakeNewToils_Patch
    {
        public static Pawn curPawn;

        [HarmonyPrefix]
        public static void StorePawn(JobDriver_Vomit __instance)
        {
            curPawn = __instance.pawn;
           
        }
    }


    [HarmonyPatch]
    public static class VanillaGenesExpanded_JobDriver_Vomit_MakeNewToils_Transpiler_Patch
    {

        static MethodBase TargetMethod()
        {
            MethodBase method = typeof(JobDriver_Vomit).GetMethod("<MakeNewToils>b__4_1", BindingFlags.Instance | BindingFlags.NonPublic);
            return method;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var field = AccessTools.Field(typeof(ThingDefOf), "Filth_Vomit");
            foreach (var code in codes)
            {
                
                if (code.opcode == OpCodes.Ldsfld && code.LoadsField(field))
                {
                   
                   
                    yield return new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(VanillaGenesExpanded_JobDriver_Vomit_MakeNewToils_Transpiler_Patch), nameof(GetVomitFilth)));
                }else yield return code;
            }
        }

        public static ThingDef GetVomitFilth() {

            
            if (VanillaGenesExpanded_JobDriver_Vomit_MakeNewToils_Patch.curPawn!=null&&
                StaticCollectionsClass.vomitType_gene_pawns.ContainsKey(VanillaGenesExpanded_JobDriver_Vomit_MakeNewToils_Patch.curPawn))
            {
                return StaticCollectionsClass.vomitType_gene_pawns[VanillaGenesExpanded_JobDriver_Vomit_MakeNewToils_Patch.curPawn];
            }
            return ThingDefOf.Filth_Vomit;

        }
        

    }





}
