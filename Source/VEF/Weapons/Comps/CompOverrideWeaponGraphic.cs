using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    public class CompOverrideWeaponGraphic : ThingComp
    {

        WeaponTraitDefExtension contentDetails;
        CompUniqueWeapon cachedComp;


        public WeaponTraitDefExtension GetDetails()
        {
            //The first one that is found will be the one we will use

            if (contentDetails == null)
            {
                CompUniqueWeapon comp = GetComp();
                if (comp != null)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension?.graphicOverride != null)
                        {
                            contentDetails = extension;
                        }
                    }
                }

            }
            return contentDetails;
        }

        public CompUniqueWeapon GetComp()
        {
            if (cachedComp == null)
            {
                cachedComp = this.parent.GetComp<CompUniqueWeapon>();
            }
            return cachedComp;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (GetDetails()?.graphicOverride != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (GetDetails()?.graphicOverride != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(); });
            }
        }

        public void ChangeGraphic()
        {

            GraphicData data = GetDetails().graphicOverride;
            Shader shader = data.shaderType?.Shader ?? ShaderTypeDefOf.Cutout.Shader;
            Color color = GetComp().ForceColor() ?? Color.white;
            if (data.graphicClass == typeof(Graphic_Single))
            {
                Graphic_Single newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(data.texPath, shader, data.drawSize, color);
                ReflectionCache.weaponGraphic(parent) = newGraphicSingle;
            }
            else if (data.graphicClass == typeof(Graphic_Random))
            {
                Graphic_Random newGraphicSingle = (Graphic_Random)GraphicDatabase.Get<Graphic_Random>(data.texPath, shader, data.drawSize, color);
                ReflectionCache.weaponGraphic(parent) = newGraphicSingle;
            }
          



        }



    }
}
