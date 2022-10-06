using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace VFECore
{

    public static class Patch_PawnApparelGenerator
    {
        public static class PossibleApparelSet
        {

            public static class manual_CoatButNoShirt
            {

                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
#if DEBUG
                        Log.Message("PawnApparelGenerator.PossibleApparelSet.manual_CoatButNoShirt transpiler start (1 match todo)");
#endif

                    var instructionList = instructions.ToList();

                    var apparelLayerDefOfShellInfo = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                    var apparelLayerDefOfOuterShellInfo = AccessTools.Field(typeof(ApparelLayerDefOf), nameof(ApparelLayerDefOf.VFEC_OuterShell));

                    for (int i = 0; i < instructionList.Count; i++)
                    {
                        var instruction = instructionList[i];

                        // Also have the generator consider OuterShell as an appropriate clothing layer
                        if (instruction.opcode == OpCodes.Beq_S)
                        {
                            var prevInstruction = instructionList[i - 1];
                            if (prevInstruction.opcode == OpCodes.Ldsfld && prevInstruction.OperandIs(apparelLayerDefOfShellInfo))
                            {
#if DEBUG
                                    Log.Message("PawnApparelGenerator.PossibleApparelSet.manual_CoatButNoShirt match 1 of 1");
#endif
                                yield return instruction;
                                yield return instructionList[i - 2]; // apparelLayerDef
                                yield return new CodeInstruction(OpCodes.Ldsfld, apparelLayerDefOfOuterShellInfo); // ApparelLayerDefOf.OuterShell
                                instruction = instruction.Clone(); //  if (... || apparelLayerDef == ApparelLayerDefOf.OuterShell || ...)
                            }
                        }

                        yield return instruction;
                    }
                }
            }
        }

        public static void GenerateStartingApparelFor_Postfix(Pawn pawn)
        {
            // Change the colour of appropriate apparel items to match the pawn's faction's colour
            if (pawn.apparel != null && pawn.Faction != null && pawn.kindDef.apparelColor == Color.white)
            {
                var pawnKindDefExtension = PawnKindDefExtension.Get(pawn.kindDef);
                var wornApparel = pawn.apparel.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    var apparel = wornApparel[i];

                    // Check from ThingDefExtension
                    var thingDefExtension = apparel.def.GetModExtension<ThingDefExtension>();
                    if (thingDefExtension != null && !thingDefExtension.useFactionColourForPawnKinds.NullOrEmpty() && thingDefExtension.useFactionColourForPawnKinds.Contains(pawn.kindDef))
                    {
                        apparel.SetColor(pawn.Faction.Color);
                        continue;
                    }

                    // Check from PawnKindDefExtension
                    var apparelProps = apparel.def.apparel;
                    var partGroupAndLayerPairs = pawnKindDefExtension.FactionColourApparelWithPartAndLayersList;
                    for (int j = 0; j < partGroupAndLayerPairs.Count; j++)
                    {
                        var partGroupAndLayerPair = partGroupAndLayerPairs[j];
                        if (apparelProps.bodyPartGroups.Contains(partGroupAndLayerPair.First) && apparelProps.layers.Contains(partGroupAndLayerPair.Second))
                        {
                            apparel.SetColor(pawn.Faction.Color);
                            break;
                        }
                    }

                }
            }
        }
    }
}
