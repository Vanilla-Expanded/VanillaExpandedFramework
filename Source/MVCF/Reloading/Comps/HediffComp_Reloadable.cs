using System;
using System.Collections.Generic;
using System.Linq;
using MVCF;
using MVCF.Reloading.Comps;
using MVCF.VerbComps;
using Verse;
using Verse.Sound;

namespace Reloading
{
    public class HediffComp_Reloadable : HediffComp, IReloadable
    {
        public HediffCompProperties_Reloadable Props => props as HediffCompProperties_Reloadable;

        public List<ThingDefCountRangeClass> GenerateAmmo => Props.GenerateAmmo;

        public int ShotsRemaining { get; set; }
        public int ItemsPerShot => Props.ItemsPerShot;
        public virtual ThingDef CurrentProjectile => null;
        public int MaxShots => Props.MaxShots;
        public Thing Thing => parent.pawn;
        public object Parent => parent;

        public virtual Thing Reload(Thing ammo)
        {
            if (!CanReloadFrom(ammo)) return null;
            var shotsToFill = ShotsToReload(ammo);
            ShotsRemaining += shotsToFill;
            return ammo.SplitOff(shotsToFill * ItemsPerShot);
        }

        public virtual int ReloadTicks(Thing ammo) => ammo == null ? 0 : (Props.ReloadTimePerShot * ShotsToReload(ammo)).SecondsToTicks();

        public virtual bool NeedsReload() => ShotsRemaining < MaxShots;

        public virtual bool CanReloadFrom(Thing ammo)
        {
            // Log.Message(ammo + " x" + ammo.stackCount);
            if (ammo == null) return false;
            return Props.AmmoFilter.Allows(ammo) && ammo.stackCount >= ItemsPerShot;
        }

        public virtual void Unload()
        {
            var thing = ThingMaker.MakeThing(Props.AmmoFilter.AnyAllowedDef);
            thing.stackCount = ShotsRemaining;
            ShotsRemaining = 0;
            GenPlace.TryPlaceThing(thing, parent.pawn.Position, parent.pawn.Map, ThingPlaceMode.Near);
        }

        public virtual void Notify_ProjectileFired()
        {
            ShotsRemaining--;
        }

        public void ReloadEffect(int curTick, int ticksTillDone)
        {
            if (curTick == ticksTillDone - 2f.SecondsToTicks()) Props.ReloadSound?.PlayOneShot(parent.pawn);
        }

        public ThingDef AmmoExample => Props.AmmoFilter.AnyAllowedDef;

        public bool GenerateBackupWeapon => Props.GenerateBackupWeapon;

        public string GetUniqueLoadID() => $"{parent.GetUniqueLoadID()}_Reloadable";

        public IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps)
        {
            if (verbProps.label is null || verbProps.label == Props.VerbLabel)
                yield return new VerbCompProperties_Reloadable
                {
                    AmmoFilter = Props.AmmoFilter,
                    GenerateAmmo = Props.GenerateAmmo,
                    GenerateBackupWeapon = Props.GenerateBackupWeapon,
                    ItemsPerShot = Props.ItemsPerShot,
                    MaxShots = Props.MaxShots,
                    NewVerbClass = Props.NewVerbClass,
                    ReloadTimePerShot = Props.ReloadTimePerShot,
                    ReloadSound = Props.ReloadSound,
                    compClass = typeof(VerbComp_Reloadable)
                };
        }

        private int ShotsToReload(Thing ammo) => Math.Min(ammo.stackCount / Props.ItemsPerShot, Props.MaxShots - ShotsRemaining);

        public override void CompExposeData()
        {
            base.CompExposeData();
            var sr = ShotsRemaining;
            Scribe_Values.Look(ref sr, "ShotsRemaining");
            ShotsRemaining = sr;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ShotsRemaining = Props.MaxShots;
        }
    }

    public class HediffCompProperties_Reloadable : HediffCompProperties
    {
        public ThingFilter AmmoFilter;
        public List<ThingDefCountRangeClass> GenerateAmmo;
        public bool GenerateBackupWeapon;
        public int ItemsPerShot;
        public int MaxShots;
        public Type NewVerbClass;
        public SoundDef ReloadSound;
        public float ReloadTimePerShot;
        public string VerbLabel;

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            if (TargetVerb(parentDef) == null) yield return "Cannot find verb to be reloaded.";

            foreach (var e in base.ConfigErrors(parentDef)) yield return e;
        }

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            AmmoFilter.ResolveReferences();
            Base.EnabledFeatures.Add("Reloading");
            Base.EnabledFeatures.Add("VerbComps");
            Base.EnabledFeatures.Add("ExtraEquipmentVerbs");
            var verb = TargetVerb(parent);
            if (verb == null) return;
            if (NewVerbClass != null) verb.verbClass = NewVerbClass;
            // PatchSet_ReloadingAuto.RegisterVerb(verb.verbClass, PatchFirstFound);
        }

        private VerbProperties TargetVerb(HediffDef parent)
        {
            var verbs = parent.CompProps<HediffCompProperties_VerbGiver>().verbs;
            return VerbLabel.NullOrEmpty()
                ? verbs.FirstOrDefault()
                : verbs.FirstOrDefault(v => v.label == VerbLabel);
        }
    }
}