using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace VEF.Apparels
{
    [HotSwappable]
    [HarmonyPatch(typeof(Building_OutfitStand), "GetFloatMenuOptions")]
    public static class VanillaExpandedFramework_Building_OutfitStand_GetFloatMenuOptionToWear_Patch
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result,
            Building_OutfitStand __instance, Pawn selPawn)
        {
            var list = __result.ToList();
            foreach (Thing item in __instance.HeldItems)
            {
                if (item is Apparel_Shield shield)
                {
                    TaggedString toCheck = "ForceWear".Translate(shield.LabelCap, shield);
                    FloatMenuOption floatMenuOption = list.FirstOrDefault((FloatMenuOption x) =>
                    x.Label.Contains(toCheck));
                    if (floatMenuOption != null)
                    {
                        list.Remove(floatMenuOption);
                    }
                    var toCheck2 = "ForceTargetToWear".Translate(shield.LabelShort, shield);
                    var floatMenuOption2 = list.FirstOrDefault((FloatMenuOption x) =>
                        x.Label.Contains(toCheck2));
                    if (floatMenuOption2 != null)
                    {
                        list.Remove(floatMenuOption2);
                    }
                }
                if (item is Apparel equipment && equipment.TryGetComp<CompEquippable>() != null)
                {
                    TaggedString toCheck = "Equip".Translate(equipment.LabelShort);
                    FloatMenuOption floatMenuOption = __result.FirstOrDefault((FloatMenuOption x) => x.Label.Contains
                    (toCheck));
                    if (floatMenuOption != null && selPawn.equipment != null
                        && !equipment.def.UsableWithShields() && selPawn.OffHandShield() is Apparel_Shield oldShield)
                    {
                        floatMenuOption.Label += $" {"VanillaFactionsExpanded.EquipWarningShieldUnusableWithWeapon".Translate(oldShield.def.label)}";
                    }
                    var option = FloatMenuOptionProvider_EquipShield.AddShieldFloatMenuOption(selPawn,
                        equipment, __instance);
                    if (option != null)
                    {
                        list.Add(option);
                    }
                }
            }
            return list;
        }
    }
}
