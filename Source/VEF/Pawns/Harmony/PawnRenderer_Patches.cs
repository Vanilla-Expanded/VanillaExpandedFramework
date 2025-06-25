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

namespace VEF.Pawns
{
    [HarmonyPatch(typeof(DynamicPawnRenderNodeSetup_Apparel), "ProcessApparel")]
    public static class VanillaExpandedFramework_DynamicPawnRenderNodeSetup_Apparel_ProcessApparel_Patch
    {
        public delegate IEnumerable<ValueTuple<PawnRenderNode, PawnRenderNode>> ProcessApparel(Pawn pawn, PawnRenderTree tree, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode, Dictionary<PawnRenderNode, int> layerOffsets);
        public static readonly ProcessApparel processApparel = AccessTools.MethodDelegate<ProcessApparel>
            (AccessTools.Method(typeof(DynamicPawnRenderNodeSetup_Apparel), "ProcessApparel"));

        public static IEnumerable<ValueTuple<PawnRenderNode, PawnRenderNode>> Postfix(IEnumerable<ValueTuple<PawnRenderNode, PawnRenderNode>> result, Pawn pawn, PawnRenderTree tree, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode, Dictionary<PawnRenderNode, int> layerOffsets)
        {
            var extension = ap.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.secondaryApparelGraphics != null)
            {
                foreach (var thingDef in extension.secondaryApparelGraphics)
                {
                    var item = ThingMaker.MakeThing(thingDef) as Apparel;
                    if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, pawn.story.bodyType, false, out _))
                        result = result.Concat(processApparel(pawn, tree, item, headApparelNode, bodyApparelNode, layerOffsets));
                }
            }

            return result;
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class VanillaExpandedFramework_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var renderAsPackMethod = AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.RenderAsPack));
            var codes = codeInstructions.ToList();
            bool found = false;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(renderAsPackMethod))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VanillaExpandedFramework_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler), nameof(IsUnifiedApparel)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand);
                }
            }
            if (!found)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on ApparelGraphicRecordGetter:TryGetGraphicApparel failed.");
            }
        }

        public static bool IsUnifiedApparel(Apparel apparel)
        {
            var extension = apparel.def.GetModExtension<ApparelDrawPosExtension>();
            return extension != null && extension.isUnifiedApparel;
        }
    }

    [HarmonyPatch]
    public static class VanillaExpandedFramework_FemaleBB_BodyType_Support_Patch
    {
        private static MethodBase target;
        private static bool Prepare()
        {
            target = AccessTools.Method(AccessTools.TypeByName("BBBodySupport.BBBodyTypeSupportHarmony+BBBodyGraphicApparelPatch"), "BBBody_ApparelPatch");
            return target != null;
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
            var thingDefExtension = eq.def.GetModExtension<ThingDefExtension>();

            Pawn pawn = eq.GetPawnAsHolder();

            if (pawn != null && thingDefExtension != null && PawnRenderUtility.CarryWeaponOpenly(pawn))
            {
                var pawnRot = pawn.Rotation;
                var pawnIsMoving = pawn.pather.Moving;

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

    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    public static class VanillaExpandedFramework_PawnRenderer_GetBodyPos_Patch
    {
        public static void Postfix(Pawn ___pawn, Vector3 drawLoc, ref bool showBody)
        {
            if (!showBody)
            {
                var pawn = ___pawn;
                if (pawn.apparel != null && ___pawn.CurrentBed() != null)
                {
                    if (pawn.apparel.WornApparel.Any(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.showBodyInBedAlways ?? false))
                    {
                        showBody = true;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Body), "CanDrawNow")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_Body_CanDrawNow_Patch
    {
        public static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
        {
            if (__result is false && parms.bed != null && parms.pawn.apparel != null)
            {
                if (parms.pawn.apparel.WornApparel.Any(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.showBodyInBedAlways ?? false))
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker), "AppendDrawRequests")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_AppendDrawRequests_Patch
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            if ((node is PawnRenderNode_Head || node.parent is PawnRenderNode_Head) && parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    requests.Add(new PawnGraphicDrawRequest(node)); // adds an empty draw request to not draw head
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "CanDrawNow")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_Apparel_Head_CanDrawNow_Patch
    {
        public static void Prefix(PawnDrawParms parms, out bool __state)
        {
            __state = Prefs.HatsOnlyOnMap;
            if (parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    Prefs.HatsOnlyOnMap = false;
                }
            }
        }

        public static void Postfix(bool __state)
        {
            Prefs.HatsOnlyOnMap = __state;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "HeadgearVisible")]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var get_HatsOnlyOnMap = AccessTools.PropertyGetter(typeof(Prefs), nameof(Prefs.HatsOnlyOnMap));
            foreach ( var codeInstruction in codeInstructions )
            {
                yield return codeInstruction;
                if (codeInstruction.Calls(get_HatsOnlyOnMap))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch), 
                        "TryOverrideHatsOnlyOnMap"));
                }
            }
        }

        public static bool TryOverrideHatsOnlyOnMap(bool result, PawnDrawParms parms)
        {
            if (result is true && parms.pawn.apparel.AnyApparel)
            {
                var headgear = parms.pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.GetModExtension<ApparelDrawPosExtension>()?.hideHead ?? false);
                if (headgear != null)
                {
                    return false;
                }
            }
            return result;
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
