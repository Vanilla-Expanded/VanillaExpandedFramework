using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class Patch_PawnRenderer
    {

        [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
        public static class DrawEquipment
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var drawEquipmentAimingInfo = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming));
                var tryDrawShieldAimingInfo = AccessTools.Method(typeof(DrawEquipment), nameof(TryDrawShieldAiming));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // For every call that draws equipment, also add a call to draw shield
                    if (instruction.opcode == OpCodes.Call && instruction.operand == drawEquipmentAimingInfo)
                    {
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        yield return instructionList[i - 2]; // drawLoc
                        instruction = new CodeInstruction(OpCodes.Call, tryDrawShieldAimingInfo); // TryDrawShieldAiming(this, drawLoc)
                    }

                    yield return instruction;
                }
            }

            private static void TryDrawShieldAiming(PawnRenderer instance, Vector3 drawLoc)
            {
                var pawn = (Pawn)NonPublicFields.PawnRenderer_pawn.GetValue(instance);
                if (pawn.equipment.OffHandShield() is ThingWithComps shield && pawn.equipment.Primary != shield)
                {
                    var shieldComp = shield.GetComp<CompShield>();
                    if (shieldComp.UsableNow)
                    {
                        var curHoldOffset = shieldComp.Props.offHandHoldOffset.Pick(pawn.Rotation);
                        var finalDrawLoc = drawLoc + curHoldOffset.offset + new Vector3(0, (curHoldOffset.behind ? -0.0390625f : 0.0390625f), 0);
                        shieldComp.Props.offHandGraphicData.GraphicColoredFor(shield).Draw(finalDrawLoc, (curHoldOffset.flip ? pawn.Rotation.Opposite : pawn.Rotation), pawn);
                    }
                        
                }
            }

        }

        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
        public static class RenderPawnInternal
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var apparelLayerDefOfShellInfo = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                var topApparelLayerDefInfo = AccessTools.Method(typeof(RenderPawnInternal), nameof(TopApparelLayerDef));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Replace references to Shell layer with references to our OuterShell layer
                    if (instruction.opcode == OpCodes.Ldsfld && instruction.operand == apparelLayerDefOfShellInfo)
                    {
                        yield return instruction; // ApparelLayerDefOf.Shell
                        instruction = new CodeInstruction(OpCodes.Call, topApparelLayerDefInfo); // TopApparelLayerDef(ApparelLayerDefOf.Shell)
                    }

                    yield return instruction;
                }
            }

            private static ApparelLayerDef TopApparelLayerDef(ApparelLayerDef original)
            {
                return ApparelLayerDefOf.VFEC_OuterShell;
            }

        }

    }

}
