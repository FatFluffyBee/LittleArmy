using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Pathing_Enemi : Agent
{
   public List<Transform> path = new List<Transform>();
    public float speed = 10;
    int index = 0;

    void Start()
    {
        //AgentInitialize();
    }
    void FixedUpdate()
    {
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if(path.Count > 1)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, path[index].position, speed * Time.deltaTime);
            transform.position = newPos;

            if(Vector2.Distance(transform.position,path[index].position) < 0.01f)
            {
                index = (index == path.Count - 1)? 0 : index+1;
            }
        }
    }

    public override Vector3 GetPredictedPos(float timeToMove) 
    {
        int count = 0;
        if(path.Count <= 1) return transform.position; //eviter le loop infini

        Vector3 lastPos = transform.position;
        Vector3 predictedPos = transform.position;
        float timeRemaining = timeToMove;
        int predictedIndex = index;

        while(timeRemaining > 0)
        {
            Debug.Log(count);
            float timeTillNextNode = Vector3.Distance(path[predictedIndex].position, lastPos) / speed;
            if(timeTillNextNode > timeRemaining) 
            {
                predictedPos += timeRemaining * speed * (path[predictedIndex].position - lastPos).normalized;
                timeRemaining = 0;
            }
            else
            {
                predictedPos += timeTillNextNode * speed * (path[predictedIndex].position - lastPos).normalized;
                timeRemaining -= timeTillNextNode;
            }
            lastPos = path[predictedIndex].position;
            predictedIndex = (predictedIndex == path.Count - 1)? 0 : predictedIndex+1;

            count ++;
            if(count > 10) return transform.position;
        }
        return predictedPos;
    }
}
