using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GeometryToGrid //might be more concise if we put data on building blocks
{
    static int MAX_GRID_SIZE = 10;
    public static GridData[,] CreateGridFromGeometry(Transform parent, int gridScale, out Vector3 origin){ // extract the grid data from the geometry 
        GridData[,] gridArray = new GridData[MAX_GRID_SIZE, MAX_GRID_SIZE];
        Vector3 offsetCenterCase = new Vector3(1, 0, 1) * gridScale / 2;
        origin = parent.transform.position;

        for(int i = 0 ; i < MAX_GRID_SIZE; i++)
            for(int j = 0 ; j < MAX_GRID_SIZE; j++){
                gridArray [i, j] = null;
                GridData gridData = new GridData(new Vector2Int(i, j), 0, false);
                Vector3 startPoint = origin + new Vector3(i, 10, j) * gridScale + offsetCenterCase;

                RaycastHit[] hits = Physics.RaycastAll(startPoint, -Vector3.up, 200);
                Array.Sort(hits, (hit2, hit1) => hit1.distance.CompareTo(hit2.distance)); //trie l'array pour que la plus petite distance soit devant
                
                foreach(RaycastHit hit in hits){
                    if (hit.collider.CompareTag("Cube")){
                        gridData.Height = hit.transform.position.y - origin.y + gridScale / 2;
                        gridData.IsWalkable = true;

                        gridArray[i, j] = gridData;
                        continue;
                    }
                }
            }

                
    
        return gridArray;
    }
}
