using HarmonyLib;
using Mono.Cecil.Cil;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld.Planet;
using Verse;
using UnityEngine.UIElements;
using Verse.Noise;
using UnityEngine.Tilemaps;

namespace VEF.AnimalBehaviours
{

    [HarmonyPatch(typeof(Hediff_Pregnant))]
    [HarmonyPatch("DoBirthSpawn")]
    public static class VanillaExpandedFramework_Hediff_Pregnant_DoBirthSpawn_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ModifyCrossbreedKindDef(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var field = AccessTools.Field(typeof(Pawn), "kindDef");
            var method = AccessTools.Method(typeof(VanillaExpandedFramework_Hediff_Pregnant_DoBirthSpawn_Patch), "ModifyCrossbreed");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 0 && codes[i - 1].opcode == OpCodes.Ldarg_0 && codes[i].LoadsField(field))
                {

                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, method);

                }

                else yield return codes[i];
            }

        }

        public static PawnKindDef ModifyCrossbreed(Pawn mother, Pawn father)
        {
            
            AnimalCrossbreedExtension extension = mother.def.GetModExtension<AnimalCrossbreedExtension>();
            if (extension is null)
            {
                extension = father?.def.GetModExtension<AnimalCrossbreedExtension>();
            }
            if (extension is null)
            {
                return mother.kindDef;
            }
            switch (extension.crossBreedKindDef)
            {
                case (FatherOrMother.AlwaysFather):
                    return father.kindDef ?? mother.kindDef;
                case (FatherOrMother.Random):
                    return Rand.Chance(0.5f) ? mother.kindDef : (father.kindDef ?? mother.kindDef);
                case (FatherOrMother.OtherPawnKind):
                    if (extension.otherPawnKindsByWeight != null && extension.otherPawnKindsByWeight.TryRandomElementByWeight(x => x.Value, out var value))
                        return value.Key;
                    return extension.otherPawnKind ?? mother.kindDef;
                default:
                    return mother.kindDef;

            }

        }

    }
}