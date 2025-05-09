using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace MapRouting
{
    // 1) Basic types
    public class Node
    {
        public int Id;
        public double X, Y;
        public List<Edge> Neighbors = new List<Edge>();
    }

    public class Edge
    {
        public Node Target;
        public double LengthKm;    // in km
        public double SpeedKph;    // in km/h
        public double TimeHours => LengthKm / SpeedKph;
    }

    // 2) Graph loader
    public class Graph
    {
        public Dictionary<int, Node> Nodes = new Dictionary<int, Node>();

        public static Graph LoadFromFile(string mapPath)
        {
            var graph = new Graph();
            var lines = File.ReadAllLines(mapPath);
            int idx = 0;
            int n = int.Parse(lines[idx++]);
            // read nodes
            for (int i = 0; i < n; i++)
            {
                var parts = lines[idx++].Split();
                int id = int.Parse(parts[0]);
                double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                graph.Nodes[id] = new Node { Id = id, X = x, Y = y };
            }
            int m = int.Parse(lines[idx++]);
            // read edges
            for (int i = 0; i < m; i++)
            {
                var p = lines[idx++].Split();
                int u = int.Parse(p[0]), v = int.Parse(p[1]);
                double len = double.Parse(p[2], CultureInfo.InvariantCulture);
                double spd = double.Parse(p[3], CultureInfo.InvariantCulture);
                var nu = graph.Nodes[u];
                var nv = graph.Nodes[v];
                nu.Neighbors.Add(new Edge { Target = nv, LengthKm = len, SpeedKph = spd });
                nv.Neighbors.Add(new Edge { Target = nu, LengthKm = len, SpeedKph = spd });
            }
            return graph;
        }
    }
    public class Query
    {
        public double sX, sY; // Source
        public double dX, dY; // Destination
        public int R;

    }
    
    public class QueryList
    {

        public List<Query> queries = new List<Query>();
        public static QueryList LoadFromFile(string query_path)
        {
            var query_list = new QueryList();
            var graph = new Graph();
            var lines = File.ReadAllLines(query_path);
            int idx = 0;
            int n = int.Parse(lines[idx++]);
            // read queries
            for (int i = 0; i < n; i++)
            {
                var parts = lines[idx++].Split();
                double sx = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double sy = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double dx = double.Parse(parts[2], CultureInfo.InvariantCulture);
                double dy = double.Parse(parts[3], CultureInfo.InvariantCulture);
                int r = int.Parse(parts[4]);
                query_list.queries.Add( new Query { sX = sx, sY = sy ,dX = dx ,dY = dy , R = r});
            }
            return query_list;
        }
    }

    public class Output
    {
        public List<int> ids = new List<int>();
        public double shortest_time,total_walking,total_vehicle;
        public double total_distance => total_walking + total_vehicle;
    }

    // 3) Query processing & main
    class Program
    {
        // walking speed constant
        const double WalkKph = 5.0;
        static int file_count = 0;
        static void Main()
        {

            while (true)
            {
                Console.WriteLine("\n~~ Map App ~~\n" +
                "[1] Sample Cases\n" +
                "[2] Medium Cases\n" +
                "[3] Long Cases\n" +
                "[4] Bonus Cases\n" +
                "[5] Quit\n");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Sample Cases!\n");
                        Console.WriteLine("Case 1:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map1.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries1.txt");
                        Console.WriteLine("Case 2:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map2.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries2.txt");
                        Console.WriteLine("Case 3:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map3.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries3.txt");
                        Console.WriteLine("Case 4:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map4.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries4.txt");
                        Console.WriteLine("Case 5:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map5.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries5.txt");
                        break;
                    case "2":
                        Console.WriteLine("Medium Cases!\n");
                        ProcessFile(@"..\..\..\TEST CASES\[2] Medium Cases\Input\OLMap.txt", @"..\..\..\TEST CASES\[2] Medium Cases\Input\OLQueries.txt");
                        break;
                    case "3":
                        Console.WriteLine("Long Cases!\n");
                        ProcessFile(@"..\..\..\TEST CASES\[3] Large Cases\Input\SFMap.txt", @"..\..\..\TEST CASES\[3] Large Cases\Input\SFQueries.txt");
                        break;
                    case "4":
                        Console.WriteLine("Bonus Cases!\n");
                        Console.WriteLine("Sample Bonus Case:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[4] BONUS Test Cases\[1] Sample Cases\Input\map1B.txt", @"..\..\..\TEST CASES\[4] BONUS Test Cases\[1] Sample Cases\Input\queries1B.txt");
                        Console.WriteLine("Medium Bonus Case:\n");
                        ProcessFile(@"..\..\..\TEST CASES\[4] BONUS Test Cases\[2] Medium Cases\Input\OLMapB.txt", @"..\..\..\TEST CASES\[4] BONUS Test Cases\[2] Medium Cases\Input\OLQueries.txt");
                        break;
                    case "5":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("WRONG CHOICE, Try again!\n");
                        break;

                }
            }
            // TODO: load queries, for each:
            // 1. Find startCandidates = all nodes with euclidDist(src, node) <= R/1000
            // 2. Find endCandidates similarly
            // 3. Build a priority-queue Dijkstra over a virtual graph:
            //    – from src, can “walk” to each start node at time = dist/5
            //    – all road-edges as defined
            //    – from each end node, “walk” to dest.
            // 4. Reconstruct path & compute metrics
        }


        static long writeOuput(List<Output> outputs, long query_time, long read_time)
        {
            Stopwatch write_time = new Stopwatch();
            write_time.Start();
            var sb = new StringBuilder();

            foreach (var output in outputs)
            {
                // 1. Intersection IDs
                sb.AppendLine(string.Join(" ", output.ids));

                // 2. Shortest time
                sb.AppendLine($"{output.shortest_time.ToString("F2", CultureInfo.InvariantCulture)} mins");

                // 3. Total distance
                sb.AppendLine($"{output.total_distance.ToString("F2", CultureInfo.InvariantCulture)} km");

                // 4. Total walking distance
                sb.AppendLine($"{output.total_walking.ToString("F2", CultureInfo.InvariantCulture)} km");

                // 5. Total vehicle distance
                sb.AppendLine($"{output.total_vehicle.ToString("F2", CultureInfo.InvariantCulture)} km");

                // Empty line after each query
                sb.AppendLine();
            }

            // Query processing time (excluding I/O)
            sb.AppendLine($"{query_time} ms");
            sb.AppendLine();

            string filename = $"Output [{file_count++}].txt";
            File.WriteAllText(filename, sb.ToString());

            // Total program time (including I/O)
            write_time.Stop();
            long total_time = query_time + read_time + write_time.ElapsedMilliseconds;
            using (var writer = File.AppendText(filename))
            {
                writer.WriteLine($"{total_time} ms");
            }

            return total_time;

        }
        static Output ProcessQuery(Graph graph,Query query)
        {
            // TODO: 
            List<int> ids = new List<int>();
            ids.Add(0);
            ids.Add(1);
            ids.Add(2);
            double shortest_time =0 , total_walking = 0, total_vehicle = 0;
            // 1. Find starting points
            // Console.WriteLine("\t\tSource (X,Y): (" + query.sX + "," + query.sY + ") Destination (X,Y): (" + query.dX + "," + query.dY + ") R: " + query.R + "\n");
            // 2. Find best intersection points using Djikstra
            // 3. Store output in a list
            Output output = new Output();
            output.total_vehicle = total_vehicle;
            output.shortest_time = shortest_time;
            output.total_walking = total_walking;
            output.ids = ids;
            return output;
        }


        static void ProcessFile(string map_path, string query_path)
        {
            Stopwatch query_time = new Stopwatch();
            Stopwatch read_time = new Stopwatch();
            // Start Stop watch for Read processing time
            read_time.Start();
            List<Output> Outputs = new List<Output>();
            Graph graph = Graph.LoadFromFile(map_path);
            QueryList queryList = QueryList.LoadFromFile(query_path);
            read_time.Stop();

            // Start Stop watch for Query processing time (excluding I/O)
            query_time.Start();
            for (int i = 0; i < queryList.queries.Count; i++)
            {
                Outputs.Add(ProcessQuery(graph, queryList.queries[i]));
            }
            query_time.Stop();
            long total_time = writeOuput(Outputs, query_time.ElapsedMilliseconds, read_time.ElapsedMilliseconds);
            Console.WriteLine("Total Query time: " + query_time.ElapsedMilliseconds + "ms Total Exec Time: " + total_time + "ms Node Count: "+ graph.Nodes.Count + " Query Count: "+ queryList.queries.Count +"\n");
        }

        static double EuclidDistKm(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2, dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
