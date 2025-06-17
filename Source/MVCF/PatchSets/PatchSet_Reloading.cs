using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Reloading;
using MVCF.Utilities;
using Reloading;
using RimWorld;
using Verse;
using Verse.AI;
using FloatMenuUtility = MVCF.Utilities.FloatMenuUtility;

namespace MVCF.PatchSets;

public class PatchSet_Reloading : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Postfix(AccessTools.Method(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor"),
            AccessTools.Method(GetType(), nameof(PostGenerate)));
        yield return Patch.Prefix(AccessTools.Method(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), AccessTools.Method(GetType(), nameof(PreTryGiveJob)));
        yield return Patch.Prefix(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"), AccessTools.Method(GetType(), nameof(PreCheckAutoAttack)));
    }

    public static void PostGenerate(Pawn pawn, PawnGenerationRequest request)
    {
        foreach (var reloadable in pawn.AllReloadComps())
        {
            if (reloadable.Props.GenerateAmmo != null)
                foreach (var thingDefRange in reloadable.Props.GenerateAmmo)
                {
                    var ammo = ThingMaker.MakeThing(thingDefRange.thingDef);
                    ammo.stackCount = thingDefRange.countRange.RandomInRange;
                    pawn.inventory?.innerContainer.TryAdd(ammo);
                }

            if (reloadable.Props.GenerateAmmoCategories != null)
                foreach (var thingCategoryRange in reloadable.Props.GenerateAmmoCategories)
                    if (thingCategoryRange.Category.childThingDefs.TryRandomElement(out var thingDef))
                    {
                        var ammo = ThingMaker.MakeThing(thingDef);
                        ammo.stackCount = thingCategoryRange.Range.RandomInRange;
                        pawn.inventory?.innerContainer.TryAdd(ammo);
                    }

            if (reloadable.Props.GenerateBackupWeapon)
            {
                var weaponPairs = Traverse.Create(typeof(PawnWeaponGenerator))
                   .Field("allWeaponPairs")
                   .GetValue<List<ThingStuffPair>>()
                   .Where(w =>
                        !w.thing.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting));
                if (pawn.kindDef.weaponMoney.Span > 0f)
                {
                    var money = pawn.kindDef.weaponMoney.RandomInRange / 5f;
                    weaponPairs = weaponPairs.Where(w => w.Price <= money);
                }

                if (pawn.kindDef.weaponStuffOverride != null)
                    weaponPairs = weaponPairs.Where(w => w.stuff == pawn.kindDef.weaponStuffOverride);

                weaponPairs = weaponPairs.Where(w =>
                    w.thing.weaponClasses == null || (w.thing.weaponClasses.Contains(ReloadingDefOf.RangedLight) &&
                                                      w.thing.weaponClasses.Contains(ReloadingDefOf.ShortShots)) ||
                    w.thing.weaponTags.Contains("MedievalMeleeBasic") || w.thing.weaponTags.Contains("SimpleGun"));

                if (weaponPairs.TryRandomElementByWeight(w => w.Price * w.Commonality, out var weaponPair))
                {
                    var weapon = (ThingWithComps)ThingMaker.MakeThing(weaponPair.thing, weaponPair.stuff);
                    PawnGenerator.PostProcessGeneratedGear(weapon, pawn);
                    var num = request.BiocodeWeaponChance > 0f ? request.BiocodeWeaponChance : pawn.kindDef.biocodeWeaponChance;
                    if (Rand.Chance(num)) weapon.TryGetComp<CompBiocodable>()?.CodeFor(pawn);

                    if (pawn.kindDef.weaponStyleDef != null)
                        weapon.StyleDef = pawn.kindDef.weaponStyleDef;
                    else if (pawn.Ideo != null) weapon.StyleDef = pawn.Ideo.GetStyleFor(weapon.def);


                    pawn.inventory?.innerContainer.TryAdd(weapon, false);
                }
            }
        }
    }

    public static bool PreTryGiveJob(Pawn pawn, ref Job __result)
    {
        if (JobGiver_ReloadFromInventory.TryGiveReloadJob(pawn) is { } job)
        {
            __result = job;
            return false;
        }

        JobGiver_SwitchWeapon.TrySwitchWeapon(pawn);
        return true;
    }

    public static bool PreCheckAutoAttack(JobDriver_Wait __instance)
    {
        if (__instance.pawn.Downed) return true;
        if (__instance.pawn.stances.FullBodyBusy) return true;
        if (__instance.pawn.IsCarryingPawn()) return true;

        var comp = __instance.pawn.AllReloadComps().FirstOrDefault(r => r.AutoReload && r.NeedsReload() && r.ReloadItemInInventory != null);
        if (comp != null)
        {
            __instance.pawn.jobs.TryTakeOrderedJob(JobGiver_ReloadFromInventory.MakeReloadJob(comp, comp.ReloadItemInInventory), JobTag.DraftedOrder);
            return false;
        }

        return true;
    }
}
