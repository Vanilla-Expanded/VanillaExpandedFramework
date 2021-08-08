using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class CompTerrainChanger : ThingComp
    {

        public int extraFertCounter = 5;

        public CompProperties_TerrainChanger Props
        {
            get
            {
                return (CompProperties_TerrainChanger)this.props;
            }
        }

        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(Props.checkingRate)) {
                Pawn pawn = this.parent as Pawn;

                if (pawn.Spawned)
                {
                    if (pawn.Faction != null && pawn.Faction.IsPlayer)
                    {
                        IntVec3 cell;
                        if (Props.inRadius)
                        {
                            cell = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, Props.radius, null);
                        }
                        else
                        {
                            cell = pawn.Position;
                        }

                        if ((cell.GetTerrain(pawn.Map) == TerrainDef.Named(Props.FirstStageTerrain)))
                        {
                            pawn.Map.terrainGrid.SetTerrain(cell, TerrainDef.Named(Props.SecondStageTerrain));

                            //This is for achievements

                            if (ModLister.HasActiveModWithName("Alpha Animals"))
                            {
                                pawn.health.AddHediff(HediffDef.Named("AA_FertilizedTerrain"));
                            }
                        }
                        if (Props.doThirdStage)
                        {
                            extraFertCounter--;
                            if (extraFertCounter <= 0)
                            {
                                if (pawn.training.HasLearned(TrainableDefOf.Obedience) && ((cell.GetTerrain(pawn.Map) == TerrainDef.Named(Props.SecondStageTerrain))))
                                {
                                    pawn.Map.terrainGrid.SetTerrain(cell, TerrainDef.Named(Props.ThirdStageTerrain));
                                    //This is for achievements

                                    if (ModLister.HasActiveModWithName("Alpha Animals"))
                                    {
                                        pawn.health.AddHediff(HediffDef.Named("AA_FertilizedTerrain"));
                                    }
                                }
                                extraFertCounter = 5;
                            }
                        }



                    }
                }

            }
            
            
        }
    }
}

