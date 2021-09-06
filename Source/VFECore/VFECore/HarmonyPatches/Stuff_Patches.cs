using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFECore
{
    [HarmonyPatch(typeof(ThingStuffPair), nameof(ThingStuffPair.Commonality), MethodType.Getter)]
    public static class Commonality_Patch
    {
        private static Dictionary<ThingDef, StuffExtension> cachedExtension = new Dictionary<ThingDef, StuffExtension>();
        public static StuffExtension GetCachedExtension(this ThingDef thingDef)
        {
            if (!cachedExtension.TryGetValue(thingDef, out var extension))
            {
                cachedExtension[thingDef] = extension = thingDef.GetModExtension<StuffExtension>();
            }
            return extension;
        }
        public static void Postfix(ThingStuffPair __instance, ref float __result)
        {
            if (__instance.stuff != null)
            {
                __result = ModifyCommonalityOf(__instance.thing, __instance.stuff, __result);
            }
        }

        public static float ModifyCommonalityOf(ThingDef thingDefFor, ThingDef stuff, float curCommonality)
        {
            var extension = stuff.GetCachedExtension();
            if (extension != null)
            {
                if (thingDefFor.IsApparel)
                {
                    if (extension.apparelGenerationCommonalityOffset.HasValue)
                    {
                        curCommonality += extension.apparelGenerationCommonalityOffset.Value;
                    }
                    if (extension.apparelGenerationCommonalityFactor.HasValue)
                    {
                        curCommonality *= extension.apparelGenerationCommonalityFactor.Value;
                    }
                }
                if (thingDefFor.IsWeapon)
                {
                    if (extension.weaponGenerationCommonalityOffset.HasValue)
                    {
                        curCommonality += extension.weaponGenerationCommonalityOffset.Value;
                    }
                    if (extension.weaponGenerationCommonalityFactor.HasValue)
                    {
                        curCommonality *= extension.weaponGenerationCommonalityFactor.Value;
                    }
                }
                if (thingDefFor.building != null)
                {
                    if (extension.structureGenerationCommonalityOffset.HasValue)
                    {
                        curCommonality += extension.structureGenerationCommonalityOffset.Value;
                    }
                    if (extension.structureGenerationCommonalityFactor.HasValue)
                    {
                        curCommonality *= extension.structureGenerationCommonalityFactor.Value;
                    }
                }
            }
            return curCommonality;
        }
    }

    [HarmonyPatch(typeof(GenStuff), nameof(GenStuff.RandomStuffInexpensiveFor), new Type[] { typeof(ThingDef), typeof(TechLevel), typeof( Predicate<ThingDef>)})]
    public static class RandomStuffInexpensiveFor_Patch
    {
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            bool found = false;
            var codes = instructions.ToList();
            var methodToCall = AccessTools.Method(typeof(RandomStuffInexpensiveFor_Patch), "TryRandomElementByWeightAndCommonality");
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                yield return instr;
                if (!found && i > 1 && codes[i - 1].opcode == OpCodes.Stloc_1 && codes[i].opcode == OpCodes.Ldloc_1)
                {
                    found = true;
                    i += 11;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 2);
                    yield return new CodeInstruction(OpCodes.Call, methodToCall);
                }
            }
        }

        public static bool TryRandomElementByWeightAndCommonality(this IEnumerable<ThingDef> enumerable, ThingDef thingDefFor, out ThingDef result)
        {
            return enumerable.TryRandomElementByWeight((ThingDef x) => Commonality_Patch.ModifyCommonalityOf(thingDefFor, x, x.stuffProps.commonality), out result);
        }
    }

    [HarmonyPatch(typeof(GenStuff), nameof(GenStuff.TryRandomStuffByCommonalityFor), 
        new Type[] { typeof(ThingDef), typeof(ThingDef), typeof(TechLevel) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class TryRandomStuffByCommonalityFor_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref bool __result, ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
        {
            __result = TryRandomStuffByCommonalityFor(td, out stuff, maxTechLevel);
            return false;
        }
        public static bool TryRandomStuffByCommonalityFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
        {
            if (!td.MadeFromStuff)
            {
                stuff = null;
                return true;
            }
            return GenStuff.AllowedStuffsFor(td, maxTechLevel).TryRandomElementByWeightAndCommonality(td, out stuff);
        }

        // failed transpiler attempt here
        //[HarmonyPriority(Priority.Last)]
        //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        //{
        //    bool found = false;
        //    var codes = instructions.ToList();
        //    var allowedStuff = AccessTools.Method(typeof(GenStuff), "AllowedStuffsFor");
        //    var methodToCall = AccessTools.Method(typeof(RandomStuffInexpensiveFor_Patch), "TryRandomElementByWeightAndCommonality");
        //    for (var i = 0; i < codes.Count; i++)
        //    {
        //        var instr = codes[i];
        //        Log.Message(i + " returning " + instr);
        //        yield return instr;
        //        if (!found && codes[i].Calls(allowedStuff))
        //        {
        //            found = true;
        //            i += 11;
        //            yield return new CodeInstruction(OpCodes.Ldarg_0);
        //            yield return new CodeInstruction(OpCodes.Call, methodToCall);
        //        }
        //    }
        //}
    }
}
