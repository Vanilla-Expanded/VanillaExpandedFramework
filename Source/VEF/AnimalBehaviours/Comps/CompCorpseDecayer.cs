using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.Sound;
using Verse;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class CompCorpseDecayer : ThingComp
    {

        public bool flagOnce = false;
        public bool decayingOn = true;

        public CompProperties_CorpseDecayer Props
        {
            get
            {
                return (CompProperties_CorpseDecayer)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.decayingOn, "decayingOn", true);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            Pawn pawn = parent as Pawn;
            if(ModsConfig.OdysseyActive && pawn.training.HasLearned(InternalDefOf.VEF_ControlledCorpseDecay))
            {
                if (decayingOn)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            decayingOn=false;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_DisableCorpseDecayingDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_CorpseDecay"),
                        defaultLabel = "VEF_DisableCorpseDecaying".Translate()
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            decayingOn = true;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_EnableCorpseDecayingDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_CorpseDecay"),
                        defaultLabel = "VEF_EnableCorpseDecaying".Translate()
                    };
                }
            }
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            if (AnimalBehaviours_Settings.flagCorpseDecayingEffect)
            {
                //Only check every 2 rare ticks (8 seconds)
                if (parent.IsHashIntervalTick(Props.tickInterval, delta))
                {
                    Pawn pawn = this.parent as Pawn;
                    if (decayingOn) {
                        

                        //Null map check
                        if (pawn.Map != null)
                        {
                            //Check on radius
                            CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                            rect = rect.ExpandedBy(Props.radius);

                            foreach (IntVec3 current in rect.Cells)
                            {
                                if (current.InBounds(pawn.Map))
                                {
                                    HashSet<Thing> hashSet = new HashSet<Thing>(current.GetThingList(pawn.Map));
                                    foreach (Thing thingInCell in hashSet)
                                    {
                                        //If anything in those cells was a corpse
                                        if (thingInCell is Corpse corpse)
                                        {
                                            //A FLESHY corpse, no mechanoid munching
                                            if (corpse.InnerPawn.def.race.IsFlesh)
                                            {
                                                //Damage the corpse, and feed the animal
                                                corpse.HitPoints -= 5;
                                                if (pawn?.needs?.food != null)
                                                {
                                                    pawn.needs.food.CurLevel += Props.nutritionGained;

                                                }

                                                //This is for achievements
                                                if (ModLister.HasActiveModWithName("Alpha Animals") && (pawn.Faction == Faction.OfPlayer) && (corpse.InnerPawn.def.race.Humanlike))
                                                {
                                                    pawn.health.AddHediff(HediffDef.Named("AA_CorpseFeast"));
                                                }

                                                //If the corpse can rot, do it
                                                CompRottable compRottable = corpse.TryGetComp<CompRottable>();
                                                if (compRottable.Stage == RotStage.Fresh)
                                                {
                                                    compRottable.RotProgress += 100000;
                                                }
                                                //If the corpse reaches 0 HP, destroy it, and spawn corpse bile
                                                if (corpse.HitPoints < 0)
                                                {
                                                    corpse.Destroy();
                                                    for (int i = 0; i < 20; i++)
                                                    {
                                                        CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out var c);
                                                        FilthMaker.TryMakeFilth(c, pawn.Map, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite());
                                                        SoundDef.Named(Props.corpseSound).PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                                                    }
                                                }
                                                FilthMaker.TryMakeFilth(current, pawn.Map, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite());
                                                flagOnce = true;
                                            }
                                        }
                                    }
                                }
                                if (flagOnce) { flagOnce = false; break; }
                            }
                        }
                    }else if (!ModsConfig.OdysseyActive && !pawn.training.HasLearned(InternalDefOf.VEF_ControlledCorpseDecay))
                    {
                        decayingOn = true;
                    }
                }
            }
        }
    }
}
