using RimWorld.BaseGen;

namespace KCSG
{
    internal class SymbolResolver_RunResolvers : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (GenOption.ext.SymbolResolvers?.Count > 0)
            {
                for (int i = 0; i < GenOption.ext.symbolResolvers.Count; i++)
                {
                    BaseGen.symbolStack.Push(GenOption.ext.symbolResolvers[i], rp, null);
                }
            }
        }
    }
}
