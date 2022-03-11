using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Reloading;
using MVCF.Utilities;
using Reloading;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using FloatMenuUtility = MVCF.Utilities.FloatMenuUtility;

namespace MVCF.Features
{
    public class Feature_Reloading : Feature_Humanoid
    {
        private static readonly Type AttackStaticSubType = typeof(JobDriver_AttackStatic).GetNestedType("<>c__DisplayClass4_0", BindingFlags.NonPublic);
        private static readonly FieldInfo thisPropertyInfo = AttackStaticSubType.GetField("<>4__this", BindingFlags.Public | BindingFlags.Instance);
        public override string Name => "Reloading";

        public override IEnumerable<Patch> GetPatches()
        {
            foreach (var patch in base.GetPatches()) yield return patch;

            yield return Patch.Postfix(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                AccessTools.Method(typeof(FloatMenuUtility), nameof(FloatMenuUtility.AddWeaponReloadOrders)));
            // yield return Patch.Transpiler(AttackStaticSubType.GetMethod("<MakeNewToils>b__1", BindingFlags.NonPublic | BindingFlags.Instance),
            //     AccessTools.Method(GetType(), nameof(EndJobIfVerbNotAvailable)));
            yield return Patch.Postfix(AccessTools.Method(typeof(Stance_Busy), "Expire"),
                AccessTools.Method(GetType(), nameof(ReloadWeaponIfEndingCooldown)));
            yield return Patch.Postfix(AccessTools.Method(typeof(PawnInventoryGenerator), "GenerateInventoryFor"),
                AccessTools.Method(GetType(), nameof(PostGenerate)));
        }

        public static IEnumerable<CodeInstruction> EndJobIfVerbNotAvailable(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var list = instructions.ToList();
            var idx = list.FindIndex(ins => ins.IsLdarg(0));
            var idx2 = list.FindIndex(idx + 1, ins => ins.IsLdarg(0));
            var idx3 = list.FindIndex(idx2, ins => ins.opcode == OpCodes.Ret);
            var list2 = list.Skip(idx2).Take(idx3 - idx2 + 1).ToList().ListFullCopy();
            list2.Find(ins => ins.opcode == OpCodes.Ldc_I4_2).opcode = OpCodes.Ldc_I4_3;
            var idx4 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_2);
            var label = generator.DefineLabel();
            list[idx4 + 1].labels.Add(label);
            var list3 = new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_2),
                new(OpCodes.Brfalse_S, label),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, thisPropertyInfo),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), "pawn")),
                new(OpCodes.Call, AccessTools.Method(typeof(Feature_Reloading), nameof(PawnCanCurrentlyUseVerb))),
                new(OpCodes.Brtrue_S, label)
            };
            list3.AddRange(list2);
            list.InsertRange(idx4 + 1, list3);
            return list;
        }

        public static bool PawnCanCurrentlyUseVerb(Verb verb, Pawn pawn)
        {
            Log.Message($"Checking use of {verb} for {pawn} with job {pawn.CurJob}");
            if (verb.IsMeleeAttack)
            {
                if (pawn.CurJobDef == JobDefOf.Hunt) return false;
                return pawn.Position.DistanceTo(verb.CurrentTarget.Cell) < 1.43f;
            }

            var reloadable = verb.GetReloadable();
            if (verb.IsStillUsableBy(pawn)) return true;
            return reloadable != null && pawn.inventory.innerContainer.Any(t => reloadable.CanReloadFrom(t));
        }

        public static void ReloadWeaponIfEndingCooldown(Stance_Busy __instance)
        {
            if (__instance.verb?.EquipmentSource == null) return;
            var pawn = __instance.verb.CasterPawn;
            if (pawn == null) return;
            var reloadable = __instance.verb.GetReloadable();
            if (reloadable is not {ShotsRemaining: 0} || pawn.stances.curStance.StanceBusy) return;

            var item = pawn.inventory.innerContainer.FirstOrDefault(t => reloadable.CanReloadFrom(t));

            if (item == null)
            {
                // if (!pawn.IsColonist && reloadable.Parent is ThingWithComps eq &&
                //     !pawn.equipment.TryDropEquipment(eq, out var result, pawn.Position))
                //     Log.Message("Failed to drop " + result);
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                if (pawn.inventory.innerContainer.FirstOrDefault(t => t.def.IsWeapon && t.def.equipmentType == EquipmentType.Primary) is ThingWithComps newWeapon)
                    // if (pawn.equipment.Primary && reloadable.Parent is ThingWithComps oldWeapon)
                    //     pawn.inventory.innerContainer.TryAddOrTransfer(oldWeapon, false);
                    pawn.inventory.innerContainer.TryTransferToContainer(newWeapon, pawn.equipment.GetDirectlyHeldThings(), 1, false);

                if (!pawn.IsColonist && (pawn.equipment.Primary?.def.IsMeleeWeapon ?? true)) pawn.GetLord()?.CurLordToil?.UpdateAllDuties();
                return;
            }

            pawn.jobs.StartJob(JobGiver_ReloadFromInventory.MakeReloadJob(reloadable, item), JobCondition.InterruptForced, null, true);
        }

        public static void PostGenerate(Pawn p, PawnGenerationRequest request)
        {
            foreach (var reloadable in p.AllReloadComps())
            {
                if (reloadable.Props.GenerateAmmo != null)
                    foreach (var thingDefCountRange in reloadable.Props.GenerateAmmo)
                    {
                        var ammo = ThingMaker.MakeThing(thingDefCountRange.thingDef);
                        ammo.stackCount = thingDefCountRange.countRange.RandomInRange;
                        p.inventory?.innerContainer.TryAdd(ammo);
                    }

                if (reloadable.Props.GenerateBackupWeapon)
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

                        if (p.kindDef.weaponStyleDef != null)
                            weapon.StyleDef = p.kindDef.weaponStyleDef;
                        else if (p.Ideo != null) weapon.StyleDef = p.Ideo.GetStyleFor(weapon.def);


                        p.inventory?.innerContainer.TryAdd(weapon, false);
                    }
                }
            }
        }
    }
}