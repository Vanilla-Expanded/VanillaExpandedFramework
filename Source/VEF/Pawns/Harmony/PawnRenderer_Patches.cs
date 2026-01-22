using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using VEF.Factions;
using VEF.Weapons;
using VEF.Things;
using VEF.Graphics;
using VEF.Apparels;

namespace VEF.Pawns
{
  

    [HarmonyPatch]
    public static class VanillaExpandedFramework_FemaleBB_BodyType_Support_Patch
    {
        private static MethodBase target;
        private static bool Prepare()
        {
            if (ModsConfig.IsActive("ssulunge.BBBodySupport"))
            {
                var type = AccessTools.TypeByName("BBBodySupport.BBBodyTypeSupportHarmony+BBBodyGraphicApparelPatch");
                if (type == null)
                {
                    Log.Error("[VEF] Failed to find BBBodySupport.BBBodyTypeSupportHarmony+BBBodyGraphicApparelPatch type.");
                    return false;
                }
                target = AccessTools.Method(type, "BBBody_GraphicApparelPatch");
                if (target == null)
                {
                    Log.Error("[VEF] Failed to find BBBody_GraphicApparelPatch method in BBBodySupport.BBBodyTypeSupportHarmony+BBBodyGraphicApparelPatch type.");
                    return false;
                }
                return true;
            }
            return false;
        }

        [HarmonyTargetMethod]
        public static MethodBase GetMethod()
        {
            return target;
        }
        public static bool Prefix(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec, ref bool __3, ref bool __result)
        {
            if (VanillaExpandedFramework_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler.IsUnifiedApparel(apparel))
            {
                __3 = true;
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
    public static class VanillaExpandedFramework_PawnRenderer_DrawEquipmentAiming_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            Pawn pawn = eq.GetPawnAsHolder();
            if (pawn == null) return;

            var thingDefExtension = eq.def.GetModExtension<ThingDefExtension>();

            if (thingDefExtension != null && PawnRenderUtility.CarryWeaponOpenly(pawn))
            {
                var pawnRot = pawn.Rotation;
                var pawnIsMoving = pawn.pather?.Moving ?? false;

                // Weapon draw offsets that apply at all times (i.e. carrying weapons while working, drafted, attacking)
                // Replaces the now-defunct CompOversizedWeapon
                ApplyWeaponDrawOffset(thingDefExtension.weaponCarryDrawOffsets, pawnRot, pawnIsMoving, ref drawLoc, ref aimAngle);

                // Weapon draw offsets that only apply when the pawn is drafted but *not* actively attacking
                // Useful for things like holding a pike/halberd while standing at attention
                //
                // Note: These offsets add on to anything in weaponCarryDrawOffsets
                if (thingDefExtension.weaponDraftedDrawOffsets != null && !pawn.stances.curStance.StanceBusy)
                {
                    ApplyWeaponDrawOffset(thingDefExtension.weaponDraftedDrawOffsets, pawnRot, pawnIsMoving, ref drawLoc, ref aimAngle);
                }
            }
        }

        private static void ApplyWeaponDrawOffset(WeaponDrawOffsets offsets, Rot4 pawnRot, bool pawnMoving, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (offsets == null)
            {
                return;
            }

            Offset offsetData = null;

            if (pawnRot == Rot4.South)
            {
                offsetData = offsets.south;
            }
            else if (pawnRot == Rot4.North)
            {
                offsetData = offsets.north;
            }
            else if (pawnRot == Rot4.East)
            {
                offsetData = offsets.east;
            }
            else if (pawnRot == Rot4.West)
            {
                offsetData = offsets.west;
            }

            if (offsetData != null)
            {
                var drawOffsetToUse = pawnMoving && offsetData.drawOffsetWhileMoving.HasValue
                    ? offsetData.drawOffsetWhileMoving.Value
                    : offsetData.drawOffset;
                drawLoc += drawOffsetToUse;
                float angleOffsetToUse = pawnMoving && offsetData.angleOffsetWhileMoving.HasValue 
                    ? offsetData.angleOffsetWhileMoving.Value 
                    : offsetData.angleOffset;
                aimAngle += angleOffsetToUse;
            }
        }
    }

    

  

    [HarmonyPatch(typeof(HeadTypeDef), "GetGraphic")]
    public static class VanillaExpandedFramework_HeadTypeDef_GetGraphic_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_HeadTypeDef_GetGraphic_Patch), nameof(TryChangeShader)));
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static Shader TryChangeShader(Shader shader, HeadTypeDef def)
        {
            var extension = def.GetModExtension<HeadExtension>();
            if (extension?.forcedHeadShader != null)
            {
                return extension.forcedHeadShader.Shader;
            }
            return shader;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNode_AnimalPack), nameof(PawnRenderNode_AnimalPack.GraphicFor))]
    public static class PawnRenderNode_AnimalPack_GraphicFor_Patch
    {
        public static void Postfix(PawnRenderNode_AnimalPack __instance, ref Graphic __result, Pawn pawn)
        {
            if (__result != null)
            {
                var faction = pawn.Faction;
                // If the pawn's a pack animal and is part of a medieval faction, use medieval pack texture if applicable
                if (faction != null)
                {
                    var factionDefExtension = FactionDefExtension.Get(faction.def);
                    if (!factionDefExtension.packAnimalTexNameSuffix.NullOrEmpty())
                    {
                        PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
                        Graphic graphic = ((pawn.gender == Gender.Female && curKindLifeStage.femaleGraphicData != null) ? curKindLifeStage.femaleGraphicData.Graphic : curKindLifeStage.bodyGraphicData.Graphic);
                        var packGraphicPath = graphic.path + factionDefExtension.packAnimalTexNameSuffix;
                        var medievalPackTexture = ContentFinder<Texture2D>.Get(packGraphicPath + "_south", false);
                        if (medievalPackTexture != null)
                        {
                            __result = GraphicDatabase.Get<Graphic_Multi>(packGraphicPath, ShaderDatabase.CutoutComplex, graphic.drawSize, pawn.Faction.Color);
                        }
                    }
                }
            }
        }
    }
}
