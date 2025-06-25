using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    [DefOf]
    public static class InternalDefOf
    {

        public static JobDef VEF_AnimalResource;
        public static JobDef VEF_DestroyItem;
        public static JobDef VEF_LayExplodingEgg;
        public static JobDef VEF_IngestWeird;
        public static JobDef VEF_FleeAndCowerShort;

        public static HediffDef VEF_LightSustenance;

        public static DamageDef VEF_SecondaryAcidBurn;

        public static ThingDef Gun_Autopistol;

        [MayRequireOdyssey]
        public static TrainableDef VEF_ControlledBlinking;
        [MayRequireOdyssey]
        public static TrainableDef VEF_CycleSeverance;
        [MayRequireOdyssey]
        public static TrainableDef VEF_Beastmastery;
        [MayRequireOdyssey]
        public static TrainableDef VEF_ControlledCorpseDecay;
        [MayRequireOdyssey]
        public static TrainableDef VEF_DiggingDiscipline;
        [MayRequireOdyssey]
        public static TrainableDef VEF_FumeRegulation;
    }
}
