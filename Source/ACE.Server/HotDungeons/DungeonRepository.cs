using ACE.Adapter.GDLE.Models;
using ACE.Server.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.HotDungeons
{
    internal static class DungeonRepository
    {
        private static Dictionary<string, DungeonLandblock> Landblocks = new Dictionary<string, DungeonLandblock>();

        public static ReadOnlyDictionary<string, DungeonLandblock> ReadonlyLandblocks;


        private static string CsvFile = "dungeon_ids.csv";

        public static void Initialize()
        {
            if (Landblocks.Count == 0)
            {
                ImportDungeonsFromCsv();
            }

            ReadonlyLandblocks = new ReadOnlyDictionary<string, DungeonLandblock>(Landblocks);
        }

        private static void ImportDungeonsFromCsv()
        {
            string csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HotDungeons", "dungeon_ids.csv");

            if (!File.Exists(csvFilePath))
            {
                throw new Exception("Failed to read dungeon_ids.csv");
            }

            using (StreamReader reader = new StreamReader(csvFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(';');

                    string landblock = parts[0];
                    string name = parts[1];
                    string coords = parts[2];

                    if (coords.Length == 2)
                        coords = "";
                    else if (coords.Length > 2 && coords.Substring(coords.Length - 2) == ",,")
                    {
                        coords = coords.Substring(0, coords.Length - 2);
                    }

                    DungeonLandblock dungeon = new DungeonLandblock(landblock, name, coords);

                    Landblocks[landblock] = dungeon;
                }
            }
        }

        public static DungeonLandblock? GetDungeon(string lb)
        {
            Landblocks.TryGetValue(lb, out DungeonLandblock dungeon);
            return dungeon;
        }

        public static bool HasDungeon(string lb)
        {
            return Landblocks.ContainsKey(lb);
        }

    }
}
