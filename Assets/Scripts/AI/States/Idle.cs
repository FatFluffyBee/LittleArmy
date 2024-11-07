using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : IState
{
    Agent agent;

    public Idle(Agent agent) {
        this.agent = agent;
    }

    public void OnEnter() {
        agent.EnableAgentMovement(false);
    }

    public void Tick() {
        if(agent.Target != null) 
            agent.LookAtDirection(agent.Target.position);
        else
            agent.LookAtDirection(agent.transform.position + Vector3.forward);
    }
    
    public void OnExit(){}
}
