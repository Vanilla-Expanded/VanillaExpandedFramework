using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VEF.Weapons
{
   
    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures


    public static class VanillaExpandedFramework_Verb_TryCastNextBurstShot_Patch
    {

        public static IEnumerable<CodeInstruction> ChangeSoundProduced(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var verbProps = AccessTools.Field(typeof(Verb), "verbProps");
            var soundCast = AccessTools.Field(typeof(VerbProperties), "soundCast");
            var changeSound = AccessTools.Method(typeof(VanillaExpandedFramework_Verb_TryCastNextBurstShot_Patch), "ChangeSound");

            int position = 0;
            bool found = false;
            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 1].OperandIs(verbProps)
                    && codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 2].OperandIs(soundCast) && codes[i + 3].opcode == OpCodes.Ldarg_0)
                {
                    position = i;
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, changeSound);

                }
                else if (found && i > position && i < position + 3)
                {
                    yield return new CodeInstruction(OpCodes.Nop);
                }
                else yield return codes[i];
            }
        }


        public static SoundDef ChangeSound(Verb verb)
        {
            CompUniqueWeapon comp = verb.EquipmentSource?.GetComp<CompUniqueWeapon>();
            if (comp != null)
            {
                foreach (WeaponTraitDef item in comp.TraitsListForReading)
                {
                    WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                    if (extension?.soundOverride != null)
                    {
                        return extension.soundOverride;
                    }
                }
            }

            return verb.verbProps.soundCast;

        }

    }
}