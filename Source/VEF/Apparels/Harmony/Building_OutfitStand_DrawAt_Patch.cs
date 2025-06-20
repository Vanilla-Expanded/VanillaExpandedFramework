using UnityEngine;
using RimWorld;
using HarmonyLib;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(Building_OutfitStand), "DrawAt")]
    public static class VanillaExpandedFramework_Building_OutfitStand_DrawAt_Patch
    {
        public static void Postfix(Building_OutfitStand __instance, Vector3 drawLoc)
        {
            foreach (var item in __instance.HeldItems)
            {
                if (item is Apparel_Shield shield)
                {
                    shield.DrawShield(shield.CompShield, drawLoc, __instance.Rotation);
                }
            }
        }
    }
}
