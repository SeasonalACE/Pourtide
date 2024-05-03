# SeasonalACE

## Summary

Seasonal ACE is a fork of [ACEmulator](https://github.com/ACEmulator/ACE) and  [ACRealms](https://github.com/ACRealms/ACRealms.WorldServer). It comes with instancing functionality, specifically realms, also known as "seasons". A season can be thought of as an instance of an entire AC world server, however, Seasonal ACE can support multiple seasons on a single server simultaneously. Additionally, a season also has the capability to support instance dungeon landblocks, where multiple dungeons of the same landblock can exist, with their own rules. 

This project is experimental, and exists as a base for any projects that wish to have instancing with Asheron's Call, and can be played the same as any other ACE server. 


## Caveats

1. `LandblockInstance` entities are shared across seasons. This means all static guids belonging to a `LandblockInstance` are the same no matter which realm  your character exists in. 
2. Because all static guids are shared, housing cannot exist in multiple seasons independently. SeasonalACE only supports housing in the current active season, past seasons that are no longer active have all housing related biotas removed and housing storage cleared (characters can still be played in past seasons, just without housing).
3. There are still unforseen bugs. ACRealms never had much exposure and is not very battle tested but there should be no serious impact on performance or player experience. 


## Custom Realm Config
* The `Content\json\realms.jsonc` is your root config file. In this file you define key/value pairs, where the key is the name of a realm you have created (more on this next), and the value is the realm identifier that is used internally.
* The `Content\json\realms\realm` directory is where your realm JSON files exist, these are what I would describe as your seasons. Realms have a parent/child relationship, multiple realms can inherit rules from other realms. You will find many config files that you can use as a foundation and example to learn from. 

For example the default season for SeasonalACE is the `realms-pvp.jsonc` file:

This realm inherits from the `Modern Realm`.

```json
{
  "name": "Modern Realm (PvP)",
  "type": "Realm",
  "parent": "Modern Realm",
  "properties": {
    "Description": "The Customized PvP Realm with the latest and greatest features.",
    "IsPKOnly": true
  }
}
```


* The `Custom\json\realms\rulesets` directory contains rulesets config files. Rulesets should be used for dungeon landblock instancing only. Rulesets are unique as they can be composed with other rulesets, and the values for certain rules can be applied with RNG values.

#### RealmProperty Enums

These are shared by all realms, application-wide, and list the possible realm properties that can be used. If the property is in this list, it can be used in a realm definition.
```C#
    public enum RealmPropertyFloat : ushort
    {
        [RealmPropertyFloat(defaultValue: 0f, minValue: 0f, maxValue: 0f)]
        Undef                          = 0,

        // First param is the default, second is the min, third is the max.
        [RealmPropertyFloat(1f, 0.1f, 5f)]
        SpellCasting_MoveToState_UpdatePosition_Threshold = 1,

        // If defaultFromServerProperty is defined, the corresponding property from PropertyManager (/fetchdouble in this case) will used in place of the default.
        // If the server property with the given name is missing from the database, the defaultValue parameter will be used as a fallback. 
        [RealmPropertyFloat(defaultFromServerProperty: "spellcast_max_angle", 20f, 0f, 360f)]
        Spellcasting_Max_Angle = 2,
        ...
    }
```

## Known Issues

- Housing is working, but not tested in full yet. Purchasing houses, abandoning houses, villa portals, villa storage have been tested. I haven't tried mansions, apartments, allegiance housing, booting, or permissions yet. I'm sure not everything works yet but it is just a matter of fixing minor things. The hard technical problems related to housing have already been solved, however.
- Ruleset and realm files were originally intended to be updatable without a restart of the server, and a very early version of this project allowed it, but there were issues with caching. I still want to fix that because restarting the server is not convenient when experimenting with ideas for new rulesets.
- The ruleset specification is complex and not covered by any unit tests. If you notice any unexpected behavior with rulesets, please report it!
- landblock content files are global and cannot be defined on a realm by realm basis yet. This is something I've wanted to address for a very long time now but it hasn't been done yet because of priorities.

## Developer notes

Property IDs (ACE.Entity.Enum.Properties.PropertyXXX) from 42000-42999 are reserved by AC Realms Core. 

Realm Property IDs (ACE.Entity.Enum.Properties.RealmPropertyXXX) from 0-9999 are reserved by AC Realms Core in a similar manner.

If using this project in your own server, please do not add new properties with IDs in this range.

## Contact

`russellfannin0@gmail.com`

Discord Link: https://discord.gg/pN65pYqZeS

## License

AGPL v3

## Server Operator Guidelines

```json
{
  "name": "random-test",
  "type": "Ruleset",
  "properties": {
    "Description": "A test ruleset for random properties.",
    "Spellcasting_Max_Angle": {
      "low": "5",
      "high": "180",
      "reroll": "landblock" /*never, always, landblock, admin */,
      "locked": true,
      "probability": 0.5
    },
    "CreatureSpawnHPMultiplier": {
      "low": 0.5,
      "high": 2.0,
      "compose": "add"
    }
  },
  "apply_rulesets": ["creature-attribute-randomizer"],
  "apply_rulesets_random": [
    [{ "creature-hp-boost": 0.5 }, { "creature-hp-boost": 1.0 }],
    { "pvp-ruleset": "auto" },
    { "creature-attribute-randomizer": 0.5 }
  ]
}
```

## Commands

**Admin Commands**
1. `/change-season <realmId>` - This command changes the current active season to a new realmId, newly created players will be created in this season, having no physical interactions with other seasoned characters. Transitioning to a new season clears housing and housing storage from the previous active season.
2. `/exiti` - Exits the current ephemeral realm (dungeon landblock instance).

**Player Commands**
1. `/season-info` - This command gives you information about the current active season that a player belongs to.
2. `/season-list` - This command displays a list of all available seasons to choose from, highlighting the current active season.
3. `/realm-list` - This command displays all realms that exist in the realms.jsonc file.
4. `/ruleset-list` - This command displays all rulesets that exist in rulesets directory.
5. `/realm-info` - This command displays all realm information for the current realm a player exists in (useful for dungeon landblock realms, also known as ephemeral realms).