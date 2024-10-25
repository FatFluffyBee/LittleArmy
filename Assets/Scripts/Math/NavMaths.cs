using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public static class NavMaths 
{
    public static float DistBtwPoints(Vector3 start, Vector3 end) {
        float distance = 0;
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);
        for (int i = 1; i < path.corners.Length; i++ )
        {
            distance += Vector3.Distance( path.corners[i-1], path.corners[i]);
        }
        return distance;
    }
}
