using System;
using Verse;

namespace VFECore
{
    public class MovingBaseDestinationAction : IExposable
    {
        public MovingBase destination;
        public Type arrivalActionType;

        public void ExposeData()
        {
            Scribe_References.Look(ref destination, "movingBaseDestination");
            Scribe_Values.Look(ref arrivalActionType, "arrivalActionType");
        }
    }
}