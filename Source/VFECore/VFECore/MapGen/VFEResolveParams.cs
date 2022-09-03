using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace VFECore
{

    public class VFEResolveParams
    {

        public const string Name = "VFEMResolveParams";

        public ThingDef edgeWallDef;
        public float? towerRadius;
        public bool? symmetricalSandbags;
        public bool? hasDoors;
        public bool? outdoorLighting;
        public bool? generatePawns;

    }

}
