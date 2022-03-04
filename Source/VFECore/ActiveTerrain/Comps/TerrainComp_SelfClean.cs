using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VFECore
{
 
    /// <summary>
    /// Self-cleaning floors.
    /// </summary>
    public class TerrainComp_SelfClean : TerrainComp
    {
        public float cleanProgress = float.NaN;

        public Filth currentFilth;

        public TerrainCompProperties_SelfClean Props { get { return (TerrainCompProperties_SelfClean)props; } }

        protected virtual bool CanClean { get { return true; } }

        public void StartClean()
        {
            if (currentFilth == null)
            {
                Log.Warning("Cannot start clean for filth because there is no filth selected. Canceling.");
                return;
            }
            if (currentFilth.def.filth == null)
            {
                Log.Error($"Filth of def {currentFilth.def.defName} cannot be cleaned because it has no FilthProperties.");
                return;
            }
            cleanProgress = currentFilth.def.filth.cleaningWorkToReduceThickness;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (CanClean)
            {
                DoCleanWork();
            }
        }

        public virtual void DoCleanWork()
        {
            if (currentFilth == null)
            {
                cleanProgress = float.NaN;
                if (!FindFilth())
                    return;
            }

            if (float.IsNaN(cleanProgress))
                StartClean();

            if (cleanProgress > 0f)
                cleanProgress -= 1f;
            else
                FinishClean();
        }

        public bool FindFilth()
        {
            if (currentFilth != null)
            {
                return true;
            }
            var filth = (Filth)parent.Position.GetThingList(parent.Map).Find(f => f is Filth);
            if (filth != null)
            {
                currentFilth = filth;
                return true;
            }
            return false;
        }

        public void FinishClean()
        {
            if (currentFilth == null)
            {
                Log.Warning("Cannot finish clean for filth because there is no filth selected. Canceling.");
                return;
            }
            currentFilth.ThinFilth();
            if (currentFilth.Destroyed)
                currentFilth = null;
            else
                cleanProgress = float.NaN;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cleanProgress, "cleanProgress", float.NaN);
            Scribe_References.Look(ref currentFilth, "currentFilth");
        }
    }
}
