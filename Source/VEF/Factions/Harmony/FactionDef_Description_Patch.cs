using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Factions;

[HarmonyPatch(typeof(FactionDef))]
[HarmonyPatch(nameof(FactionDef.Description), MethodType.Getter)]
public static class FactionDef_Description_Patch
{
    public static Lazy<FieldInfo> cachedDescription = new Lazy<FieldInfo>(() => AccessTools.Field(typeof(FactionDef), "cachedDescription"));
    
    public static void Prefix(FactionDef __instance, out bool __state)
    {
        var desc = cachedDescription.Value.GetValue(__instance);
        __state = desc == null; 
    }

    public static void Postfix(FactionDef __instance, bool __state, ref string __result)
    {
        if (!__state) return;

        HashSet<Def> positives = [];
        HashSet<Def> negatives = [];

        foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs.Where(cb =>
                     cb.factions.Contains(__instance)))
        {
            if (contrabandDef.impactMultiplier > 0)
            {
                positives.AddRange(contrabandDef.AllContraband());
            }
            else
            {
                negatives.AddRange(contrabandDef.AllContraband());
            }
        }

        StringBuilder sb = new StringBuilder(__result);

        if (!positives.NullOrEmpty() || !negatives.NullOrEmpty())
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(("VEF.Factions.FactionDef_Description_Contraband".Translate() + ":").AsTipTitle());
        }
        if (positives.Count > 0)
        {
            sb.AppendLine("VEF.Factions.FactionDef_Description_Positives"
                .Translate(positives.Select(p => p.LabelCap)
                    .Aggregate((a, b) => a + " ," + b)));
        }

        if (negatives.Count > 0)
        {
            sb.AppendLine("VEF.Factions.FactionDef_Description_Negatives"
                .Translate(negatives.Select(p=>p.LabelCap)
                    .Aggregate((a,b)=> a + " ," + b)));
        }
        
        __result = sb.ToString();
        cachedDescription.Value.SetValue(__instance, __result);
    }
}