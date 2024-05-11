using ACE.Entity.Enum.RealmProperties;
using System;
using System.ComponentModel;
using System.Globalization;

namespace ACE.Entity.Enum.Properties
{
    public enum RealmPropertyInt : ushort
    {
        [RealmPropertyInt(defaultValue: 0, minValue: 0, maxValue: 0)]
        Undef                                    = 0,

        [RealmPropertyInt(0, 0, 0xFFFF)]
        RulesetStampVendorCategory = 1,

        [RealmPropertyInt(0, int.MinValue, int.MaxValue)]
        CreatureStrengthAdded = 2,

        [RealmPropertyInt(0, int.MinValue, int.MaxValue)]
        CreatureEnduranceAdded = 3,

        [RealmPropertyInt(0, int.MinValue, int.MaxValue)]
        CreatureCoordinationAdded = 4,

        [RealmPropertyInt(0, int.MinValue, int.MaxValue)]
        CreatureQuicknessAdded = 5,

        [RealmPropertyInt(0, int.MinValue, int.MaxValue)]
        CreatureFocusAdded = 6,

        [RealmPropertyInt(0, int.MinValue, int.MaxValue)]
        CreatureSelfAdded = 7,

        /*Below not implemented*/
        [RealmPropertyInt(0, -100000, 100000)]
        PlayerDamageMeleeAdded = 8,

        [RealmPropertyInt(0, -100000, 100000)]
        PlayerDamageMagicAdded = 9,

        [RealmPropertyInt(0, -100000, 100000)]
        PlayerDamageMissileAdded = 10,

        [RealmPropertyInt(0, -100000, 100000)]
        HealingKitHealAdded = 11,

        [RealmPropertyInt(0, -100000, 100000)]
        FoodRestoreAmountAdded = 12,

        [RealmPropertyInt((int)PlayerInstanceSelectMode.HomeRealm)]
        RecallInstanceSelectMode = 13,

        [RealmPropertyInt((int)PlayerInstanceSelectMode.HomeRealm)]
        PortalInstanceSelectMode = 14,

        /// <summary>
        /// Landblocks which have been inactive for this many minutes will be unloaded
        /// </summary>
        [RealmPropertyInt(5, 1, 1440)]
        LandblockUnloadInterval = 15,

        // between 1/30 to 1/100 chance ore spawns on death of mob or generator spawn
        [RealmPropertyInt(30, 30, 100)]
        OreDropChance = 16,

        // between 1/1 to 1/5 chance slayer skulls drop on ore
        [RealmPropertyInt(1, 1, 5)]
        OreSlayerDropChance = 17,

        // multiply this value by 2 and that's the amount of salvage dropped from ore
        // default value is 2, which means 4 bags of slavage will drop, but up to 10 may drop if 5 is selected
        [RealmPropertyInt(2, 2, 5)]
        OreSalvageDropAmount = 18,
    }

    public static class RealmPropertyIntExtensions
    {
        public static string GetDescription(this RealmPropertyInt prop)
        {
            var description = prop.GetAttributeOfType<DescriptionAttribute>();
            return description?.Description ?? prop.ToString();
        }
    }
}
