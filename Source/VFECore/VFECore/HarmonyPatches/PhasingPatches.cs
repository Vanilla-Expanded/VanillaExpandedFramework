using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.Assertions;
using Verse;
using Verse.AI;

namespace VFECore
{
    public static class PhasingPatches
    {
        private static readonly MethodInfo FloodUnfogAdjMI = AccessTools.Method(typeof(FogGrid), "FloodUnfogAdjacent", new Type[] { typeof(IntVec3), typeof(bool) });

        private static Pawn patherPawn;
        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "GenerateNewPath"), new HarmonyMethod(typeof(PhasingPatches), nameof(GenerateNewPath_Prefix)));
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "CostToMoveIntoCell", new[] {typeof(Pawn), typeof(IntVec3)}),
                transpiler: new HarmonyMethod(typeof(PhasingPatches), nameof(CostToMoveIntoCell_Transpile)));
            harm.Patch(AccessTools.Method(typeof(GenGrid), nameof(GenGrid.WalkableBy)), new HarmonyMethod(typeof(PhasingPatches), nameof(WalkableBy_Prefix)));
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.BuildingBlockingNextPathCell)),
                new HarmonyMethod(typeof(PhasingPatches), nameof(NoBuildingBlocking)));
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "TryEnterNextPathCell"), postfix: new HarmonyMethod(typeof(PhasingPatches), nameof(UnfogEnteredCells)));
            harm.Patch(
                AccessTools.Method(typeof(Reachability), nameof(Reachability.CanReach),
                    new[] {typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms)}), new HarmonyMethod(typeof(PhasingPatches), nameof(AllReachable)));
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath)),
                new HarmonyMethod(typeof(PhasingPatches), nameof(StartPath_Prefix)), new HarmonyMethod(typeof(PhasingPatches), nameof(StartPath_Postfix)));
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.SpawnSetup)), postfix: new HarmonyMethod(typeof(PhasingPatches), nameof(CheckPhasing)));
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.DeSpawn)), postfix: new HarmonyMethod(typeof(PhasingPatches), nameof(Despawn_Postfix)));
        }

        public static void UnfogEnteredCells(Pawn_PathFollower __instance, Pawn ___pawn)
        {
            if (___pawn.Spawned && __instance.nextCell.Fogged(___pawn.Map) && ___pawn.IsPhasing())
                FloodUnfogAdjMI.Invoke(___pawn.Map.fogGrid, new object[] {__instance.nextCell, true});
        }

        public static bool AllReachable(TraverseParms traverseParams, ref bool __result)
        {
            if (traverseParams.pawn != null && traverseParams.pawn.IsPhasing() || patherPawn != null && patherPawn.IsPhasing())
            {
                __result = true;
                return false;
            }

            return true;
        }

        public static void StartPath_Prefix(Pawn ___pawn)
        {
            patherPawn = ___pawn;
        }

        public static void StartPath_Postfix()
        {
            patherPawn = null;
        }

        public static bool NoBuildingBlocking(ref Building __result, Pawn ___pawn)
        {
            if (___pawn.IsPhasing())
            {
                __result = null;
                return false;
            }

            return true;
        }

        public static bool WalkableBy_Prefix(ref bool __result, Pawn pawn, IntVec3 c)
        {
            if (pawn.IsPhasing())
            {
                __result = true;
                return false;
            }

            return true;
        }

        public static IEnumerable<CodeInstruction> CostToMoveIntoCell_Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.PropertyGetter(typeof(Thing), nameof(Pawn.Map));
            var idx1 = list.FindIndex(ins => ins.Calls(info1)) - 2;
            Assert.AreEqual(list[idx1].opcode, OpCodes.Ldloc_0);
            var info2 = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.CurJob));
            var idx2 = list.FindIndex(ins => ins.Calls(info2)) - 1;
            Assert.AreEqual(list[idx2].opcode, OpCodes.Ldarg_0);
            var label1 = generator.DefineLabel();
            var labels = list[idx1].ExtractLabels();
            list[idx2].labels.Add(label1);
            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PhasingUtils), nameof(PhasingUtils.IsPhasing))),
                new CodeInstruction(OpCodes.Brtrue, label1)
            });
            return list;
        }

        public static bool GenerateNewPath_Prefix(Pawn_PathFollower __instance, Pawn ___pawn, LocalTargetInfo ___destination, PathEndMode ___peMode, ref PawnPath __result)
        {
            if (___pawn.IsPhasing())
            {
                __instance.lastPathedTargetPosition = ___destination.Cell;
                __result = ___pawn.Map.pathFinder.FindPath(___pawn.Position, ___destination, new TraverseParms
                {
                    pawn = ___pawn,
                    alwaysUseAvoidGrid = false,
                    canBashDoors = true,
                    canBashFences = true,
                    fenceBlocked = false,
                    maxDanger = Danger.Deadly,
                    mode = TraverseMode.PassAllDestroyableThings
                }, ___peMode, new PathFinderCostTuning
                {
                    costBlockedDoor = 0,
                    costBlockedDoorPerHitPoint = 0,
                    costBlockedWallBase = 0,
                    costBlockedWallExtraForNaturalWalls = 0,
                    costBlockedWallExtraPerHitPoint = 0,
                    costOffLordWalkGrid = 0
                });
                return false;
            }

            return true;
        }

        public static void CheckPhasing(Pawn __instance)
        {
            if (__instance.IsPhasingSlow()) PhasingUtils.PhasingPawns.Add(__instance);
        }

        public static void Despawn_Postfix(Pawn __instance)
        {
            if (PhasingUtils.PhasingPawns.Contains(__instance)) PhasingUtils.PhasingPawns.Remove(__instance);
        }
    }

    public static class PhasingUtils
    {
        public static HashSet<Pawn> PhasingPawns = new();
        public static bool IsPhasing(this Pawn p) => PhasingPawns.Contains(p);
        public static bool IsPhasingSlow(this Pawn p) => p.health.hediffSet.GetAllComps().OfType<HediffComp_Phasing>().Any();
    }
}