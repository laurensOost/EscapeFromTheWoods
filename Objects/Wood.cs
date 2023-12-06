using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;

namespace EscapeFromTheWoods
{
    public class Wood
    {
        private const int drawingFactor = 8;
        private string path;
        private DBwriter db;
        private Random r = new Random();
        public int woodID { get; set; }
        public List<Tree> trees { get; set; }
        public List<Monkey> monkeys { get; private set; }
        private Map map;
        public Wood(int woodID, List<Tree> trees, Map map, string path, DBwriter db)
        {
            this.woodID = woodID;
            this.trees = trees;
            this.monkeys = new List<Monkey>();
            this.map = map;
            this.path = path;
            this.db = db;
        }

        public void PlaceMonkey(string monkeyName, int monkeyID)
        {
            int treeNr;
            do
            {
                treeNr = r.Next(0, trees.Count);
            }
            while (trees[treeNr].hasMonkey);
            Monkey m = new Monkey(monkeyID, monkeyName, trees[treeNr]);
            monkeys.Add(m);
            trees[treeNr].hasMonkey = true;
            Tree placedTree = trees[treeNr];
            db.WriteLogRecord(new DBLogRecord
            {
                woodID = this.woodID,
                monkeyID = monkeyID,
                message = $"{monkeyName} placed in tree {placedTree.treeID} at location ({placedTree.x},{placedTree.y})"
            });
        }

        public void Escape()
        {
            List<List<Tree>> routes = new List<List<Tree>>();
            foreach (Monkey m in monkeys)
            {
                var route = EscapeMonkey(m);
                routes.Add(route);
                WriteRouteToDB(m, route);
            }
            WriteEscaperoutesToBitmap(routes);
            db.WriteLogRecord(new DBLogRecord { woodID = woodID, message = "Escape ended" });
        }

        private void WriteRouteToDB(Monkey monkey, List<Tree> route)
        {
            DBMonkeyRecord record = new DBMonkeyRecord
            {
                monkeyID = monkey.monkeyID,
                monkeyName = monkey.name,
                woodID = woodID,
                route = route.Select(t => new Route
                {
                    treeID = t.treeID,
                    location = new Location { x = t.x, y = t.y }
                }).ToList()
            };
            db.WriteMonkeyRecords(new List<DBMonkeyRecord> { record });
            db.WriteLogRecord(new DBLogRecord { woodID = woodID, monkeyID = monkey.monkeyID, message = $"Route written to DB for {monkey.name}" });
        }

        public void WriteEscaperoutesToBitmap(List<List<Tree>> routes)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");
            Color[] cvalues = new Color[] { Color.Red, Color.Yellow, Color.Blue, Color.Cyan, Color.GreenYellow };
            using (Bitmap bm = new Bitmap((map.xmax - map.xmin) * drawingFactor, (map.ymax - map.ymin) * drawingFactor))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    int delta = drawingFactor / 2;
                    using (Pen p = new Pen(Color.Green, 1))
                    {
                        foreach (Tree t in trees)
                        {
                            g.DrawEllipse(p, t.x * drawingFactor, t.y * drawingFactor, drawingFactor, drawingFactor);
                        }
                    }

                    int colorN = 0;
                    foreach (List<Tree> route in routes)
                    {
                        int p1x = route[0].x * drawingFactor + delta;
                        int p1y = route[0].y * drawingFactor + delta;
                        Color color = cvalues[colorN % cvalues.Length];
                        using (Pen pen = new Pen(color, 1))
                        {
                            g.DrawEllipse(pen, p1x - delta, p1y - delta, drawingFactor, drawingFactor);
                            using (SolidBrush brush = new SolidBrush(color))
                            {
                                g.FillEllipse(brush, p1x - delta, p1y - delta, drawingFactor, drawingFactor);
                            }
                            for (int i = 1; i < route.Count; i++)
                            {
                                g.DrawLine(pen, p1x, p1y, route[i].x * drawingFactor + delta, route[i].y * drawingFactor + delta);
                                p1x = route[i].x * drawingFactor + delta;
                                p1y = route[i].y * drawingFactor + delta;
                            }
                        }
                        colorN++;
                    }
                }

                string directoryPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(path, woodID.ToString() + "_escapeRoutes.jpg");

                try
                {
                    bm.Save(filePath, ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving the image: {ex.Message}");
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} end");
        }

        public void WriteWoodToDB()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{woodID}:write db wood {woodID} start");
            List<DBWoodRecord> records = new List<DBWoodRecord>();
            foreach (Tree t in trees)
            {
                DBWoodRecord record = new DBWoodRecord
                {
                    woodID = woodID,
                    treeID = t.treeID,
                    location = new Location { x = t.x, y = t.y }
                };
                records.Add(record);
            }
            db.WriteWoodRecords(records);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{woodID}:write db wood {woodID} end");
        }

        public List<Tree> EscapeMonkey(Monkey monkey)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");
            Dictionary<int, bool> visited = new Dictionary<int, bool>();
            trees.ForEach(x => visited.Add(x.treeID, false));
            List<Tree> route = new List<Tree>() { monkey.tree };
            do
            {
                visited[monkey.tree.treeID] = true;
                SortedList<double, List<Tree>> distanceToMonkey = new SortedList<double, List<Tree>>();

                // Find the nearest tree that has not been visited
                foreach (Tree t in trees)
                {
                    if ((!visited[t.treeID]) && (!t.hasMonkey))
                    {
                        double d = Math.Sqrt(Math.Pow(t.x - monkey.tree.x, 2) + Math.Pow(t.y - monkey.tree.y, 2));
                        if (distanceToMonkey.ContainsKey(d)) distanceToMonkey[d].Add(t);
                        else distanceToMonkey.Add(d, new List<Tree>() { t });
                    }
                }

                // Calculate the distance to the nearest border (north, east, south, west)
                double distanceToBorder = (new List<double>()
                {
                    map.ymax - monkey.tree.y,
                    map.xmax - monkey.tree.x,
                    monkey.tree.y - map.ymin,
                    monkey.tree.x - map.xmin
                }).Min();

                // If there are no more unvisited trees or the nearest border is closer than the nearest tree, end the route
                if (distanceToMonkey.Count == 0 || distanceToBorder < distanceToMonkey.First().Key)
                {
                    WriteRouteToDB(monkey, route);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
                    return route;
                }

                route.Add(distanceToMonkey.First().Value.First());
                monkey.tree = distanceToMonkey.First().Value.First();
            }
            while (true);
        }
    }
}
