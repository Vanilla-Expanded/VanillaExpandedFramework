﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class AnimalsDisabledFromQuestsDef : Def
    {
        //A list of pawnkind defNames
        public List<PawnKindDef> disabledFromQuestsPawns;
    }
}