using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GraphicCustomization
{
    [HarmonyPatch(typeof(PawnAttackGizmoUtility), "AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons")]
    public static class PawnAttackGizmoUtility_AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons_Patch
    {
        public static void Postfix(ref bool __result)
        {
            if (__result is false && AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons())
            {
                __result = true;
            }
        }

        private static bool AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons()
        {
            if (Find.Selector.NumSelected <= 1)
            {
                return false;
            }
            ThingDef thingDef = null;
            bool flag = false;
            List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
            for (int i = 0; i < selectedObjectsListForReading.Count; i++)
            {
                if (selectedObjectsListForReading[i] is Pawn pawn && PawnAttackGizmoUtility.CanOrderPlayerPawn(pawn))
                {
                    ThingDef thingDef2 = ((pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.def : null);
                    if (!flag)
                    {
                        thingDef = thingDef2;
                        flag = true;
                    }
                    else if (thingDef.HasComp<CompGraphicCustomization>())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }


}
