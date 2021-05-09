using System;
using System.Collections.Generic;
using UnityEngine;
using AfGD.Execise3;

namespace AfGD.Assignment1
{
    public static class AStarSearch
    {
        // Exercise 3.3 - Implement A* search
        // Explore the graph and fill the _cameFrom_ dictionairy with data using uniform cost search.
        // Similar to Exercise 3.1 PathFinding.ReconstructPath() will use the data in cameFrom  
        // to reconstruct a path between the start node and end node. 
        //
        // Notes:
        //      Use the data structures used in Exercise 3.1 and 3.2
        //
        public static void Execute(Graph graph, Node startPoint, Node endPoint, Dictionary<Node, Node> cameFrom)
        {
            var frontier = new PriorityQueue<Node>();
            frontier.Enqueue(startPoint, 0);
            var cumulativeCost = new Dictionary<Node, float>();
            cameFrom.Add(startPoint, null);
            cumulativeCost.Add(startPoint, 0);

            while(frontier.Count != 0)
            {
                var curNode = frontier.Dequeue();

                if (curNode == endPoint)
                    break;

                var neighbours = new List<Node>();
                graph.GetNeighbours(curNode, neighbours);
                foreach(Node n in neighbours)
                {
                    float newCost = cumulativeCost[curNode] + graph.GetCost(curNode, n);
                    if(!cumulativeCost.ContainsKey(n) || newCost < cumulativeCost[n])
                    {
                        cumulativeCost[n] = newCost;
                        float priority = newCost + DistanceHeuristic(n, endPoint);
                        frontier.Enqueue(n, priority);
                        cameFrom[n] = curNode;
                    }
                }
            }
        }

        private static float DistanceHeuristic(Node from, Node endPoint)
        {
            return Mathf.Sqrt(Mathf.Pow(from.Position.x - endPoint.Position.x, 2.0f) + Mathf.Pow(from.Position.y - endPoint.Position.y, 2.0f));
        }
    }
}