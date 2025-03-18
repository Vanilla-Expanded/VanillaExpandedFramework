
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;



namespace VFECore
{
    public class CompProperties_LabelByIngredients : CompProperties
    {
        public bool fullReplace = false;
        public Dictionary<ThingDef, string> overrides = new Dictionary<ThingDef, string>();
        public List<ThingDef> exclusions = new List<ThingDef>();

        public CompProperties_LabelByIngredients()
        {
            compClass = typeof(CompLabelByIngredients);
        }
    }

    class CompLabelByIngredients : ThingComp
    {
        CompIngredients ingredients = null;
        string cachedLabel = "";

        public CompProperties_LabelByIngredients Props
        {
            get
            {
                return (CompProperties_LabelByIngredients)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (ingredients == null)
            {
                ingredients = this.parent.TryGetComp<CompIngredients>();
            }

        }

        public override string TransformLabel(string label)
        {
            if (cachedLabel == "")
            {
                if (ingredients != null && !ingredients.ingredients.NullOrEmpty())
                {
                    if (!Props.overrides.NullOrEmpty())
                    {
                        List<ThingDef> possibleIngredients = ingredients.ingredients.Where(x => Props.exclusions?.Contains(x) != true).ToList();
                        if(possibleIngredients.Count > 0)
                        {
                            ThingDef thingDef = possibleIngredients.First();
                            if (thingDef != null && Props.overrides.ContainsKey(thingDef))
                            {
                                if (Props.fullReplace)
                                {
                                    cachedLabel = Props.overrides[thingDef];
                                }
                                else
                                {
                                    cachedLabel = Props.overrides[thingDef] + " " + label;
                                }

                            }
                            else
                            {
                                cachedLabel = ingredients.ingredients.Where(x => Props.exclusions?.Contains(x) != true).First().LabelCap + " " + label;
                            }
                        }
                        
                        

                    }
                    else
                    {
                        cachedLabel = ingredients.ingredients.Where(x => Props.exclusions?.Contains(x) != true).First().LabelCap + " " + label;
                    }
                }


            }

            return cachedLabel == "" ? label : cachedLabel;

        }

    }
}

