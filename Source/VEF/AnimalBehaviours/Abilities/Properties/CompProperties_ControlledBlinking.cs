using RimWorld;
using Verse;
namespace VEF.AnimalBehaviours
{
    public class CompProperties_ControlledBlinking : CompProperties_AbilityEffect
    {

        public bool warpEffect = true;

        public CompProperties_ControlledBlinking()
        {
            compClass = typeof(CompAbilityEffect_ControlledBlinking);
        }
    }
}