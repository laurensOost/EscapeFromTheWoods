using System;
using System.Collections.Generic;
using EscapeFromTheWoods;

public class Wood
{
    private int _woodID;
    private List<Tree> _trees;
    private Map _map;
    private EscapeRouteManager _escapeRouteManager;
    private MonkeyManager _monkeyManager;
    private DBwriter _dbWriter;
    private Random _r;

    public Wood(int woodID, List<Tree> trees, Map map, string path, DBwriter db)
    {
        _woodID = woodID;
        _trees = trees;
        _map = map;
        _dbWriter = db;
        _r = new Random();
        _escapeRouteManager = new EscapeRouteManager(path, db, map, _trees); // Pass the 'trees' list here
        _monkeyManager = new MonkeyManager(db);
    }

    public void PlaceMonkey(string monkeyName, int monkeyID)
    {
        _monkeyManager.PlaceMonkey(_woodID, monkeyName, monkeyID, _trees, _r);
    }

    public void Escape()
    {
        List<List<Tree>> routes = new List<List<Tree>>();
        foreach (Monkey m in _monkeyManager.GetMonkeys())
        {
            var route = _escapeRouteManager.FindEscapeRoute(_woodID, m);
            routes.Add(route);
        }
        _escapeRouteManager.WriteEscapeRoutesToBitmap(_woodID, routes);
        _dbWriter.WriteLogRecord(new DBLogRecord { woodID = _woodID, message = "Escape ended" });
    }

    public void WriteWoodToDB()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{_woodID}:write db wood {_woodID} start");
        List<DBWoodRecord> records = new List<DBWoodRecord>();
        foreach (Tree t in _trees)
        {
            DBWoodRecord record = new DBWoodRecord
            {
                woodID = _woodID,
                treeID = t.treeID,
                location = new Location { x = t.x, y = t.y }
            };
            records.Add(record);
        }
        _dbWriter.WriteWoodRecords(records);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{_woodID}:write db wood {_woodID} end");
    }
}