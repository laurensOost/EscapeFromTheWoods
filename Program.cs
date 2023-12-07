using System;
using System.Diagnostics;
using System.IO;
using EscapeFromTheWoods;

namespace EscapeFromTheWoods
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Escape From The Woods");

            // Updated MongoDB connection string
            string connectionString = "mongodb://localhost:27017";
            DBwriter db = new DBwriter(connectionString);

            string path = @"C:\Users\Ok\Desktop\HoGent\Specialisatie\Evals\EscapeFromTheWoods\EscapeGitTwee\bitmaps"; // Update the path as needed

            // Create the maps and woods
            Map m1 = new Map(0, 500, 0, 500);
            Map m2 = new Map(0, 200, 0, 400);
            Map m3 = new Map(0, 400, 0, 400);

            // Create the woods using the WoodBuilder
            Wood w1 = WoodBuilder.GetWood(500, m1, path, db);
            Wood w2 = WoodBuilder.GetWood(2500, m2, path, db);
            Wood w3 = WoodBuilder.GetWood(2000, m3, path, db);

            // Place monkeys in the woods
            w1.PlaceMonkey("Alice", IDgenerator.GetMonkeyID());
            w2.PlaceMonkey("Tom", IDgenerator.GetMonkeyID());
            w3.PlaceMonkey("Kelly", IDgenerator.GetMonkeyID());

            // Write wood information to the database
            w1.WriteWoodToDB();
            w2.WriteWoodToDB();
            w3.WriteWoodToDB();

            // Perform the escape simulation for each wood
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
            Console.WriteLine("Program completed successfully.");
        }
    }
}