using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class CompProperties_ThrowMote : CompProperties
    {
        public CompProperties_ThrowMote()
        {
            this.compClass = typeof(CompThrowMote);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (this.mote == null)
            {
                yield return "VFECore.CompThrowMote must have a mote assigned.";
            }
            if (this.emissionInterval == -1)
            {
                yield return "VFECore.CompThrowMote must have an emissionInterval.";
            }
            yield break;
        }

        public ThingDef mote;
        public int emissionInterval = -1;

        public int moteScale = 1;
        public int solidTime = -1;
        public int fadeOutTime = -1;
        public FloatRange speedRange = new FloatRange(0.6f, 0.75f);
        public FloatRange angleRange = new FloatRange(0f, 360f);
        public FloatRange rotationRange = new FloatRange(-60f, 60f);
    }

    public class CompThrowMote : ThingComp
    {
        private CompProperties_ThrowMote Props
        {
            get
            {
                return (CompProperties_ThrowMote)this.props;
            }
        }

		public int ticksSinceLastEmitted;
        public ThingDef customizedMoteDef;

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (this.Props.fadeOutTime != -1 || this.Props.solidTime != -1)
            {
                customizedMoteDef = this.Props.mote;
                if (this.Props.fadeOutTime != -1)
                {
                    customizedMoteDef.mote.fadeOutTime = this.Props.fadeOutTime;
                }
                if (this.Props.solidTime != -1)
                {
                    customizedMoteDef.mote.solidTime = this.Props.solidTime;
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
		{
            CompRefuelable compRefuelable = this.parent.GetComp<CompRefuelable>();
            CompFlickable compFlickable = this.parent.GetComp<CompFlickable>();

            if (compRefuelable != null && !compRefuelable.HasFuel)
			{
                return;
            }
            if (compFlickable != null && !compFlickable.SwitchIsOn)
            {
                return;
            }

            if (this.ticksSinceLastEmitted >= this.Props.emissionInterval)
            {
                this.Throw();
                this.ticksSinceLastEmitted = 0;
            }
            else
            {
                this.ticksSinceLastEmitted++;
            }
        }

        protected void Throw()
        {
            MoteThrown moteThrown;
            if (this.customizedMoteDef != null)
            {
                moteThrown = (MoteThrown)ThingMaker.MakeThing(this.customizedMoteDef, null);
            }
            else
            {
                moteThrown = (MoteThrown)ThingMaker.MakeThing(this.Props.mote, null);
            }
            moteThrown.Scale = 1.9f * this.Props.moteScale;
            moteThrown.rotationRate = (float)Rand.Range(this.Props.rotationRange.min, this.Props.rotationRange.max);
            moteThrown.exactPosition = this.parent.TrueCenter();
            moteThrown.SetVelocity((float)Rand.Range(this.Props.angleRange.min, this.Props.angleRange.max), Rand.Range(this.Props.speedRange.min, this.Props.speedRange.max));
            GenSpawn.Spawn(moteThrown, this.parent.TrueCenter().ToIntVec3(), this.parent.Map, WipeMode.Vanish);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceLastEmitted, "ticksSinceLastEmitted", 0, false);
            Scribe_Defs.Look(ref this.customizedMoteDef, "customizedMoteDef");
        }
    }
}
