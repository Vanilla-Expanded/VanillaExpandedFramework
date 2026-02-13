using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Noise;

namespace PipeSystem
{
    public class ThingAndResourceOwner : IExposable
    {
        private int wantedCount;

        private bool beingFilled;
        private PipeNetDef pipeNetDef;
        private ThingDef thingDef;
        private ThingCategoryDef thingCategoryDef;
        private int count = 0;

        public ThingDef ThingDef => thingDef;

        public PipeNetDef PipeNetDef => pipeNetDef;

        public ThingCategoryDef ThingCategoryDef => thingCategoryDef;

        public bool Require => count < wantedCount;

        public bool BeingFilled
        {
            get => beingFilled;
            internal set => beingFilled = value;
        }

        public int Required => wantedCount - count;

        public int Count => count;

        public ThingDef lastThingStored;
        public ThingDef stuffOfLastThingStored;

        public ThingAndResourceOwner()
        { }

        public ThingAndResourceOwner(ThingDef thingDef, PipeNetDef pipeNetDef, int wantedCount, ThingCategoryDef thingCategory)
        {
            this.pipeNetDef = pipeNetDef;
            this.thingDef = thingDef;
            this.wantedCount = wantedCount;
            this.thingCategoryDef = thingCategory;
            beingFilled = false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref wantedCount, "wantedCount");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref beingFilled, "beingFilled");
            Scribe_Defs.Look(ref pipeNetDef, "pipeNetDef");
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Defs.Look(ref thingCategoryDef, "thingCategoryDef");
            Scribe_Defs.Look(ref lastThingStored, "lastThingStored");
        }

        public void AddFromThing(Thing thing)
        {
          

            if (thingDef != null && thing.def != thingDef)
            {
                return;
            }
            if (thingCategoryDef != null)
            {
                List<ThingCategoryDef> allRootAndChildCategories = ProcessUtility.AllChildrenCategories(thingCategoryDef);
                
                if (!thing.def.thingCategories.ToList().Intersect(allRootAndChildCategories).Any())
                    return;
            }

            lastThingStored = thing.def;
            stuffOfLastThingStored = thing.Stuff;
            var needed = wantedCount - count;
            if (thing.stackCount > needed)
            {
                if (needed != 0)
                {
                    var taken = thing.SplitOff(needed);
                    count += taken.stackCount;
                    taken.Destroy();
                }
                
            }
            else
            {
                count += thing.stackCount;
                thing.Destroy();
            }
        }

        public void AddFromNet(PipeNet net)
        {
            if (pipeNetDef == null || net.def != pipeNetDef)
                return;

            var needed = wantedCount - count;
            var available = (int)net.Stored;

            if (needed > available)
            {
                net.DrawAmongStorage(available, net.storages);
                count += available;
            }
            else
            {
                net.DrawAmongStorage(needed, net.storages);
                count += needed;
            }
        }

        public void Reset()
        {
            count = 0;
            beingFilled = false;
            lastThingStored = null;
            stuffOfLastThingStored = null;
        }

        public override string ToString()
        {
            return $"Owner ({ThingDef?.defName} {PipeNetDef?.defName} {ThingCategoryDef?.defName}): {count}/{wantedCount}";
        }

        public string ToStringHumanReadable(ThingDef def = null)
        {
            return GetLabel(def ?? thingDef);
        }

        private string GetLabel(ThingDef def)
        {
            var sb = new StringBuilder();
            if (def != null)
            {
                sb.Append(def.label);
            }
            else
            {
                if (pipeNetDef != null)
                {
                    sb.Append(pipeNetDef.label);
                }
                if (thingCategoryDef != null)
                {
                    if (sb.Length > 0)
                        sb.Append(" ");
                    sb.Append(thingCategoryDef.label);
                }
            }
            return $"({sb.ToString().Trim()}): {count}/{wantedCount}";
        }
    }
}