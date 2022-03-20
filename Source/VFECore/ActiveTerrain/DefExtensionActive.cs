using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    using Verse;

    public abstract class DefExtensionActive : DefModExtension
    {
        public abstract void DoWork(TerrainDef def);
        public abstract void DoWork(ThingDef def);
    }

    public class DefExtension_ShaderSpeedMult : DefExtensionActive
    {
        private float timeMult = 1;

        public override void DoWork(TerrainDef def) => 
            def.waterDepthMaterial.SetFloat("_GameSeconds", Find.TickManager.TicksGame * this.timeMult);

        public override void DoWork(ThingDef def) => throw new NotImplementedException();
    }
}
