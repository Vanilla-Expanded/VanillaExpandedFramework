using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Cooking
{
    public class CompTempTransforms : ThingComp
    {

        //A comp class to make an item transform into a different one if a certain temperature is reached

        //It is used for example in Vanilla Cooking Expanded's grills, that turn into "ruined" versions of
        //themselves if frozen, or in vanilla Brewing Expanded's Hot coffee, that turns into Iced coffee
        //when frozen

        //The code here is mostly a copy of vanilla's CompTemperatureRuinable

        public CompProperties_TempTransforms Props
        {
            get
            {
                return (CompProperties_TempTransforms)this.props;
            }
        }

        public bool Ruined
        {
            get
            {
                return this.ruinedPercent >= 1f;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref this.ruinedPercent, "ruinedPercent", 0f, false);
        }

        public void Reset()
        {
            this.ruinedPercent = 0f;
        }

        public override void CompTickInterval(int delta)
        {
            this.DoTicks(delta);
        }

        public override void CompTickRare()
        {
            this.DoTicks(GenTicks.TickRareInterval);
        }

        public override void CompTickLong()
        {
            this.DoTicks(GenTicks.TickLongInterval);
        }

        private void DoTicks(int ticks)
        {
            if (!this.Ruined)
            {
                float ambientTemperature = this.parent.AmbientTemperature;
                if (ambientTemperature > this.Props.maxSafeTemperature)
                {
                    this.ruinedPercent += (ambientTemperature - this.Props.maxSafeTemperature) * this.Props.progressPerDegreePerTick * (float)ticks;
                }
                else if (ambientTemperature < this.Props.minSafeTemperature)
                {
                    this.ruinedPercent -= (ambientTemperature - this.Props.minSafeTemperature) * this.Props.progressPerDegreePerTick * (float)ticks;
                }
                if (this.ruinedPercent >= 1f)
                {
                    this.ruinedPercent = 1f;
                    this.parent.BroadcastCompSignal("RuinedByTemperature");
                    return;
                }
                if (this.ruinedPercent < 0f)
                {
                    this.ruinedPercent = 0f;
                }
            }
            else
            {
                if (this.parent.Map != null)
                {
                    //Here is the main change: when the temperature is reached the item is "Ruined", and
                    //this code will spawn a new item with the same stackCount, the same ingredients list
                    //(if any) and destroy the original item

                    Thing newItem = ThingMaker.MakeThing(ThingDef.Named(Props.thingToTransformInto));

                    newItem.stackCount = this.parent.stackCount;
                    // Keep the HP percentage
                    if (Props.preserveHp)
                        newItem.HitPoints = Mathf.Max(Mathf.CeilToInt((float)parent.HitPoints / parent.MaxHitPoints * newItem.MaxHitPoints), 1);

                    // Only do those if the new item can have comps
                    if (newItem is ThingWithComps newItemWithComps)
                    {
                        // Move over the ingredients to new item
                        if (parent.GetComp<CompIngredients>()?.ingredients is { Count: > 0 } ingredients)
                            newItemWithComps.TryGetComp<CompIngredients>()?.ingredients?.AddRange(ingredients);

                        // Set as forbidden if it was beforehand
                        if (Props.keepForbidden && newItemWithComps.compForbiddable != null && parent.compForbiddable != null)
                            newItemWithComps.compForbiddable.Forbidden = parent.compForbiddable.Forbidden;

                        // Keep rot progress percentage
                        if (Props.keepRottableProgress && newItemWithComps.GetComp<CompRottable>() is {} newRottable && parent.GetComp<CompRottable>() is {} oldRottable)
                            newRottable.RotProgress = oldRottable.RotProgressPct * newRottable.PropsRot.TicksToRotStart;

                        if (Props.keepQuality)
                        {
                            // Copy quality, don't init art
                            if (newItemWithComps.compQuality != null && parent.compQuality != null)
                                newItemWithComps.compQuality.SetQuality(parent.compQuality.Quality, null);

                            // Copy art/author/etc. if present
                            if (newItemWithComps.GetComp<CompArt>() is {} newArt && parent.GetComp<CompArt>() is {} oldArt)
                            {
                                NonPublicFields.CompArt_authorNameInt(newArt) = NonPublicFields.CompArt_authorNameInt(oldArt);
                                NonPublicFields.CompArt_titleInt(newArt) = NonPublicFields.CompArt_titleInt(oldArt);
                                NonPublicFields.CompArt_taleRef(newArt) = NonPublicFields.CompArt_taleRef(oldArt);
                            }
                        }
                    }


                    var wasSelected = Find.Selector.IsSelected(parent);
                    GenSpawn.Spawn(newItem, this.parent.Position, this.parent.Map);
                    this.parent.Destroy();
                    // Reselect if it was previously selected
                    if (wasSelected)
                        Find.Selector.Select(newItem);
                }


            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(this.parent.stackCount + count);
            CompTempTransforms comp = ((ThingWithComps)otherStack).GetComp<CompTempTransforms>();
            this.ruinedPercent = Mathf.Lerp(this.ruinedPercent, comp.ruinedPercent, t);
        }

        public override bool AllowStackWith(Thing other)
        {
            CompTempTransforms comp = ((ThingWithComps)other).GetComp<CompTempTransforms>();
            return this.Ruined == comp.Ruined;
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CompTempTransforms>().ruinedPercent = this.ruinedPercent;
        }

        public override string CompInspectStringExtra()
        {
            if (this.Ruined)
            {
                return "RuinedByTemperature".Translate();
            }
            if (this.ruinedPercent > 0f)
            {
                float ambientTemperature = this.parent.AmbientTemperature;
                string str;
                if (ambientTemperature > this.Props.maxSafeTemperature)
                {
                    str = "Overheating".Translate();
                }
                else
                {
                    if (ambientTemperature >= this.Props.minSafeTemperature)
                    {
                        return null;
                    }
                    str = "Freezing".Translate();
                }
                return str + ": " + this.ruinedPercent.ToStringPercent();
            }
            return null;
        }

        protected float ruinedPercent;

        public const string RuinedSignal = "RuinedByTemperature";
    }
}
