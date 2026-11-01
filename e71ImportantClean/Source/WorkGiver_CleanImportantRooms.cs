using System.Collections.Generic;
using RimWorld;
using Verse;

namespace e71ImportantClean
{
    /*
     * Vanilla filth cleaning, limited to rooms whose role counts cleanliness (kitchen, hospital,
     * laboratory, bedroom, barracks, dining room, rec room, prison cell...). The work giver def
     * ranks it above vanilla CleanFilth, so those rooms get cleaned before the rest of the home area.
     */
    public class WorkGiver_CleanImportantRooms : WorkGiver_CleanFilth
    {
        private static readonly List<Thing> importantFilth = new List<Thing>();

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            importantFilth.Clear();
            List<Thing> filth = pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
            for (int i = 0; i < filth.Count; i++)
            {
                if (InImportantRoom(filth[i]))
                {
                    importantFilth.Add(filth[i]);
                }
            }
            return importantFilth;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return InImportantRoom(t) && base.HasJobOnThing(pawn, t, forced);
        }

        public static bool InImportantRoom(Thing t)
        {
            Room room = t.GetRoom();
            if (room == null || room.PsychologicallyOutdoors)
            {
                return false;
            }
            RoomRoleDef role = room.Role;
            return role != null && role.IsStatRelated(RoomStatDefOf.Cleanliness);
        }
    }
}
