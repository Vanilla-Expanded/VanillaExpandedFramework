
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_DieAndChangeIntoOtherDef : CompProperties
    {

        //A comp that displays a gizmo if the animal is tamed. When clicked, it destroys the animal and turns it into a different def

        public bool needsDiggableTerrain = false;
        public bool mustBeTamed = true;
        public ThingDef defToChangeTo;
        public string gizmoImage;
        public string gizmoLabel;
        public string gizmoDesc;


        public CompProperties_DieAndChangeIntoOtherDef()
        {
            this.compClass = typeof(CompDieAndChangeIntoOtherDef);
        }


    }
}