using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

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

    // 3) Query processing & main
    class Program
    {
        // walking speed constant
        const double WalkKph = 5.0;

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
                int choice = int.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Sample Cases!\n");
                        Console.WriteLine("Case 1:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map1.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries1.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Output\output1.txt");
                        Console.WriteLine("Case 2:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map2.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries2.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Output\output2.txt");
                        Console.WriteLine("Case 3:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map3.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries3.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Output\output3.txt");
                        Console.WriteLine("Case 4:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map4.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries4.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Output\output4.txt");
                        Console.WriteLine("Case 5:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[1] Sample Cases\Input\map5.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Input\queries5.txt", @"..\..\..\TEST CASES\[1] Sample Cases\Output\output5.txt");
                        break;
                    case 2:
                        Console.WriteLine("Medium Cases!\n");
                        CalculateFile(@"..\..\..\TEST CASES\[2] Medium Cases\Input\OLMap.txt", @"..\..\..\TEST CASES\[2] Medium Cases\Input\OLQueries.txt", @"..\..\..\TEST CASES\[2] Medium Cases\Output\OLOutput.txt");
                        break;
                    case 3:
                        Console.WriteLine("Long Cases!\n");
                        CalculateFile(@"..\..\..\TEST CASES\[3] Large Cases\Input\SFMap.txt", @"..\..\..\TEST CASES\[3] Large Cases\Input\SFQueries.txt", @"..\..\..\TEST CASES\[3] Large Cases\Output\SFOutput.txt");
                        break; 
                    case 4:
                        Console.WriteLine("Bonus Cases!\n");
                        Console.WriteLine("Sample Bonus Case:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[4] BONUS Test Cases\[1] Sample Cases\Input\map1B.txt", @"..\..\..\TEST CASES\[4] BONUS Test Cases\[1] Sample Cases\Input\queries1B.txt", @"..\..\..\TEST CASES\[4] BONUS Test Cases\[1] Sample Cases\Output\output1.txt");
                        Console.WriteLine("Medium Bonus Case:\n");
                        CalculateFile(@"..\..\..\TEST CASES\[4] BONUS Test Cases\[2] Medium Cases\Input\OLMapB.txt", @"..\..\..\TEST CASES\[4] BONUS Test Cases\[2] Medium Cases\Input\OLQueries.txt", @"..\..\..\TEST CASES\[4] BONUS Test Cases\[2] Medium Cases\Output\OLOutput.txt");
                        break;
                    case 5:
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
        static void CalculateFile(string map_path, string query_path, string output_path)
        {
            Graph graph = Graph.LoadFromFile(map_path);
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                Console.WriteLine("Hello! im node " + graph.Nodes[i].Id + " And my Coordinates are (X, Y): (" + graph.Nodes[i].X + "," + graph.Nodes[i].Y + ")\n");
                Console.WriteLine("\tIm connected to: \n");
                List<Edge> neighbors = graph.Nodes[i].Neighbors;
                for (int j = 0; j < neighbors.Count; j++)
                {
                    Console.WriteLine("\t\t[" + neighbors[j].Target.Id + "] Speed: " + neighbors[j].SpeedKph + " Length: " + neighbors[j].LengthKm + "\n");
                }
            }
        }

        static double EuclidDistKm(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2, dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
