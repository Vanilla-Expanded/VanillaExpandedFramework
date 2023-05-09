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
        yield return Patch.Postfix(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
            AccessTools.Method(typeof(FloatMenuUtility), nameof(FloatMenuUtility.AddWeaponReloadOrders)));
        yield return Patch.Postfix(AccessTools.Method(typeof(PawnInventoryGenerator), "GenerateInventoryFor"),
            AccessTools.Method(GetType(), nameof(PostGenerate)));
        yield return Patch.Prefix(AccessTools.Method(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), AccessTools.Method(GetType(), nameof(PreTryGiveJob)));
        yield return Patch.Prefix(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"), AccessTools.Method(GetType(), nameof(PreCheckAutoAttack)));
    }

    public static void PostGenerate(Pawn p, PawnGenerationRequest request)
    {
        foreach (var reloadable in p.AllReloadComps())
        {
            if (reloadable.Props.GenerateAmmo != null)
                foreach (var thingDefRange in reloadable.Props.GenerateAmmo)
                {
                    var ammo = ThingMaker.MakeThing(thingDefRange.thingDef);
                    ammo.stackCount = thingDefRange.countRange.RandomInRange;
                    p.inventory?.innerContainer.TryAdd(ammo);
                }

            if (reloadable.Props.GenerateAmmoCategories != null)
                foreach (var thingCategoryRange in reloadable.Props.GenerateAmmoCategories)
                    if (thingCategoryRange.Category.childThingDefs.TryRandomElement(out var thingDef))
                    {
                        var ammo = ThingMaker.MakeThing(thingDef);
                        ammo.stackCount = thingCategoryRange.Range.RandomInRange;
                        p.inventory?.innerContainer.TryAdd(ammo);
                    }

            if (reloadable.Props.GenerateBackupWeapon)
            {
                var weaponPairs = Traverse.Create(typeof(PawnWeaponGenerator))
                   .Field("allWeaponPairs")
                   .GetValue<List<ThingStuffPair>>()
                   .Where(w =>
                        !w.thing.IsRangedWeapon || !p.WorkTagIsDisabled(WorkTags.Shooting));
                if (p.kindDef.weaponMoney.Span > 0f)
                {
                    var money = p.kindDef.weaponMoney.RandomInRange / 5f;
                    weaponPairs = weaponPairs.Where(w => w.Price <= money);
                }

                if (p.kindDef.weaponStuffOverride != null)
                    weaponPairs = weaponPairs.Where(w => w.stuff == p.kindDef.weaponStuffOverride);

                weaponPairs = weaponPairs.Where(w =>
                    w.thing.weaponClasses == null || (w.thing.weaponClasses.Contains(ReloadingDefOf.RangedLight) &&
                                                      w.thing.weaponClasses.Contains(ReloadingDefOf.ShortShots)) ||
                    w.thing.weaponTags.Contains("MedievalMeleeBasic") || w.thing.weaponTags.Contains("SimpleGun"));

                if (weaponPairs.TryRandomElementByWeight(w => w.Price * w.Commonality, out var weaponPair))
                {
                    var weapon = (ThingWithComps)ThingMaker.MakeThing(weaponPair.thing, weaponPair.stuff);
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
