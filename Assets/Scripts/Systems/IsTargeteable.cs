using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AgentType {Ally, Ennemi, Neutral}
public enum ObjectType {Agent, Structure}
public class IsTargeteable : MonoBehaviour
{
    public AgentType agentType;
    public bool isRange;
    NavMeshAgent navMeshAgent;

    void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public virtual Vector3 GetPredictedPos(float timeToMove) { //return the predicted position of the object in x seconds
        if(navMeshAgent == null) return transform.position;
        else return transform.position + navMeshAgent.velocity * timeToMove;
    }
}
