using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Reloading
{
    public class HarmonyPatches
    {
        private static Harmony harm;
        private static FieldInfo thisPropertyInfo;

        private static readonly List<MethodInfo> patchedMethods = new List<MethodInfo>();

        public new static Type GetType()
        {
            return typeof(HarmonyPatches);
        }


        public static void Patch(MethodInfo target, HarmonyMethod prefix = null, HarmonyMethod postfix = null, string debug_targetName = null, Type debug_targetType = null)
        {
            if (target is null)
            {
                Log.Warning(
                    $"[MVCF] [Reloading] Target method of patch is null: Failed to find {debug_targetName} method of {debug_targetType?.Namespace}.{debug_targetType?.Name}");
                return;
            }

            if (patchedMethods.Contains(target)) return;
            Log.Message($"[MVCF] [Reloading] Patching method {target.DeclaringType?.Namespace}.{target.DeclaringType?.Name}.{target.Name}");
            patchedMethods.Add(target);
            harm.Patch(target, prefix, postfix);
        }

        public static void HasHuntingWeapon_Postfix(ref bool __result, Pawn p)
        {
            if (__result) __result = p.equipment.PrimaryEq.PrimaryVerb.IsStillUsableBy(p);
        }

        public static void MakeNewToils_Postfix(JobDriver_Hunt __instance)
        {
            __instance.FailOn(() => __instance.job?.verbToUse != null && __instance.job.verbToUse.IsMeleeAttack);
        }

        public static IEnumerable<CodeInstruction> EndJobIfVerbNotAvailable(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var list = instructions.ToList();
            var idx = list.FindIndex(ins => ins.IsLdarg(0));
            var idx2 = list.FindIndex(idx + 1, ins => ins.IsLdarg(0));
            var idx3 = list.FindIndex(idx2, ins => ins.opcode == OpCodes.Ret);
            var list2 = list.Skip(idx2).Take(idx3 - idx2).ToList().ListFullCopy();
            list2.Find(ins => ins.opcode == OpCodes.Ldc_I4_2).opcode = OpCodes.Ldc_I4_3;
            var idx4 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_2);
            var label = generator.DefineLabel();
            list[idx4 + 1].labels.Add(label);
            var list3 = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, thisPropertyInfo),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), "pawn")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(PawnCanCurrentlyUseVerb))),
                new CodeInstruction(OpCodes.Brtrue_S, label)
            };
            list3.AddRange(list2);
            list.InsertRange(idx4 + 1, list3);
            return list;
        }

        public static bool PawnCanCurrentlyUseVerb(Verb verb, Pawn pawn)
        {
            Log.Message($"Checking use of {verb} for {pawn} with job {pawn.CurJob}");
            if (verb.IsMeleeAttack && pawn.CurJobDef == JobDefOf.Hunt) return false;
            var reloadable = verb.GetReloadable();
            return (verb.IsStillUsableBy(pawn) || pawn.inventory.innerContainer.Any(t => reloadable.CanReloadFrom(t))) &&
                   (!verb.IsMeleeAttack || pawn.Position.DistanceTo(verb.CurrentTarget.Cell) < 1.43f);
        }

        public static void ReloadWeaponIfEndingCooldown(Stance_Busy __instance)
        {
            if (__instance.verb?.EquipmentSource == null) return;
            var pawn = __instance.verb.CasterPawn;
            if (pawn == null) return;
            var reloadable = __instance.verb.GetReloadable();
            if (reloadable == null || reloadable.ShotsRemaining != 0 || pawn.stances.curStance.StanceBusy) return;

            var item = pawn.inventory.innerContainer.FirstOrDefault(t => reloadable.CanReloadFrom(t));

            if (item == null)
            {
                if (!pawn.IsColonist && reloadable.Parent is ThingWithComps eq &&
                    !pawn.equipment.TryDropEquipment(eq, out var result, pawn.Position))
                    Log.Message("Failed to drop " + result);
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                if (pawn.inventory.innerContainer.FirstOrDefault(t => t.def.IsWeapon && t.def.equipmentType == EquipmentType.Primary) is ThingWithComps newWeapon)
                {
                    if (reloadable.Parent == pawn.equipment.Primary && reloadable.Parent is ThingWithComps oldWeapon)
                        pawn.inventory.innerContainer.TryAddOrTransfer(oldWeapon, false);
                    pawn.inventory.innerContainer.TryTransferToContainer(newWeapon, pawn.equipment.GetDirectlyHeldThings(), 1, false);
                }

                if (!pawn.IsColonist && (pawn.equipment.Primary?.def.IsMeleeWeapon ?? true)) pawn.GetLord()?.CurLordToil?.UpdateAllDuties();
                return;
            }

            var job = new Job(pawn.CurJobDef, pawn.CurJob.targetA, pawn.CurJob.targetB, pawn.CurJob.targetC)
            {
                canUseRangedWeapon = pawn.CurJob.canUseRangedWeapon,
                verbToUse = __instance.verb,
                endIfCantShootInMelee = pawn.CurJob.endIfCantShootInMelee
            };
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            pawn.jobs.TryTakeOrderedJob(JobGiver_ReloadFromInventory.MakeReloadJob(reloadable, item),
                JobTag.UnspecifiedLordDuty);
            if (!pawn.IsColonist) pawn.GetLord()?.CurLordToil?.UpdateAllDuties();
            else pawn.jobs.TryTakeOrderedJob(job, JobTag.UnspecifiedLordDuty, true);
        }

        public static void GenerateAdditionalAmmo(Pawn p, PawnGenerationRequest request)
        {
            foreach (var thingDefCountRange in from comp in p.AllReloadComps()
                let gen = comp.GenerateAmmo
                where gen != null
                from tdcr in gen
                select tdcr)
            {
            }

            foreach (var reloadable in p.AllReloadComps())
            {
                if (reloadable.GenerateAmmo != null)
                    foreach (var thingDefCountRange in reloadable.GenerateAmmo)
                    {
                        var ammo = ThingMaker.MakeThing(thingDefCountRange.thingDef);
                        ammo.stackCount = thingDefCountRange.countRange.RandomInRange;
                        p.inventory?.innerContainer.TryAdd(ammo);
                    }

                if (reloadable.GenerateBackupWeapon)
                {
                    var weaponPairs = Traverse.Create(typeof(PawnWeaponGenerator)).Field("allWeaponPairs").GetValue<List<ThingStuffPair>>().Where(w =>
                        !w.thing.IsRangedWeapon || !p.WorkTagIsDisabled(WorkTags.Shooting));
                    if (p.kindDef.weaponMoney.Span > 0f)
                    {
                        var money = p.kindDef.weaponMoney.RandomInRange / 5f;
                        weaponPairs = weaponPairs.Where(w => w.Price <= money);
                    }

                    if (p.kindDef.weaponStuffOverride != null)
                        weaponPairs = weaponPairs.Where(w => w.stuff == p.kindDef.weaponStuffOverride);

                    weaponPairs = weaponPairs.Where(w =>
                        w.thing.weaponClasses == null || w.thing.weaponClasses.Contains(ReloadingDefOf.RangedLight) && w.thing.weaponClasses.Contains(ReloadingDefOf.ShortShots) ||
                        w.thing.weaponTags.Contains("MedievalMeleeBasic") || w.thing.weaponTags.Contains("SimpleGun"));

                    if (weaponPairs.TryRandomElementByWeight(w => w.Price * w.Commonality, out var weaponPair))
                    {
                        var weapon = (ThingWithComps) ThingMaker.MakeThing(weaponPair.thing, weaponPair.stuff);
                        PawnGenerator.PostProcessGeneratedGear(weapon, p);
                        var num = request.BiocodeWeaponChance > 0f ? request.BiocodeWeaponChance : p.kindDef.biocodeWeaponChance;
                        if (Rand.Chance(num)) weapon.TryGetComp<CompBiocodable>()?.CodeFor(p);

                        var compEquippable = weapon.TryGetComp<CompEquippable>();
                        if (compEquippable != null)
                        {
                            if (p.kindDef.weaponStyleDef != null)
                                compEquippable.parent.StyleDef = p.kindDef.weaponStyleDef;
                            else if (p.Ideo != null) compEquippable.parent.StyleDef = p.Ideo.GetStyleFor(weapon.def);
                        }

                        p.inventory?.innerContainer.TryAdd(weapon, false);
                    }
                }
            }
        }

        public static void DoPatches()
        {
            if (harm != null) return;

            harm = new Harmony("legodude17.reloading");

            harm.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), postfix: ReloadingFloatMenuAdder.Method);
            harm.Patch(AccessTools.Method(typeof(VerbTracker), "CreateVerbTargetCommand"), ReloadingGizmos.Create);
            var type = typeof(JobDriver_AttackStatic).GetNestedType("<>c__DisplayClass4_0", BindingFlags.NonPublic);
            thisPropertyInfo = type.GetField("<>4__this", BindingFlags.Public | BindingFlags.Instance);
            harm.Patch(type.GetMethod("<MakeNewToils>b__1", BindingFlags.NonPublic | BindingFlags.Instance),
                transpiler: new HarmonyMethod(AccessTools.Method(GetType(), nameof(EndJobIfVerbNotAvailable))));
            harm.Patch(AccessTools.Method(typeof(Stance_Busy), "Expire"),
                postfix: new HarmonyMethod(GetType(), nameof(ReloadWeaponIfEndingCooldown)));
            harm.Patch(AccessTools.Method(typeof(PawnInventoryGenerator), "GenerateInventoryFor"),
                postfix: new HarmonyMethod(GetType(), nameof(GenerateAdditionalAmmo)));
            harm.Patch(AccessTools.Method(typeof(JobDriver_Hunt), "MakeNewToils"),
                postfix: new HarmonyMethod(GetType(), nameof(MakeNewToils_Postfix)));
            harm.Patch(AccessTools.Method(typeof(WorkGiver_HunterHunt), nameof(WorkGiver_HunterHunt.HasHuntingWeapon)),
                new HarmonyMethod(GetType(), nameof(HasHuntingWeapon_Postfix)));
            harm.Patch(AccessTools.Method(Type.GetType("MVCF.Utilities.PawnVerbGizmoUtility, MVCF"), "GetGizmosForVerb"), postfix: ReloadingGizmos.Use);
        }
    }
}