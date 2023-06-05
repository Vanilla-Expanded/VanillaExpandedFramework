using Verse;

namespace KCSG
{
    internal class GenStep_WorldObject : RimWorld.GenStep_Settlement
    {
        public override int SeedPart => 1969308471;

        protected override bool CanScatterAt(IntVec3 loc, Map map) => true;

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            map.Parent.def.GetModExtension<CustomGenOption>().Generate(loc, map);
        }
    }
}