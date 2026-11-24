using Verse;
using Waylith.Injection;


namespace VEF.AnimalGenes
{
    public static class InjectedFields
    {
        extension(Thing t)
        {
            [InjectedField]
            public CompAnimalGenes AnimalGenes
            {
                get => throw new System.InvalidOperationException("not injected");
                set => throw new System.InvalidOperationException("not injected");
            }
        }
    }
}