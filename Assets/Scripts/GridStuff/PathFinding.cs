using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class PathFinding {
    /*public static int[,] directions = new int[,] {
    {-1, -1}, {-1, 0}, {-1, 1},
    { 0, -1},          { 0, 1},
    { 1, -1}, { 1, 0}, { 1, 1},
    };
    //A* breakdown : you start by a case and check all the neighbors and calculate two values, the distance in node traversed to the start point and the optimistic distance to end
    // when this is done for all neighouring nodes, you take the sum of the two numbers and take the lowest value, the most likely node for the path. 
    // you add the current node to the connection of this future node and continue. If future node as already a faster connection or cannot be travelled to skip to the next. 
    // When a node as been processed, never come back to it. all valid neigbours node are added to a tobeprocessed list. when you reach the end you just backtrack through 
    //each connection node to the start
    public static bool CalculatePathAStar(GridData startNode, GridData endNode, out List<GridData> path) {
        List<GridData> toSearch = new List<GridData>() {startNode};
        List<GridData> processed = new List<GridData>();
        path = new List<GridData>();
        int count = 0;

        while(toSearch.Any() && count < 100) {
            GridData currentNode = toSearch[0];
            foreach(GridData node in toSearch) 
                if(node.F < currentNode.F || node.F == currentNode.F && node.H < currentNode.H) 
                    currentNode = node;

            
            processed.Add(currentNode);
            toSearch.Remove(currentNode);

            if(currentNode == endNode) {
                while(currentNode!= startNode) {
                    path.Add(currentNode);
                    currentNode = currentNode.Connection;
                }
                path.Add(startNode);
                path.Reverse();
                return true;
            }

            foreach(GridData neighbor in currentNode.Neighbors.Where(t => t.IsWalkable && !processed.Contains(t))) {
                bool inSearch = toSearch.Contains(neighbor);

                float costToNeighbor = currentNode.G + GetOptimisticDistanceToTarget(currentNode.Key, neighbor.Key);

                if(!inSearch || costToNeighbor < neighbor.G){
                    neighbor.G = costToNeighbor;
                    neighbor.Connection = currentNode;

                    if(!inSearch) {
                        neighbor.H = GetOptimisticDistanceToTarget(neighbor.Key, endNode.Key);
                        toSearch.Add(neighbor);
                    }
                }
            }
            count++;
        }
        Debug.Log("Path Overflow detected, iteration count : " + count);
        return false;
    }
    public static float GetOptimisticDistanceToTarget(Vector2Int start, Vector2Int end){
        float xDiff = Mathf.Abs(start.x - end.x);  
        float yDiff = Mathf.Abs(start.y - end.y);     

        return Mathf.Min(xDiff, yDiff) * 1.4f + Mathf.Abs(xDiff - yDiff);
    }
*/
}

