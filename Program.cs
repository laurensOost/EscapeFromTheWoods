using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using EscapeFromTheWoods;
using MongoDB.Driver;

namespace EscapeFromTheWoods
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Hello World!");
            // Updated MongoDB connection string
            string connectionString = "mongodb://localhost:27017";
            DBwriter db = new DBwriter(connectionString);

            string path = @"C:\\Users\\Ok\\Desktop\\HoGent\\Specialisatie\\Evals\\EscapeFromTheWoods\\EscapeGit\\bitmaps";
            Map m1 = new Map(0, 500, 0, 500);
            Wood w1 = WoodBuilder.GetWood(500, m1, path, db);
            w1.PlaceMonkey("Alice", IDgenerator.GetMonkeyID());

            Map m2 = new Map(0, 200, 0, 400);
            Wood w2 = WoodBuilder.GetWood(2500, m2, path, db);
            w2.PlaceMonkey("Tom", IDgenerator.GetMonkeyID());

            Map m3 = new Map(0, 400, 0, 400);
            Wood w3 = WoodBuilder.GetWood(2000, m3, path, db);
            w3.PlaceMonkey("Kelly", IDgenerator.GetMonkeyID());

            w1.WriteWoodToDB();
            w2.WriteWoodToDB();
            w3.WriteWoodToDB();
            w1.Escape();
            w2.Escape();
            w3.Escape();

            stopwatch.Stop();
            db.WriteLogRecord(new DBLogRecord 
            { 
                woodID = 0, 
                monkeyID = 0, 
                message = $"Time elapsed: {stopwatch.Elapsed}" 
            });
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
            Console.WriteLine("end");
        }
    }
}