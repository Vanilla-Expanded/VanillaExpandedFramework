﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Buildings
{
    public class StudiableBuildingDetails : DefModExtension
    {


        public ThingDef buildingLeft = null;
        public SoundDef deconstructSound = null;
        public string gizmoTexture;
        public string gizmoText;
        public string gizmoDesc;
        public bool craftingInspiration;
        public SkillDef skillForStudying;
        public string overlayTexture;
        public bool showProgressBar = false;
        public bool showResearchEffecter = true;

    }
}