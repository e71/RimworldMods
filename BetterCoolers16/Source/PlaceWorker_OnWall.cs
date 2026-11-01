using Verse;

namespace BetterCV;

public class PlaceWorker_OnWall : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		Building edifice = loc.GetEdifice(map);
		if (edifice == null)
		{
			return "MessagePlacementOnSupport".Translate();
		}
		if (edifice.def == null)
		{
			return "MessagePlacementOnSupport".Translate();
		}
		if (edifice.def.IsSmoothed)
		{
			return AcceptanceReport.WasAccepted;
		}
		if (edifice.def.graphicData == null)
		{
			return "MessagePlacementOnSupport".Translate();
		}
		if ((edifice.def.graphicData.linkFlags & LinkFlags.Wall) == 0)
		{
			return "MessagePlacementOnSupport".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}
}
