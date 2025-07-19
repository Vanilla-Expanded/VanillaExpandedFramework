using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours;

[HarmonyPatch(typeof(CompEggLayer), nameof(CompEggLayer.ProduceEgg))]
public class VanillaExpandedFramework_CompEggLayer_ProduceEgg
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ModifyCrossbreedEggThingDef(IEnumerable<CodeInstruction> codeInstructions)
    {
        var codes = codeInstructions.ToList();
        var field = AccessTools.DeclaredField(typeof(CompProperties_EggLayer), nameof(CompProperties_EggLayer.eggFertilizedDef));
        var method = AccessTools.DeclaredPropertyGetter(typeof(CompEggLayer), nameof(CompEggLayer.Props));

        var extraField = AccessTools.DeclaredField(typeof(CompEggLayer), "fertilizedBy");
        var extraMethod = AccessTools.DeclaredMethod(typeof(VanillaExpandedFramework_CompEggLayer_ProduceEgg), nameof(ModifyCrossbreedEgg));

        for (var i = 0; i < codes.Count; i++)
        {
            var instr = codes[i];

            if (i + 1 < codes.Count && instr.Calls(method) && codes[i + 1].LoadsField(field))
            {
                // Load this (CompEggLayer) onto the stack
                yield return CodeInstruction.LoadArgument(0);
                // Load the fertilizedBy field (Pawn)
                yield return new CodeInstruction(OpCodes.Ldfld, extraField);
                // Call our method
                yield return new CodeInstruction(OpCodes.Call, extraMethod);

                // Skip this and next instructions
                i++;
            }
            else yield return instr;
        }
    }

    public static ThingDef ModifyCrossbreedEgg(CompEggLayer comp, Pawn father)
    {
        var extension = comp.parent.def.GetModExtension<AnimalCrossbreedExtension>();
        extension ??= father?.def.GetModExtension<AnimalCrossbreedExtension>();
        if (extension is null)
        {
            return comp.Props.eggFertilizedDef;
        }

        switch (extension.crossBreedKindDef)
        {
            case FatherOrMother.AlwaysFather:
                return father.GetComp<CompEggLayer>()?.Props.eggFertilizedDef ?? comp.Props.eggFertilizedDef;
            case FatherOrMother.Random:
                return Rand.Bool ? comp.Props.eggFertilizedDef : father.GetComp<CompEggLayer>()?.Props.eggFertilizedDef ?? comp.Props.eggFertilizedDef;
            case FatherOrMother.OtherPawnKind:
            {
                PawnKindDef randomPawn = null;
                if (extension.otherPawnKindsByWeight != null && extension.otherPawnKindsByWeight.TryRandomElementByWeight(x => x.weight, out var value))
                    randomPawn = value.kindDef;
                return GetEggForPawnKind(randomPawn) ?? GetEggForPawnKind(extension.otherPawnKind) ?? comp.Props.eggFertilizedDef;
            }
            default:
                return comp.Props.eggFertilizedDef;
        }

        ThingDef GetEggForPawnKind(PawnKindDef kindDef) => kindDef.race?.GetCompProperties<CompProperties_EggLayer>()?.eggFertilizedDef;
    }
}