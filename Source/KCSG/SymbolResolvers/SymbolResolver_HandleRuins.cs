using RimWorld.BaseGen;

namespace KCSG
{
    internal class SymbolResolver_HandleRuins : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            GenOption.currentGenStep = "Ruining structure";

            if (GenOption.ext.shouldRuin)
            {
                for (int i = 0; i < GenOption.ext.ruinSymbolResolvers.Count; i++)
                {
                    string resolver = GenOption.ext.ruinSymbolResolvers[i];
                    if (!(GenOption.ext.ruinSymbolResolvers.Contains("kcsg_randomroofremoval") && resolver == "kcsg_scatterstuffaround"))
                        BaseGen.symbolStack.Push(resolver, rp, null);
                }
            }
        }
    }
}
