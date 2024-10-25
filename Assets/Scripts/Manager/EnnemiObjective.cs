using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnnemiObjective : MonoBehaviour
{
    public static EnnemiObjective instance;
    List<Castle> ennemiObjectives = new List<Castle>(); 

    void Awake() { //! To change later (initialize in a map loading)
        instance = this;
        ennemiObjectives = FindObjectsOfType<Castle>().ToList();
    }
    void Update() { //! Create game manager
        if(ennemiObjectives.Count == 0)
        {
            Debug.Log("Game Over!");
        }
    }

    public Castle GetClosestObjective(Vector3 agentPos) { //TODO Use closest point on bounding box to get more accurate position
        if(ennemiObjectives.Count == 0) return null; //
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for(int i = 0; i < ennemiObjectives.Count; i++) {
            float distance = 0;
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(agentPos, ennemiObjectives[i].transform.position, NavMesh.AllAreas, path);
            for ( int j = 1; j < path.corners.Length; j++ )
            {
                distance += Vector3.Distance( path.corners[j-1], path.corners[j] );
            }

            if(closestDistance > distance) {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return ennemiObjectives[closestIndex];
    }

    public void Remove(Castle castle) {
        ennemiObjectives.Remove(castle);
    }
}
