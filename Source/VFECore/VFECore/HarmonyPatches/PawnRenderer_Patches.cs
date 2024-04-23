using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class ApparelDrawPosExtension : DefModExtension
    {
        public List<ThingDef> secondaryApparelGraphics;
        public bool isUnifiedApparel;
        public bool hideHead;
        public bool showBodyInBedAlways;
    }

    [HarmonyPatch(typeof(PawnRenderTree), "ProcessApparel")]
    public static class PawnRenderTree_ProcessApparel_Patch
    {
        public delegate void ProcessApparel(PawnRenderTree __instance, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode);
        public static readonly ProcessApparel processApparel = AccessTools.MethodDelegate<ProcessApparel>
            (AccessTools.Method(typeof(PawnRenderTree), "ProcessApparel"));

        public static void Postfix(PawnRenderTree __instance, Apparel ap, PawnRenderNode headApparelNode, PawnRenderNode bodyApparelNode)
        {
            var extension = ap.def.GetModExtension<ApparelDrawPosExtension>();
            if (extension?.secondaryApparelGraphics != null)
            {
                foreach (var thingDef in extension.secondaryApparelGraphics)
                {
                    var item = ThingMaker.MakeThing(thingDef) as Apparel;
                    if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, __instance.pawn.story.bodyType, out var rec))
                    {
                        processApparel(__instance, item, headApparelNode, bodyApparelNode);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler
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
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler), nameof(IsUnifiedApparel)));
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
    public static class FemaleBB_BodyType_Support_Patch
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
            if (Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel_Transpiler.IsUnifiedApparel(apparel))
            {
                __3 = true;
                __result = true;
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
    public static class PawnRenderer_DrawEquipmentAiming_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            var thingDefExtension = eq.def.GetModExtension<ThingDefExtension>();

            Pawn pawn = eq.GetPawnAsHolder();

            if (thingDefExtension != null && PawnRenderUtility.CarryWeaponOpenly(pawn))
            {
                var pawnRot = pawn.Rotation;

                // Weapon draw offsets that apply at all times (i.e. carrying weapons while working, drafted, attacking)
                // Replaces the now-defunct CompOversizedWeapon
                if (thingDefExtension.weaponCarryDrawOffsets != null)
                {
                    if (pawnRot == Rot4.South)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.south.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.south.angleOffset;
                    }
                    else if (pawnRot == Rot4.North)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.north.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.north.angleOffset;
                    }
                    else if (pawnRot == Rot4.East)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.east.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.east.angleOffset;
                    }
                    else if (pawnRot == Rot4.West)
                    {
                        drawLoc += thingDefExtension.weaponCarryDrawOffsets.west.drawOffset;
                        aimAngle += thingDefExtension.weaponCarryDrawOffsets.west.angleOffset;
                    }
                }

                // Weapon draw offsets that only apply when the pawn is drafted but *not* actively attacking
                // Useful for things like holding a pike/halberd while standing at attention
                //
                // Note: These offsets add on to anything in weaponCarryDrawOffsets
                if (thingDefExtension.weaponDraftedDrawOffsets != null && !pawn.stances.curStance.StanceBusy)
                {
                    if (pawnRot == Rot4.South)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.south.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.south.angleOffset;
                    }
                    else if (pawnRot == Rot4.North)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.north.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.north.angleOffset;
                    }
                    else if (pawnRot == Rot4.East)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.east.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.east.angleOffset;
                    }
                    else if (pawnRot == Rot4.West)
                    {
                        drawLoc += thingDefExtension.weaponDraftedDrawOffsets.west.drawOffset;
                        aimAngle += thingDefExtension.weaponDraftedDrawOffsets.west.angleOffset;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    public static class PawnRenderer_GetBodyPos_Patch
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
    public static class PawnRenderNodeWorker_Body_CanDrawNow_Patch
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
    public static class PawnRenderNodeWorker_AppendDrawRequests_Patch
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
    public static class PawnRenderNodeWorker_Apparel_Head_CanDrawNow_Patch
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
    public static class PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch
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
                        AccessTools.Method(typeof(PawnRenderNodeWorker_Apparel_Head_HeadgearVisible_Patch), 
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
    public static class HeadTypeDef_GetGraphic_Patch
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
                        AccessTools.Method(typeof(HeadTypeDef_GetGraphic_Patch), nameof(TryChangeShader)));
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
