using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures


    public static class VanillaExpandedFramework_Verb_MeleeAttackDamage_DamageInfosToApply_Patch
    {

        public static IEnumerable<CodeInstruction> ModifyMeleeDamage(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var meleeDamageDef = AccessTools.Field(typeof(VerbProperties), "meleeDamageDef");      
            var changeMeleeDamage = AccessTools.Method(typeof(VanillaExpandedFramework_Verb_MeleeAttackDamage_DamageInfosToApply_Patch), "ChangeMeleeDamage");

          
            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(meleeDamageDef)
                    )
                {
                  
                   yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, changeMeleeDamage);

                }
               
                else yield return codes[i];
            }
        }


        public static DamageDef ChangeMeleeDamage(VerbProperties verbProps, Verb_MeleeAttackDamage verb)
        {
            if (verb.EquipmentSource != null && StaticCollectionsClass.uniqueWeaponsInGame.Contains(verb.EquipmentSource.def))
            {
                CompUniqueWeapon comp = verb.EquipmentSource?.GetComp<CompUniqueWeapon>();
                if (comp != null)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension?.meleeDamageOverride != null)
                        {
                            return extension.meleeDamageOverride;
                        }
                    }
                }
            }
            return verbProps.meleeDamageDef;
        }

    }
}