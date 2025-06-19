using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Equip), "GetSingleOptionFor")]
    public static class VanillaExpandedFramework_FloatMenuOptionProvider_Equip_GetSingleOptionFor_Patch
    {
        public static void Postfix(FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
        {
            if (clickedThing is Apparel equipment && equipment.TryGetComp<CompEquippable>() != null)
            {
                var selPawn = context.FirstSelectedPawn;
                TaggedString toCheck = "Equip".Translate(equipment.LabelShort);
                if (__result != null && toCheck == __result.Label && selPawn.equipment != null
                    && !equipment.def.UsableWithShields() && selPawn.OffHandShield() is Apparel_Shield oldShield)
                {
                    __result.Label += $" {"VanillaFactionsExpanded.EquipWarningShieldUnusableWithWeapon".Translate(oldShield.def.label)}";
                }
            } 
        }
    }
}
