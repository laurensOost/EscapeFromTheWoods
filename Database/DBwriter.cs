using System.Collections.Generic;
using EscapeFromTheWoods;
using MongoDB.Driver;

public class DBwriter
{
    private MongoClient client;
    private IMongoDatabase database;

    public DBwriter(string connectionString)
    {
        client = new MongoClient(connectionString);
        database = client.GetDatabase("EscapeFromTheWoodsDB");
    }

    public void WriteWoodRecords(List<DBWoodRecord> data)
    {
        var collection = database.GetCollection<DBWoodRecord>("WoodRecords");
        collection.InsertMany(data);
    }

    public void WriteMonkeyRecords(List<DBMonkeyRecord> data)
    {
        var collection = database.GetCollection<DBMonkeyRecord>("MonkeyRecords");
        collection.InsertMany(data);
    }
    
    public void WriteLogRecord(DBLogRecord logRecord)
    {
        var collection = database.GetCollection<DBLogRecord>("Logs");
        collection.InsertOne(logRecord);
    }

}