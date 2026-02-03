using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures


    public static class VanillaExpandedFramework_Verb_BurstShotCount_Patch
    {

        public static IEnumerable<CodeInstruction> RandomizeBurstCount(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();


            var changeBurst = AccessTools.Method(typeof(VanillaExpandedFramework_Verb_BurstShotCount_Patch), "ChangeBurstCount");


            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 0 && codes[i].opcode == OpCodes.Stloc_0 && codes[i - 1].opcode == OpCodes.Mul)
                {

                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, changeBurst);
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }

                else yield return codes[i];
            }
        }


        public static float ChangeBurstCount(WeaponTraitDef trait)
        {

            WeaponTraitDefExtension extension = trait.GetModExtension<WeaponTraitDefExtension>();
            if (extension != null && extension.burstShotCountRange!= null)
            {
                return extension.burstShotCountRange.RandomElement();
            }

            return 1;

        }

    }
}