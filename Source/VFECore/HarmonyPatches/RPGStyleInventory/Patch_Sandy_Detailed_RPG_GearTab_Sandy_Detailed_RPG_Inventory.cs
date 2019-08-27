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

    public static class Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory
    {

        public static class manual_TryDrawOverallArmor
        {

            public static Type DetailedRPGGearTab;

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // This is pretty much identical to the ITab_Pawn_Gear patch
                var instructionList = instructions.ToList();

                bool foundCoverageAbs = false;
                bool done = false;

                var coverageAbsInfo = AccessTools.Field(typeof(BodyPartRecord), nameof(BodyPartRecord.coverageAbs));
                var getSelPawnForGearInfo = AccessTools.Property(DetailedRPGGearTab, "SelPawnForGear").GetGetMethod(true);
                var overallArmourFromShieldInfo = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "OverallArmourFromShield");

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

        }

        public static class manual_TryDrawOverallArmor1
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // This is pretty much identical to the other TryDrawOverallArmor patch. Only difference is that stat is the 2nd parameter instead of the 3rd
                var instructionList = instructions.ToList();

                bool foundCoverageAbs = false;
                bool done = false;

                var coverageAbsInfo = AccessTools.Field(typeof(BodyPartRecord), nameof(BodyPartRecord.coverageAbs));
                var getSelPawnForGearInfo = AccessTools.Property(manual_TryDrawOverallArmor.DetailedRPGGearTab, "SelPawnForGear").GetGetMethod(true);
                var overallArmourFromShieldInfo = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "OverallArmourFromShield");

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
                            yield return new CodeInstruction(OpCodes.Ldarg_2); // stat
                            yield return new CodeInstruction(OpCodes.Call, overallArmourFromShieldInfo); // OverallArmourFromShield(num, this.pawn, stat)
                            yield return new CodeInstruction(OpCodes.Stloc_0); // num = OverallArmourFromShield(num, this.pawn, stat)
                            instruction = instruction.Clone(); // num
                            done = true;
                        }
                    }

                    yield return instruction;
                }
            }
        }

    }

}
