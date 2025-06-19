using HarmonyLib;
using RimWorld;

namespace VEF.Weapons
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public static class VanillaExpandedFramework_Pawn_ApparelTracker_Notify_ApparelRemoved_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance)
        {
            var equipment = __instance.pawn.equipment?.Primary;
            if (equipment != null)
            {
                var options = equipment.def.GetModExtension<HeavyWeapon>();
                if (options != null && options.isHeavy)
                {
                    if (!VanillaExpandedFramework_EquipmentUtility_CanEquip_Patch.CanEquip(__instance.pawn, options))
                    {
                        __instance.pawn.equipment.TryDropEquipment(equipment, out var pos, __instance.pawn.Position);
                    }
                }
            }
        }
    }
}
