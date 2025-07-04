﻿using Verse;

namespace VEF.Weapons
{
    public class DamageExtension : DefModExtension
    {
        public FloatRange pushBackDistance;
        public SoundDef soundOnDamage;
        public FleckDef fleckOnDamage;
        public float fleckRadius;
        public bool fleckOnInstigator;
    }
}
