using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace VEF.Genes
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]
    public static class VanillaExpandedFramework_Pawn_HealthTracker_DropBloodFilth_Patch
    {

        

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {

            var codes = codeInstructions.ToList();
            var TryChangeBloodFilthInfo = AccessTools.Method(typeof(VanillaExpandedFramework_Pawn_HealthTracker_DropBloodFilth_Patch), nameof(TryChangeBloodFilth));
          

            for (var i = 0; i < codes.Count; i++)
            {
                if (i == 0)
                {
                    yield return codes[i];
                }
                else
                if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i - 1].opcode== OpCodes.Callvirt)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, TryChangeBloodFilthInfo);
                   

                }
                else yield return codes[i];


            }


        }

        public static ThingDef TryChangeBloodFilth(ThingDef thingDef, Pawn pawn) 
        {
            if (StaticCollectionsClass.bloodtype_gene_pawns.TryGetValue(pawn, out var customBlood))
            {
                return customBlood;
            }
            return thingDef;
        }
    }
}
