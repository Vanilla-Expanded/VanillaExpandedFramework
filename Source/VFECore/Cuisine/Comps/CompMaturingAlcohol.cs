using System;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaCookingExpanded
{


    //This code is mostly copied from CompProperties_Rotable, with a few tweaks. Instead of destroying an item when
    //it rots, this code will swap the item for a different one. For example, it is used with Vanilla Brewing Expanded Ambrosia mush 
    //to turn it into Ambrandy must

    public class CompMaturingAlcohol : ThingComp
    {
        public CompProperties_MaturingAlcohol PropsRot
        {
            get
            {
                return (CompProperties_MaturingAlcohol)this.props;
            }
        }

        public float RotProgressPct
        {
            get
            {
                return this.RotProgress / (float)this.PropsRot.TicksToRotStart;
            }
        }

        public float RotProgress
        {
            get
            {
                return this.rotProgressInt;
            }
            set
            {
                RotStage stage = this.Stage;
                this.rotProgressInt = value;
                if (stage != this.Stage)
                {
                    this.StageChanged();
                }
            }
        }

        public RotStage Stage
        {
            get
            {
                if (this.RotProgress < (float)this.PropsRot.TicksToRotStart)
                {
                    return RotStage.Fresh;
                }
                if (this.RotProgress < (float)this.PropsRot.TicksToDessicated)
                {
                    return RotStage.Rotting;
                }
                return RotStage.Dessicated;
            }
        }

        public int TicksUntilRotAtCurrentTemp
        {
            get
            {
                float num = this.parent.AmbientTemperature;
                num = (float)Mathf.RoundToInt(num);
                return this.TicksUntilRotAtTemp(num);
            }
        }

        public bool Active
        {
            get
            {
                if (this.PropsRot.disableIfHatcher)
                {
                    CompHatcher compHatcher = this.parent.TryGetComp<CompHatcher>();
                    if (compHatcher != null && !compHatcher.TemperatureDamaged)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.rotProgressInt, "rotProg", 0f, false);
        }

        public override void CompTick()
        {
            this.Tick(1);
        }

        public override void CompTickRare()
        {
            this.Tick(250);
        }

        private void Tick(int interval)
        {
            if (!this.Active)
            {
                return;
            }
            float rotProgress = this.RotProgress;
            float num = GenTemperature.RotRateAtTemperature(this.parent.AmbientTemperature);
            this.RotProgress += num * (float)interval;
            if (this.Stage == RotStage.Rotting && this.PropsRot.rotDestroys)
            {
                //The first main change from the rotting code is here
                //If the item reaches it's rotting time, a new item with the same stackCount is spawned on its location, and this item is destroyed

                //The map check is done... just in case

                if (this.parent.Map != null)
                {
                    int stackCount = this.parent.stackCount;
                    ThingDef newThingDef = ThingDef.Named(PropsRot.thingToTransformTo);
                    Thing newThing = GenSpawn.Spawn(newThingDef, this.parent.Position, this.parent.Map, WipeMode.Vanish);
                    newThing.stackCount = stackCount;
                    this.parent.Destroy(DestroyMode.Vanish);
                }
                else this.parent.Destroy(DestroyMode.Vanish);




                return;
            }
            if (Mathf.FloorToInt(rotProgress / 60000f) != Mathf.FloorToInt(this.RotProgress / 60000f) && this.ShouldTakeRotDamage())
            {
                if (this.Stage == RotStage.Rotting && this.PropsRot.rotDamagePerDay > 0f)
                {
                    this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, (float)GenMath.RoundRandom(this.PropsRot.rotDamagePerDay), 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    return;
                }
                if (this.Stage == RotStage.Dessicated && this.PropsRot.dessicatedDamagePerDay > 0f)
                {
                    this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, (float)GenMath.RoundRandom(this.PropsRot.dessicatedDamagePerDay), 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                }
            }
        }

        private bool ShouldTakeRotDamage()
        {
            Thing thing = this.parent.ParentHolder as Thing;
            return thing == null || thing.def.category != ThingCategory.Building || !thing.def.building.preventDeteriorationInside;
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(this.parent.stackCount + count);
            float rotProgress = ((ThingWithComps)otherStack).GetComp<CompMaturingAlcohol>().RotProgress;
            this.RotProgress = Mathf.Lerp(this.RotProgress, rotProgress, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CompMaturingAlcohol>().RotProgress = this.RotProgress;
        }

        public override void PostIngested(Pawn ingester)
        {
            if (this.Stage != RotStage.Fresh && FoodUtility.GetFoodPoisonChanceFactor(ingester) > 1.401298E-45f)
            {
                FoodUtility.AddFoodPoisoningHediff(ingester, this.parent, FoodPoisonCause.Rotten);
            }
        }

        public override string CompInspectStringExtra()
        {
            //The other changes are made here, to display custom strings regarding how well the item is "rotting" into the new one. No default, so they NEED
            //to be defined via XML

            if (!this.Active)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            switch (this.Stage)
            {
                case RotStage.Fresh:
                    stringBuilder.Append(PropsRot.maturingString.Translate() + ".");
                    break;


            }
            if ((float)this.PropsRot.TicksToRotStart - this.RotProgress > 0f)
            {
                float num = GenTemperature.RotRateAtTemperature((float)Mathf.RoundToInt(this.parent.AmbientTemperature));
                int ticksUntilRotAtCurrentTemp = this.TicksUntilRotAtCurrentTemp;
                stringBuilder.AppendLine();
                if (num < 0.001f)
                {
                    stringBuilder.Append(PropsRot.maturingStopped.Translate() + ".");
                }
                else if (num < 0.999f)
                {
                    stringBuilder.Append(PropsRot.maturingSlowly.Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriod(true, false, true, true)) + ".");
                }
                else
                {
                    stringBuilder.Append(PropsRot.maturingProperly.Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriod(true, false, true, true)) + ".");
                }
            }
            return stringBuilder.ToString();
        }

        public int ApproxTicksUntilRotWhenAtTempOfTile(int tile, int ticksAbs)
        {
            float temperatureFromSeasonAtTile = GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs, tile);
            return this.TicksUntilRotAtTemp(temperatureFromSeasonAtTile);
        }

        public int TicksUntilRotAtTemp(float temp)
        {
            if (!this.Active)
            {
                return 72000000;
            }
            float num = GenTemperature.RotRateAtTemperature(temp);
            if (num <= 0f)
            {
                return 72000000;
            }
            float num2 = (float)this.PropsRot.TicksToRotStart - this.RotProgress;
            if (num2 <= 0f)
            {
                return 0;
            }
            return Mathf.RoundToInt(num2 / num);
        }

        private void StageChanged()
        {
            Corpse corpse = this.parent as Corpse;
            if (corpse != null)
            {
                corpse.RotStageChanged();
            }
        }

        public void RotImmediately()
        {
            if (this.RotProgress < (float)this.PropsRot.TicksToRotStart)
            {
                this.RotProgress = (float)this.PropsRot.TicksToRotStart;
            }
        }

        private float rotProgressInt;
    }
}