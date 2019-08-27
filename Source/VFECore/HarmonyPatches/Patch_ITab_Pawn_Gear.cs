using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class Patch_ITab_Pawn_Gear
    {

        [HarmonyPatch(typeof(ITab_Pawn_Gear), "TryDrawOverallArmor")]
        public static class TryDrawOverallArmor
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                bool foundCoverageAbs = false;
                bool done = false;

                var coverageAbsInfo = AccessTools.Field(typeof(BodyPartRecord), nameof(BodyPartRecord.coverageAbs));
                var getSelPawnForGearInfo = AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear").GetGetMethod(true);
                var overallArmourFromShieldInfo = AccessTools.Method(typeof(TryDrawOverallArmor), nameof(OverallArmourFromShield));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (!done)
                    {
                        // Look for the first instruction in the method that references BodyPartRecord.coverageAbs
                        if (instruction.opcode == OpCodes.Ldfld && instruction.operand == coverageAbsInfo)
                            foundCoverageAbs = true;

                        // Look for the next reference to 'num' when coverageAbs is found; this is where we patch
                        if (foundCoverageAbs && instruction.opcode == OpCodes.Ldloc_0)
                        {
                            yield return instruction; // num
                            yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                            yield return new CodeInstruction(OpCodes.Call, getSelPawnForGearInfo); // this.SelPawnForGear
                            yield return new CodeInstruction(OpCodes.Ldarg_3); // stat
                            yield return new CodeInstruction(OpCodes.Call, overallArmourFromShieldInfo); // OverallArmourFromShield(num, this.pawn, stat)
                            yield return new CodeInstruction(OpCodes.Stloc_0); // num = OverallArmourFromShield(num, this.pawn, stat)
                            instruction = instruction.Clone(); // num
                            done = true;
                        }
                    }

                    yield return instruction;
                }
            }

            public static float OverallArmourFromShield(float overallArmour, Pawn pawn, StatDef stat)
            {
                var equipment = pawn.equipment;

                // Go through each body part and each piece of equipment to get overall defence bonuses of usable shields
                if (equipment != null)
                {
                    float naturalArmour = Mathf.Clamp01(pawn.GetStatValue(stat) / 2);
                    var bodyParts = pawn.RaceProps.body.AllParts;
                    var equipmentList = equipment.AllEquipmentListForReading;
                    foreach (var part in bodyParts)
                    {
                        float armourImportance = 1 - naturalArmour;
                        foreach (var eq in equipmentList)
                        {
                            if (eq.IsShield(out CompShield shieldComp) && shieldComp.UsableNow && shieldComp.CoversBodyPart(part))
                            {
                                float shieldRating = Mathf.Clamp01(eq.GetStatValue(stat) / 2);
                                armourImportance *= 1 - shieldRating;
                            }
                        }
                        overallArmour += part.coverageAbs * (1 - armourImportance);
                    }
                }

                return overallArmour;
            }

        }

    }

}
