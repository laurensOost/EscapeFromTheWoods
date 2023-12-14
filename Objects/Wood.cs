using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EscapeFromTheWoods
{
    public class Wood
    {
        public int WoodID { get; private set; }
        private List<Tree> _trees;
        private Map _map;
        private EscapeRouteManager _escapeRouteManager;
        private MonkeyManager _monkeyManager;
        private DBwriter _dbWriter;
        private Random _r;

        public Wood(int woodID, List<Tree> trees, Map map, string path, DBwriter db)
        {
            WoodID = woodID;
            _trees = trees;
            _map = map;
            _dbWriter = db;
            _r = new Random();
            _escapeRouteManager = new EscapeRouteManager(path, db, map, _trees);
            _monkeyManager = new MonkeyManager(db);
        }

        public void PlaceMonkey(string monkeyName, int monkeyID)
        {
            _monkeyManager.PlaceMonkey(WoodID, monkeyName, monkeyID, _trees, _r);
        }

        /*public void Escape() // originele zonder async
        {
            List<List<Tree>> routes = new List<List<Tree>>();
            foreach (Monkey m in _monkeyManager.GetMonkeys())
            {
                var route = _escapeRouteManager.FindEscapeRoute(WoodID, m);
                routes.Add(route);
            }
            _escapeRouteManager.WriteEscapeRoutesToBitmap(WoodID, routes);
            _dbWriter.WriteLogRecord(new DBLogRecord { woodID = WoodID, message = "Escape ended" });
        }*/
        
        public async Task Escape() // async, sneller 
        {
            List<List<Tree>> routes = new List<List<Tree>>();
            foreach (Monkey m in _monkeyManager.GetMonkeys())
            {
                var route = _escapeRouteManager.FindEscapeRoute(WoodID, m);
                routes.Add(route);
            }
        
            // Now awaiting the asynchronous call to write the bitmap
            await _escapeRouteManager.WriteEscapeRoutesToBitmap(WoodID, routes);

            _dbWriter.WriteLogRecord(new DBLogRecord { woodID = WoodID, message = "Escape ended" });
        }

        public void WriteWoodToDB()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{WoodID}:write db wood {WoodID} start");
            List<DBWoodRecord> records = new List<DBWoodRecord>();
            foreach (Tree t in _trees)
            {
                DBWoodRecord record = new DBWoodRecord
                {
                    woodID = WoodID,
                    treeID = t.treeID,
                    location = new Location { x = t.x, y = t.y }
                };
                records.Add(record);
            }
            _dbWriter.WriteWoodRecords(records);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{WoodID}:write db wood {WoodID} end");
        }
    }
}
