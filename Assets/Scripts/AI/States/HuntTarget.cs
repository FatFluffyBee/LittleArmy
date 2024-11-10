using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HuntTarget : IState
{
    private Agent agent;
    private float atkRange;

    public HuntTarget(Agent agent, float atkRange){
        this.agent = agent;
        this.atkRange = atkRange;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(true);
        agent.InCombat = true;
    }

    public void Tick(){
        if(agent.Target != null /*&& agent.NavDistToTarget > atkRange*/) {
            agent.CheckAndSetDestination(agent.Target.position + (agent.transform.position - agent.Target.position).normalized * atkRange / 2);
            agent.LookAtDirection(agent.Target.position);
        }
    }

    public void OnExit(){
        agent.InCombat = false;
    }
}
