using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.Sound;

namespace AnimalBehaviours
{
    public class Pawn_SwallowWhole : Pawn, IThingHolder
    {

        public ThingOwner innerContainer = null;
        protected bool contentsKnown;
        public int tickCounter = 0;
       
        CompSwallowWhole comp = null;


        public CompSwallowWhole Comp
        {
            get
            {
                if (this.comp != null)
                {
                    return this.comp;
                }
                this.comp = this.TryGetComp<CompSwallowWhole>();
                return this.comp;


            }
        }


        public Pawn_SwallowWhole()
        {
            //Constructor initializes the pawn container and the comp
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            comp = Comp;

        }
        public override void ExposeData()
        {
            //Save all the key variables so they work on game save / load
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[] { this });
        }
        public new ThingOwner GetDirectlyHeldThings()
        {
            //Not used, included just in case something external calls it           
            return this.innerContainer;
        }

        public new void GetChildHolders(List<IThingHolder> outChildren)
        {
            //Not used, included just in case something external calls it
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public virtual void EjectContents()
        {
            //Remove ingredients from the pawn container. 
            if (this.Map != null)
            {
                this.innerContainer.TryDropAll(this.Position, base.Map, ThingPlaceMode.Near, null, null);
            }
        }

        public void DestroyContents()
        {
            //Empties all containers and destroys contents

            if (this.innerContainer != null && this.innerContainer.Any)
            {
                this.innerContainer.ClearAndDestroyContents();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {

            if (this.Map != null && Comp.Props.createFilthWhenKilled)
            {
                EjectContents();
                for (int i = 0; i < 20; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableNearbyCell(this.Position, this.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                    FilthMaker.TryMakeFilth(c, this.Map, Comp.Props.filthToMake);

                }
                if (Comp.Props.playSoundWhenKilled) {
                    SoundDef.Named(Comp.Props.soundToPlay).PlayOneShot(new TargetInfo(this.Position, this.Map, false));

                }
            }

            base.Destroy(mode);
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (this.Map != null && Comp.Props.createFilthWhenKilled)
            {
                EjectContents();
                for (int i = 0; i < 20; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableNearbyCell(this.Position, this.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                    FilthMaker.TryMakeFilth(c, this.Map, Comp.Props.filthToMake);

                }
                if (Comp.Props.playSoundWhenKilled)
                {
                    SoundDef.Named(Comp.Props.soundToPlay).PlayOneShot(new TargetInfo(this.Position, this.Map, false));

                }
            }

            base.Kill(dinfo, exactCulprit);
        }

        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.Remove(thing);
                this.innerContainer.TryAdd(thing, thing.stackCount, false);
                flag = true;
            }
            else
            {
                flag = this.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }

        public override void TickRare()
        {
            base.TickRare();
            if (innerContainer.Count >= Comp.Props.stomachCapacity)
            {
                tickCounter++;
                if (tickCounter > Comp.Props.digestionPeriod)
                {
                    foreach (Thing thing in innerContainer)
                    {
                        Pawn pawnSwallowed = thing as Pawn;
                        if (pawnSwallowed != null)
                        {

                            if (!pawnSwallowed.Dead) { pawnSwallowed.Kill(null); }
                            CompRottable compRottable = pawnSwallowed.Corpse.TryGetComp<CompRottable>();


                            if (compRottable != null && compRottable.Stage == RotStage.Fresh)
                            {

                                compRottable.RotProgress += 100000000;
                            }
                        }

                    }
                    EjectContents();
                    tickCounter = 0;
                }
            }


        }

        public override string GetInspectString()
        {
            string stomachContents = "";

            if (innerContainer.Count >= Comp.Props.stomachCapacity)
            {
                stomachContents += "\n" + "VEF_StomachContents".Translate(innerContainer.Count) + "VEF_DigestionTime".Translate(((Comp.Props.digestionPeriod - tickCounter) * 250).ToStringTicksToPeriod(true, false, true, true));
            }
            else stomachContents += "\n" + "VEF_StomachContents".Translate(innerContainer.Count);

            return base.GetInspectString() + stomachContents;
        }




    }
}
