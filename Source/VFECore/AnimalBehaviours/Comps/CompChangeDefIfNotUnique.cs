using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompChangeDefIfNotUnique : ThingComp
    {

        bool flag = false;


        public CompProperties_ChangeDefIfNotUnique Props
        {
            get
            {
                return (CompProperties_ChangeDefIfNotUnique)this.props;
            }
        }

        public override void PostExposeData()
        {
          
            Scribe_Values.Look<bool>(ref this.flag, "flag", false);

        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            base.PostSpawnSetup(respawningAfterLoad);
            IReadOnlyList<Pawn> pawnList = parent.Map.mapPawns.AllPawnsSpawned;
            
            foreach(Pawn pawn in pawnList)
            {
                if (pawn.def.defName == this.parent.def.defName)
                {
                    flag = true;
                }
            }
            
            
        }

        public override void CompTick()
        {
            base.CompTick();

            if (flag)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.defToChangeTo));
                Pawn pawnToCreate = PawnGenerator.GeneratePawn(request);
                GenSpawn.Spawn(pawnToCreate, this.parent.Position, parent.Map, WipeMode.Vanish);
                this.parent.Destroy();
            }
        }






    }
}