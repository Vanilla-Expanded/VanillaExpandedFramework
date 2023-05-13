using RimWorld.BaseGen;

namespace KCSG
{
    internal class SymbolResolver_RunResolvers : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (GenOption.customGenExt.symbolResolvers?.Count > 0)
            {
                for (int i = 0; i < GenOption.customGenExt.symbolResolvers.Count; i++)
                {
                    BaseGen.symbolStack.Push(GenOption.customGenExt.symbolResolvers[i], rp, null);
                }
            }
        }
    }
}
