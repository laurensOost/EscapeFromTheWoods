using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EscapeFromTheWoods
{
    public class EscapeRouteManager
    {
        private const int DrawingFactor = 8;
        
        private DBwriter _dbWriter;
        private string _path;
        private Map _map;
        private List<Tree> _trees;
        private Dictionary<int, Tree> _treeDictionary;

        public EscapeRouteManager(string path, DBwriter dbWriter, Map map, List<Tree> trees) 
        {
            _path = path;
            _dbWriter = dbWriter;
            _map = map;
            _trees = trees;
            _treeDictionary = trees.ToDictionary(t => t.treeID); // Convert the list to a dictionary
        }

        /*public void WriteEscapeRoutesToBitmap(int woodID, List<List<Tree>> routes) // oude methode zonder async
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");
            Color[] cvalues = new Color[] { Color.Red, Color.Yellow, Color.Blue, Color.Cyan, Color.GreenYellow };
            using (Bitmap bm = new Bitmap((_map.xmax - _map.xmin) * DrawingFactor, (_map.ymax - _map.ymin) * DrawingFactor))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    int delta = DrawingFactor / 2;
                    using (Pen p = new Pen(Color.Green, 1))
                    {
                        foreach (Tree t in _treeDictionary.Values)
                        {
                            g.DrawEllipse(p, t.x * DrawingFactor, t.y * DrawingFactor, DrawingFactor, DrawingFactor);
                        }
                    }

                    int colorN = 0;
                    foreach (List<Tree> route in routes)
                    {
                        int p1x = route[0].x * DrawingFactor + delta;
                        int p1y = route[0].y * DrawingFactor + delta;
                        Color color = cvalues[colorN % cvalues.Length];
                        using (Pen pen = new Pen(color, 1))
                        {
                            g.DrawEllipse(pen, p1x - delta, p1y - delta, DrawingFactor, DrawingFactor);
                            using (SolidBrush brush = new SolidBrush(color))
                            {
                                g.FillEllipse(brush, p1x - delta, p1y - delta, DrawingFactor, DrawingFactor);
                            }
                            for (int i = 1; i < route.Count; i++)
                            {
                                g.DrawLine(pen, p1x, p1y, route[i].x * DrawingFactor + delta, route[i].y * DrawingFactor + delta);
                                p1x = route[i].x * DrawingFactor + delta;
                                p1y = route[i].y * DrawingFactor + delta;
                            }
                        }
                        colorN++;
                    }
                }

                string directoryPath = Path.GetDirectoryName(_path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(_path, woodID.ToString() + "_escapeRoutes.jpg");

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
        }*/
        
        public async Task WriteEscapeRoutesToBitmap(int woodID, List<List<Tree>> routes) // async method, is sneller 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");

            Color[] cvalues = { Color.Red, Color.Yellow, Color.Blue, Color.Cyan, Color.GreenYellow };

            using (Bitmap bm = new Bitmap((_map.xmax - _map.xmin) * DrawingFactor, (_map.ymax - _map.ymin) * DrawingFactor))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    int delta = DrawingFactor / 2;
                    using (Pen p = new Pen(Color.Green, 1))
                    {
                        foreach (Tree t in _treeDictionary.Values)
                        {
                            g.DrawEllipse(p, t.x * DrawingFactor, t.y * DrawingFactor, DrawingFactor, DrawingFactor);
                        }
                    }

                    int colorN = 0;
                    foreach (List<Tree> route in routes)
                    {
                        int p1x = route[0].x * DrawingFactor + delta;
                        int p1y = route[0].y * DrawingFactor + delta;
                        Color color = cvalues[colorN % cvalues.Length];
                        using (Pen pen = new Pen(color, 2))
                        {
                            g.DrawEllipse(pen, p1x - delta, p1y - delta, DrawingFactor, DrawingFactor);
                            using (SolidBrush brush = new SolidBrush(color))
                            {
                                g.FillEllipse(brush, p1x - delta, p1y - delta, DrawingFactor, DrawingFactor);
                            }
                            for (int i = 1; i < route.Count; i++)
                            {
                                g.DrawLine(pen, p1x, p1y, route[i].x * DrawingFactor + delta, route[i].y * DrawingFactor + delta);
                                p1x = route[i].x * DrawingFactor + delta;
                                p1y = route[i].y * DrawingFactor + delta;
                            }
                        }
                        colorN++;
                    }
                }

                string directoryPath = Path.GetDirectoryName(_path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(_path, woodID.ToString() + "_escapeRoutes.jpg");
                await Task.Run(() => bm.Save(filePath, ImageFormat.Jpeg));
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} end");
        }

        /*public List<Tree> FindEscapeRoute(int woodID, Monkey monkey) // O(n^2), voor elke verdubbeling van het aantal bomen wordt de tijd 4 keer zo lang. 
        {   
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //originele methode
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");
            Dictionary<int, bool> visited = new Dictionary<int, bool>(); //houdt bij welke bomen al bezocht zijn door de aap, door bool 
            _trees.ForEach(x => visited.Add(x.treeID, false));
            List<Tree> route = new List<Tree>() { monkey.tree };
            
            do // eerste loop is O(N), tweede nested loop is O(N^2), dus O(N^2)
            {
                visited[monkey.tree.treeID] = true; // 0(1) operatie 
                SortedList<double, List<Tree>> distanceToMonkey = new SortedList<double, List<Tree>>(); // ook O(1) operatie

                // Find the nearest tree that has not been visited
                foreach (Tree t in _trees) // voor elke boom in de lijst van bomen (N trees), dus O(N) operatie, de nested loop. 
                {
                    if ((!visited[t.treeID]) && (!t.hasMonkey)) // checken of de boom al bezocht is en of er al een aap in zit, ook O(1) operatie
                    {
                        double d = Math.Sqrt(Math.Pow(t.x - monkey.tree.x, 2) + Math.Pow(t.y - monkey.tree.y, 2)); // onafhankelijke operatie van N tussen twee punten, dus 0(1) operatie, maar wel in nested loop
                        if (distanceToMonkey.ContainsKey(d)) distanceToMonkey[d].Add(t);
                        else distanceToMonkey.Add(d, new List<Tree>() { t });
                    }
                }

                // Calculate the distance to the nearest border (north, east, south, west)
                double distanceToBorder = (new List<double>()
                {
                    _map.ymax - monkey.tree.y,
                    _map.xmax - monkey.tree.x,
                    monkey.tree.y - _map.ymin,
                    monkey.tree.x - _map.xmin
                }).Min();

                // If there are no more unvisited trees or the nearest border is closer than the nearest tree, end the route
                if (distanceToMonkey.Count == 0 || distanceToBorder < distanceToMonkey.First().Key)
                {
                    WriteRouteToDB(woodID, monkey, route);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
                    stopwatch.Stop();
                    Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
                    return route;
                }

                route.Add(distanceToMonkey.First().Value.First());
                monkey.tree = distanceToMonkey.First().Value.First();
            }
            while (true); 

        }*/
        
        public List<Tree> FindEscapeRoute(int woodID, Monkey monkey) // met dictionaries ipv lists, O(1) want dictionaries zijn hashtables en hebben een constante lookup tijd
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");
            
            Dictionary<int, bool> visited = new Dictionary<int, bool>(); // houdt bij welke bomen al bezocht zijn door de aap, door bool
            foreach (var tree in _treeDictionary.Values)
            {
                visited[tree.treeID] = false; // initieel alle bomen op false, want duh 
            }
            List<Tree> route = new List<Tree>() { monkey.tree };

            while (true)
            {
                visited[monkey.tree.treeID] = true; // aap is in de boom, dus true
                Tree nearestTree = null;
                double nearestDistance = double.MaxValue;

                foreach (var kvp in _treeDictionary)
                {
                    Tree t = kvp.Value;
                    if (!visited[t.treeID] && !t.hasMonkey) // als de boom nog niet bezocht is en er geen aap in zit
                    {
                        double d = Math.Sqrt(Math.Pow(t.x - monkey.tree.x, 2) + Math.Pow(t.y - monkey.tree.y, 2));
                        if (d < nearestDistance)
                        {
                            nearestDistance = d;
                            nearestTree = t;
                        }
                    }
                }

                double distanceToBorder = new List<double>
                {
                    _map.ymax - monkey.tree.y,
                    _map.xmax - monkey.tree.x,
                    monkey.tree.y - _map.ymin,
                    monkey.tree.x - _map.xmin
                }.Min();

                if (nearestTree == null || distanceToBorder < nearestDistance)
                {
                    WriteRouteToDB(woodID, monkey, route);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
                    stopwatch.Stop();
                    Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
                    return route;
                }

                route.Add(nearestTree);
                monkey.tree = nearestTree;
            }
        }


        public void WriteRouteToDB(int woodID, Monkey monkey, List<Tree> route)
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
            _dbWriter.WriteMonkeyRecords(new List<DBMonkeyRecord> { record });
            _dbWriter.WriteLogRecord(new DBLogRecord { woodID = woodID, monkeyID = monkey.monkeyID, message = $"Route written to DB for {monkey.name}" });
        }
    }
}
