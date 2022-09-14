using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_IncidentWorker_WandererJoin
    {

        [HarmonyPatch(typeof(IncidentWorker_WandererJoin), "GeneratePawn")]
        public static class TryExecuteWorker
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("IncidentWorker_WandererJoin.TryExecuteWorker transpiler start (1 match todo)");
                #endif


                var instructionList = instructions.ToList();

                var defInfo = AccessTools.Field(typeof(IncidentWorker), nameof(IncidentWorker.def));
                var pawnKindInfo = AccessTools.Field(typeof(IncidentDef), nameof(IncidentDef.pawnKind));

                var finalisedPawnKindDefInfo = AccessTools.Method(typeof(TryExecuteWorker), nameof(FinalisedPawnKindDef));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Finalise the pawnKindDef to use
                    if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(pawnKindInfo))
                    {
                        #if DEBUG
                            Log.Message("IncidentWorker_WandererJoin.TryExecuteWorker match 1 of 1");
                        #endif
                        yield return instruction; // this.def.pawnKind
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        yield return new CodeInstruction(OpCodes.Ldfld, defInfo); // this.def
                        instruction = new CodeInstruction(OpCodes.Call, finalisedPawnKindDefInfo); // FinalisedPawnKindDef(this.def.pawnKind, this.def)
                    }

                    yield return instruction;
                }
            }

            private static PawnKindDef FinalisedPawnKindDef(PawnKindDef original, IncidentDef def)
            {
                // Finalise stranger in black pawn kind
                if (def == IncidentDefOf.StrangerInBlackJoin)
                {
                    var factionDefExtension = FactionDefExtension.Get(Faction.OfPlayer.def);
                    if (factionDefExtension.strangerInBlackReplacement != null)
                        return factionDefExtension.strangerInBlackReplacement;
                }

                return original;
            }

        }

    }

}
