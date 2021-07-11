using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class HediffComp_AutoPermanentInjury : HediffComp
    {

        public override void CompPostMake()
        {
            HediffComp_GetsPermanent permanentcomp = this.parent.TryGetComp<HediffComp_GetsPermanent>();
            permanentcomp.IsPermanent = true;

        }

    }
}
