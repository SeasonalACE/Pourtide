using ACE.Common;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Database.Models.World;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.Features.Rifts;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Managers
{
    internal static class MutationsManager
    {

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static uint GetMonsterTierByLevel(uint level)
        {
            uint tier = 0;

            if (level <= 300)
                tier = 6;
            if (level <= 220)
                tier = 5;
            if (level <= 150)
                tier = 4;
            if (level <= 115)
                tier = 3;
            if (level <= 100)
                tier = 2;
            if (level <= 50)
                tier = 1;
            if (level <= 20)
                tier = 0;

            return tier;
        }

        private static uint[] BlackListedLandblockIds =
        {
            0x8A02FFFF, // facility hub
        };

        private static List<ushort> BlackListedLandblocks = BlackListedLandblockIds.Select(r => new LandblockId(r).Landblock).ToList(); // Winthur Gate, Random Villas

        public static ACE.Entity.Models.Weenie ProcessLandblockInstance(ACE.Entity.Models.Weenie weenie, AppliedRuleset ruleset, LandblockInstance instance, uint iid)
        {
            if (weenie == null)
                return null;

            var location = new Position(instance.ObjCellId, instance.OriginX, instance.OriginY, instance.OriginZ, instance.AnglesX, instance.AnglesY, instance.AnglesZ, instance.AnglesW, iid);

            var lb = location.LandblockId.Landblock;
            if (BlackListedLandblocks.Contains(lb))
            {
                return null;
            }

            var weenieType = weenie.WeenieType;

            var disableHousing = ruleset.Realm.Id != RealmManager.ServerBaseRealm.Realm.Id;

            if (weenieType == WeenieType.House ||
                weenieType == WeenieType.Storage ||
                weenieType == WeenieType.Hook ||
                weenieType == WeenieType.Hooker ||
                weenieType == WeenieType.HousePortal ||
                weenieType == WeenieType.SlumLord)
            {
                if (disableHousing || !HouseManager.ValidatePourHousing(location.LandblockId.Landblock))
                    return null;
            }

            return weenie;
        }

        private static WorldObject RollForOre(Position position, uint tier = 1)
        {
            if (ThreadSafeRandom.Next(1, 100) == 1)
            {
                var ore = WorldObjectFactory.CreateNewWorldObject(603001);

                if (tier >= 2 && ThreadSafeRandom.Next(1, 10) == 1)
                {
                    ore?.Destroy();
                    ore = WorldObjectFactory.CreateNewWorldObject((uint)603002);
                }

                if (tier >= 4 && ThreadSafeRandom.Next(1, 20) == 1)
                {
                    ore?.Destroy();
                    ore = WorldObjectFactory.CreateNewWorldObject((uint)603003);
                }
                ore.Location = new Position(position);
                return ore;
            }

            return null;
        }
    }
}
