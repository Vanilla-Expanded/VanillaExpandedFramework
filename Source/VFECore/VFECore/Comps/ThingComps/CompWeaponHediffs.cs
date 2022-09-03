using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class CompProperties_WeaponHediffs : CompProperties
    {
        public List<HediffDef> hediffs;
        public CompProperties_WeaponHediffs()
        {
            this.compClass = typeof(CompWeaponHediffs);
        }
    }
    public class CompWeaponHediffs : ThingComp
    {
        public Pawn wearer = null;
        public List<Hediff> wearerHediffs = new List<Hediff>();
        public CompProperties_WeaponHediffs Props => base.props as CompProperties_WeaponHediffs;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            AssignHediffs();
        }
        public override void CompTick()
        {
            base.CompTick();
            AssignHediffs();
        }

        public void AssignHediffs()
        {
            if (this.parent is ThingWithComps weapon)
            {
                var hediffs = this.Props.hediffs;
                if (weapon.ParentHolder is not Pawn_EquipmentTracker tracker || tracker.pawn != wearer)
                {
                    if (wearer != null)
                    {
                        foreach (var hediff in wearerHediffs)
                        {
                            wearer.health.hediffSet.hediffs.Remove(hediff);
                        }
                        wearerHediffs.Clear();
                        wearer = null;
                    }
                }
                if (weapon.ParentHolder is Pawn_EquipmentTracker tracker2 && tracker2.pawn != null 
                    && (tracker2.pawn != wearer || (wearerHediffs?.Any(x => x?.pawn == tracker2.pawn) is false)))
                {
                    wearerHediffs = new List<Hediff>();
                    foreach (var hediffDef in hediffs)
                    {
                        var hediff = HediffMaker.MakeHediff(hediffDef, tracker2.pawn);
                        tracker2.pawn.health.AddHediff(hediff);
                        wearerHediffs.Add(hediff);
                    }
                    wearer = tracker2.pawn;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref wearer, this.GetType() + "_wearer");
            Scribe_Collections.Look(ref wearerHediffs, this.GetType() + "_wearerHediffs", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                AssignHediffs();
            }
        }
    }
}

