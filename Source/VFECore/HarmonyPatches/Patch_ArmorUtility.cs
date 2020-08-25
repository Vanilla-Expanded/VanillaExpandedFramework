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

    public static class Patch_ArmorUtility
    {

        [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
        public static class GetPostArmorDamage
        {

            public static bool Prefix(Pawn pawn, ref float amount, ref float armorPenetration, BodyPartRecord part, ref DamageDef damageDef, out bool deflectedByMetalArmor, out bool diminishedByMetalArmor, ref float __result)
            {
                deflectedByMetalArmor = false;
                diminishedByMetalArmor = false;

                // Apply shield damage reduction before apparel damage reduction
                if (damageDef.armorCategory != null)
                {
                    var armourRating = damageDef.armorCategory.armorRatingStat;
                    if (pawn.apparel != null)
                    {
                        // Multiple shields? Why not I guess
                        var equipmentList = pawn.apparel.WornApparel;
                        for (int i = 0; i < equipmentList.Count; i++)
                        {
                            var curEq = equipmentList[i];
                            if (curEq.IsShield(out CompShield sC) && sC.UsableNow && sC.CoversBodyPart(part))
                            {
                                float prevAmount = amount;
                                NonPublicMethods.ArmorUtility_ApplyArmor(ref amount, armorPenetration, curEq.GetStatValue(armourRating), curEq, ref damageDef, pawn, out bool metalArmour);

                                // Deflected
                                if (amount < 0.001f)
                                {
                                    deflectedByMetalArmor = metalArmour;
                                    __result = 0;
                                    return false;
                                }

                                // Diminished
                                if (amount < prevAmount)
                                    diminishedByMetalArmor = metalArmour;
                            }
                        }
                    }
                }

                return true;
            }

        }

        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
        public static class ApplyArmor
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
            {
                #if DEBUG
                    Log.Message("ArmorUtility.ApplyArmor transpiler start (no matches todo)");
                #endif

                var instructionList = instructions.ToList();

                // Create a label and add to the first instruction in the original method
                var firstLabel = ilGen.DefineLabel();
                instructionList[0].labels.Add(firstLabel);

                yield return new CodeInstruction(OpCodes.Ldarg_S, 3); // armorThing
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ApplyArmor), nameof(IsShield))); // IsShield(armorThing)
                yield return new CodeInstruction(OpCodes.Brfalse, firstLabel); // if (IsShield(armorThing))
                yield return new CodeInstruction(OpCodes.Ldarg_S, 6); // metalArmor
                yield return new CodeInstruction(OpCodes.Ldarg_S, 3); // armourThing
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ApplyArmor), nameof(ShieldUseDeflectMetalEffect))); // ShieldUseDeflectMetalEffect(armorThing)
                yield return instructionList.First(i => i.opcode == OpCodes.Br_S).Clone();

                /* 

                Code block:

                if (IsShield(armorThing))
                    metalArmor = ShieldUseDeflectMetalEffect(armorThing)
                else if (armorThing != null)
                    metalArmor = (armorThing.def.apparel.useDeflectMetalEffect || (armorThing.Stuff != null && armorThing.Stuff.IsMetal));
                else
                    metalArmor = pawn.RaceProps.IsMechanoid;

                etc...

                 */

                // Original method - leaving like this in case future changes need to be made
                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];
                    yield return instruction;
                }
            }

            private static bool IsShield(Thing armourThing)
            {
                return armourThing != null && armourThing.def.IsShield();
            }

            private static bool ShieldUseDeflectMetalEffect(Thing armourThing)
            {
                return armourThing.TryGetComp<CompShield>().Props.useDeflectMetalEffect;
            }

        }

    }

}
