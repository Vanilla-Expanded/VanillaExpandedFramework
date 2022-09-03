using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VFECore
{
    public class CompApparelHediffs : ThingComp
    {
        public Pawn wearer = null;

        public List<Hediff> wearerHediffs = new List<Hediff>();
        public CompProperties_ApparelHediffs Props
        {
            get
            {
                return (CompProperties_ApparelHediffs)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent is Apparel apparel && apparel.Wearer != wearer)
            {
                //Log.Message("wearer: " + wearer + " - apparel.Wearer: " + apparel.Wearer, true);
                var hediffStrings = this.Props.hediffDefnames;
                if (hediffStrings != null && hediffStrings.Count > 0)
                {
                    if (wearer != null) // it removes the hediffs from the previous owner
                    {
                        foreach (var hediff in wearerHediffs)
                        {
                            //Log.Message("Remove it " + hediff);
                            wearer.health.hediffSet.hediffs.Remove(hediff);
                        }
                    }
                    if (apparel.Wearer != null)
                    {
                        wearerHediffs.Clear();
                        foreach (var hediffDefName in hediffStrings) //it adds hediffs to the new owner
                        {
                            var hediff = HediffMaker.MakeHediff(HediffDef.Named(hediffDefName), apparel.Wearer);
                            if (hediff != null)
                            {
                                //Log.Message("Add it " + hediff);
                                apparel.Wearer.health.AddHediff(hediff);
                                wearerHediffs.Add(hediff);
                            }
                        }
                    }
                }
                wearer = apparel.Wearer;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Pawn>(ref wearer, "wearer");
            Scribe_Collections.Look<Hediff>(ref wearerHediffs, "wearerHediffs", LookMode.Reference);
        }
    }
}

