using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VFECore
{
    public class CompPawnDependsOn : ThingComp
    {
        public Pawn myPawn;

        public CompProperties_PawnDependsOn Props
        {
            get
            {
                return (CompProperties_PawnDependsOn)this.props;
            }
        }
        public bool MyPawnIsAlive => this.myPawn != null && !this.myPawn.Destroyed && !this.myPawn.Dead;

        public virtual void SpawnMyPawn()
        {
            if (!MyPawnIsAlive)
            {
                myPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Props.pawnToSpawn, parent.Faction, canGeneratePawnRelations: false));
                myPawn.Position = parent.Position;
                myPawn.Rotation = Rot4.South;
                CompDependsOnBuilding pawnComp = myPawn.TryGetComp<CompDependsOnBuilding>();
                if (pawnComp == null)
                    Log.Error("CompPawnDependsOn spawned a pawn without CompDependsOnBuilding! This should never happen.");
                else
                    pawnComp.myBuilding = (Building)this.parent;
                myPawn.SpawnSetup(parent.Map, false);
            }
        }

        public virtual void OnPawnDestroyed()
        {
            //Do something - spawn a new pawn, send an alert, or maybe just explode
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (myPawn != null)
            {
                var comp = myPawn.TryGetComp<CompDependsOnBuilding>();
                comp.OnBuildingDestroyed(this);
                comp.myBuilding = null;
                myPawn = null;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            gizmos.AddRange(base.CompGetGizmosExtra());
            if (DebugSettings.ShowDevGizmos && this.Props.pawnToSpawn != null)
            {
                Command_Action debugForceSpawn = new Command_Action
                {
                    action = delegate { SpawnMyPawn(); },
                    defaultLabel = "Dev: Spawn pawn",
                    defaultDesc = "Spawn this building's pawn if none currently exists"
                };
                gizmos.Add(debugForceSpawn);
            }
            return gizmos;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Pawn>(ref myPawn, "myPawn");
        }
    }
}