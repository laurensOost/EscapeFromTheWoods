using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace EscapeFromTheWoods
{
    public class EscapeRouteManager
    {
        private const int DrawingFactor = 8;
        
        private DBwriter _dbWriter;
        private string _path;
        private Map _map;
        private List<Tree> _trees;

        public EscapeRouteManager(string path, DBwriter dbWriter, Map map, List<Tree> trees) 
        {
            _path = path;
            _dbWriter = dbWriter;
            _map = map;
            _trees = trees; 
        }

        public void WriteEscapeRoutesToBitmap(int woodID, List<List<Tree>> routes)
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
                        foreach (Tree t in _trees)
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
        }

        public List<Tree> FindEscapeRoute(int woodID, Monkey monkey)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");
            Dictionary<int, bool> visited = new Dictionary<int, bool>();
            _trees.ForEach(x => visited.Add(x.treeID, false));
            List<Tree> route = new List<Tree>() { monkey.tree };
            do
            {
                visited[monkey.tree.treeID] = true;
                SortedList<double, List<Tree>> distanceToMonkey = new SortedList<double, List<Tree>>();

                // Find the nearest tree that has not been visited
                foreach (Tree t in _trees)
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
                    return route;
                }

                route.Add(distanceToMonkey.First().Value.First());
                monkey.tree = distanceToMonkey.First().Value.First();
            }
            while (true);
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
