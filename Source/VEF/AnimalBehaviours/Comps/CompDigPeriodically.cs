using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using VEF;

namespace VEF.AnimalBehaviours
{
    public class CompDigPeriodically : ThingComp
    {
        public int diggingCounter = 0;
        private Effecter effecter;
        public bool diggingOn = true;

        public CompProperties_DigPeriodically Props
        {
            get
            {
                return (CompProperties_DigPeriodically)this.props;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            Pawn pawn = parent as Pawn;
            if (ModsConfig.OdysseyActive && pawn.training.HasLearned(InternalDefOf.VEF_DiggingDiscipline))
            {
                if (diggingOn)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            diggingOn = false;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_DisableDiggingDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_DiggingDiscipline", true),
                        defaultLabel = "VEF_DisableDigging".Translate()
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            diggingOn = true;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_EnableDiggingDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_DiggingDiscipline", true),
                        defaultLabel = "VEF_EnableDigging".Translate()
                    };
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.diggingOn, "diggingOn", true, false);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (AnimalBehaviours_Settings.flagDigPeriodically) {
                diggingCounter++;
                //Only do every ticksToDig
                if (diggingCounter > Props.ticksToDig)
                {
                    Pawn pawn = this.parent as Pawn;
                    if (diggingOn)
                    {
                        //Null map check. Also check that the animal isn't sleeping, downed or dead, and if onlyWhenTamed is true, that the animal is tamed
                        if ((pawn.Map != null) && pawn.Awake() && !pawn.Downed && !pawn.Dead && (!Props.onlyWhenTamed || (Props.onlyWhenTamed && pawn.Faction != null && pawn.Faction.IsPlayer)))
                        {

                            if (!Props.onlyDigIfPolluted || (Props.onlyDigIfPolluted && pawn.Position.IsPolluted(pawn.Map)))
                            {
                                if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(VEFDefOf.Diggable))
                                {
                                    string thingToDig = "";
                                    int amount = 1;
                                    ThingDef newThing = null;
                                    if (!Props.digBiomeRocks)
                                    {
                                        //This could have been done with a Dictionary
                                        thingToDig = this.Props.customThingToDig.RandomElement();
                                        int index = Props.customThingToDig.IndexOf(thingToDig);
                                        amount = Props.customAmountToDig[index];
                                        newThing = ThingDef.Named(thingToDig);
                                    }
                                    else
                                    {
                                        amount = Props.customAmountToDigIfRocksOrBricks;
                                        IEnumerable<ThingDef> rocksInThisBiome = Find.World.NaturalRockTypesIn(this.parent.Map.Tile);
                                        List<ThingDef> chunksInThisBiome = new List<ThingDef>();
                                        foreach (ThingDef rock in rocksInThisBiome)
                                        {
                                            chunksInThisBiome.Add(rock.building.mineableThing);
                                        }
                                        if (!Props.digBiomeBricks)
                                        {
                                            newThing = Find.World.NaturalRockTypesIn(this.parent.Map.Tile).RandomElementWithFallback().building.mineableThing;

                                        }
                                        else
                                        {
                                            newThing = Find.World.NaturalRockTypesIn(this.parent.Map.Tile).RandomElementWithFallback().building.mineableThing.butcherProducts.FirstOrFallback().thingDef;

                                        }

                                    }
                                    Thing newDugThing;
                                    if (Props.resultIsCorpse)
                                    {
                                        PawnKindDef pawnkind = PawnKindDef.Named(thingToDig);
                                        newDugThing = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkind));
                                        newDugThing.Kill();
                                        IntVec3 near = CellFinder.StandableCellNear(this.parent.Position, this.parent.Map, 1f);
                                        Thing spawnedPawn = GenSpawn.Spawn(newDugThing, near, this.parent.Map, WipeMode.Vanish);

                                    }
                                    else
                                    {
                                        newDugThing = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                        newDugThing.stackCount = amount;
                                    }


                                    if (Props.spawnForbidden)
                                    {
                                        newDugThing.SetForbidden(true, false);
                                    }
                                    if (this.effecter == null)
                                    {
                                        this.effecter = EffecterDefOf.Mine.Spawn();
                                    }
                                    this.effecter.Trigger(pawn, newDugThing);
                                }
                            }



                        }
                    }
                    else if (!ModsConfig.OdysseyActive || !pawn.training.HasLearned(InternalDefOf.VEF_DiggingDiscipline))
                    {
                        diggingOn = true;
                    }



                    diggingCounter = 0;
                }


            }

            
        }
    }
}
