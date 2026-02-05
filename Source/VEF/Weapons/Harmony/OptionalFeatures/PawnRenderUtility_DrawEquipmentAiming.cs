using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_PawnRenderUtility_DrawEquipmentAiming_Patch
    {
        private static bool recursionCheck = false;
        private static Pawn storedPawn = null;
        private static PawnRenderFlags storedFlags;
        private static Rot4 storedFacing;


        public static void GrabPawn(Pawn pawn, PawnRenderFlags flags, Rot4 facing)
        {

            storedPawn = pawn;
            storedFlags = flags;
            storedFacing = facing;

        }

        public static void DrawDuplicate(Thing eq, ref Vector3 drawLoc, float aimAngle)
        {
            if (recursionCheck) return;
            if (!StaticCollectionsClass.uniqueWeaponsInGame.Contains(eq.def)) return;

            recursionCheck = true;

            CompUniqueWeapon comp = eq.TryGetComp<CompUniqueWeapon>();
            if (comp != null)
            {
                foreach (WeaponTraitDef item in comp.TraitsListForReading)
                {
                    WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                    if (extension?.drawDuplicate == true)
                    {
                        PawnRenderUtility.DrawEquipmentAndApparelExtras(storedPawn, drawLoc + new Vector3(0, 0, 0.2f), storedFacing, storedFlags);
                        drawLoc -= new Vector3(0, 0, -0.2f);
                    }
                }
            }

            
            return;



        }
        public static void DrawDuplicateCleanup()
        {
            recursionCheck = false;
            storedPawn = null;
        }
    }


}
