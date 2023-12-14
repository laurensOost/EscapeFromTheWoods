using System;
using System.Collections.Generic;
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

            // Create the maps
            Map m1 = new Map(0, 500, 0, 500);
            Map m2 = new Map(0, 200, 0, 200);
            Map m3 = new Map(0, 400, 0, 400);

            // parallel 
            var woodTasks = new List<Task<Wood>>
            {
                Task.Run(() => WoodBuilder.GetWood(10000, m1, path, db)),
                Task.Run(() => WoodBuilder.GetWood(10000, m2, path, db)),
                Task.Run(() => WoodBuilder.GetWood(10000, m3, path, db))
            };
            
            while (woodTasks.Count > 0)
            {
                Task<Wood> finishedTask = await Task.WhenAny(woodTasks);
                woodTasks.Remove(finishedTask);
                Wood wood = await finishedTask;

                wood.PlaceMonkey("Alice", IDgenerator.GetMonkeyID());
                wood.WriteWoodToDB();
                await wood.Escape(); // Now awaiting the escape process here
            }

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