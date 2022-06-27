using RimWorld.BaseGen;

namespace KCSG
{
    public class SymbolResolver_SettlementPower : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            SettlementGenUtils.PowerNetManagement.ManagePower(BaseGen.globalSettings.map);
        }
    }
}
