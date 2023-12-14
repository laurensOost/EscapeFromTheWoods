using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using EscapeFromTheWoods;

namespace EscapeFromTheWoods
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Escape From The Woods");

            // MongoDB connection string
            string connectionString = "mongodb://localhost:27017";
            DBwriter db = new DBwriter(connectionString);

            string path = @"C:\Users\Ok\Desktop\HoGent\Specialisatie\Evals\EscapeFromTheWoods\EscapeGitTwee\bitmaps"; // Update the path as needed

            // Create the maps and woods
            Map m1 = new Map(0, 500, 0, 500);
            Map m2 = new Map(0, 200, 0, 400);
            Map m3 = new Map(0, 400, 0, 400);

            // Create the woods using the WoodBuilder
            Wood w1 = WoodBuilder.GetWood(10000, m1, path, db);
            Wood w2 = WoodBuilder.GetWood(10000, m2, path, db);
            Wood w3 = WoodBuilder.GetWood(5000, m3, path, db);

            // Place monkeys in the woods
            w1.PlaceMonkey("Alice", IDgenerator.GetMonkeyID());
            w2.PlaceMonkey("Tom", IDgenerator.GetMonkeyID());
            w3.PlaceMonkey("Kelly", IDgenerator.GetMonkeyID());

            // Write wood information to the database
            w1.WriteWoodToDB();
            w2.WriteWoodToDB();
            w3.WriteWoodToDB();

            /* Perform the escape simulation for each wood
            await w1.Escape();
            await w2.Escape();
            await w3.Escape();*/
            
            await Task.WhenAll(w1.Escape(), w2.Escape(), w3.Escape()); // await alle tasks tegelijk, ipv 1 voor 1, scheelt tijd (zie stopwatch), runt parallel ipv sequentieel


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