using ACE.Common;
using ACE.Database.Models.Shard;
using ACE.Entity;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Managers
{
    internal static class MutationsManager
    {
        private static uint[] BlackListedLandblockIds =
        {
            0x8A02FFFF, // facility hub
        };

        private static List<ushort> BlackListedLandblocks = BlackListedLandblockIds.Select(r => new LandblockId(r).Landblock).ToList(); // Winthur Gate, Random Villas

        public static WorldObject ProcessWorldObject(WorldObject wo, AppliedRuleset ruleset, bool replace = true)
        {
            var disableHousing = ruleset.Realm.Id != RealmManager.ServerBaseRealm.Realm.Id;

            if (wo is House || wo is Storage || wo is Hook || wo is Hooker || wo is HousePortal || wo is SlumLord)
            {
                if (disableHousing || !HouseManager.ValidatePourHousing(wo.Location.LandblockId.Landblock))

                {
                    wo = null;
                    return wo;
                }
            }

            if (wo.Location != null)
            {
                var lb = wo.Location.LandblockId.Landblock;
                if (BlackListedLandblocks.Contains(lb))
                {
                    wo = null;
                    return wo;
                }
            }

            return wo;
        }
    }
}
