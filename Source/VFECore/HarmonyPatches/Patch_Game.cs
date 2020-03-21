using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_Game
    {

        [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
        public static class InitNewGame
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("Game.InitNewGame transpiler start (1 match todo)");
                #endif

                var instructionList = instructions.ToList();

                var hasTagInfo = AccessTools.Method(typeof(ResearchProjectDef), nameof(ResearchProjectDef.HasTag));

                var autoCompleteResearchInfo = AccessTools.Method(typeof(InitNewGame), nameof(AutoCompleteResearch));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Callvirt && instruction.OperandIs(hasTagInfo))
                    {
                        #if DEBUG
                            Log.Message("Game.InitNewGame match 1 of 1");
                        #endif
                        yield return instruction; // researchProjectDef.HasTag(tag)
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 9); // researchProjectDef
                        instruction = new CodeInstruction(OpCodes.Call, autoCompleteResearchInfo); // AutoCompleteResearch(researchProjectDef.HasTag(tag), researchProjectDef)
                    }

                    yield return instruction;
                }
            }

            private static bool AutoCompleteResearch(bool original, ResearchProjectDef research)
            {
                if (original && research != null)
                {
                    // Check the faction def starting tags and research DefModExtension greylist tags; return false if there are any matches
                    var researchProjectDefExtension = ResearchProjectDefExtension.Get(research);
                    if (!researchProjectDefExtension.greylistedTags.NullOrEmpty())
                    {
                        var startingTags = Faction.OfPlayer.def.startingResearchTags;
                        return !startingTags.Any(t => researchProjectDefExtension.greylistedTags.Contains(t));
                    }
                }
                return original;
            }

        }

    }

}
