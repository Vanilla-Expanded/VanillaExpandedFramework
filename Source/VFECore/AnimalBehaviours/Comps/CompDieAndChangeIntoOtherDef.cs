using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;
using VFECore;

namespace AnimalBehaviours
{


    public class CompDieAndChangeIntoOtherDef : ThingComp, PawnGizmoProvider
    {
        public CompProperties_DieAndChangeIntoOtherDef Props => base.props as CompProperties_DieAndChangeIntoOtherDef;


        public IEnumerable<Gizmo> GetGizmos()
        {


            if (!Props.mustBeTamed ||(Props.mustBeTamed &&this.parent.Faction?.IsPlayer==true)) {
                yield return new Command_Action
                {
                    defaultLabel = Props.gizmoLabel.Translate(),
                    defaultDesc = Props.gizmoDesc.Translate(),
                    icon = ContentFinder<Texture2D>.Get(Props.gizmoImage, true),
                    action = delegate
                    {
                        DiggableTerrainSetup();
                    }
                };
            }
                
            
        }

        public void DiggableTerrainSetup()
        {
            Pawn pawn = this.parent as Pawn;
            if (Props.needsDiggableTerrain)
            {
                if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(VFEDefOf.Diggable))
                {
                    ChangeDef(pawn);
                }else { 
                    Messages.Message("VEF_TerrainsNeedsDiggable".Translate().CapitalizeFirst(), new TargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.NegativeEvent); 
                }

            }else
            {
                ChangeDef(pawn);
            }
            
        }

        public void ChangeDef(Pawn pawn)
        {

            ThingDef newThing = Props.defToChangeTo;
            Thing newbuilding = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
            pawn.DeSpawn();
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);


        }

        }


}
