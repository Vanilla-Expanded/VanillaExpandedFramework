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
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent is ThingWithComps weapon && 
                (weapon.ParentHolder is not Pawn_EquipmentTracker tracker || tracker.pawn != wearer))
            {
                var hediffs = this.Props.hediffs;
                if (wearer != null)
                {
                    foreach (var hediff in wearerHediffs)
                    {
                        wearer.health.hediffSet.hediffs.Remove(hediff);
                    }
                }

                if (weapon.ParentHolder is Pawn_EquipmentTracker tracker2 && tracker2.pawn != null)
                {
                    wearerHediffs.Clear();
                    foreach (var hediffDef in hediffs) //it adds hediffs to the new owner
                    {
                        var hediff = HediffMaker.MakeHediff(hediffDef, tracker2.pawn);
                        if (hediff != null)
                        {
                            tracker2.pawn.health.AddHediff(hediff);
                            wearerHediffs.Add(hediff);
                        }
                    }
                    wearer = tracker2.pawn;
                }
                else
                {
                    wearer = null;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref wearer, this.GetType() + "_wearer");
            Scribe_Collections.Look(ref wearerHediffs, this.GetType() + "_wearerHediffs", LookMode.Reference);
        }
    }
}

