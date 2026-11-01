using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterCV;

[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintOver")]
public static class CanPlaceBlueprintOver
{
	[HarmonyPostfix]
	public static void _Postfix(BuildableDef newDef, ThingDef oldDef, ref bool __result)
	{
		if (!__result && ((BetterCVUtil.isOverWall(newDef) && BetterCVUtil.isWall(GenConstruct.BuiltDefOf(oldDef))) || (BetterCVUtil.isOverWall(GenConstruct.BuiltDefOf(oldDef)) && BetterCVUtil.isWall(newDef))))
		{
			__result = true;
		}
	}
}
