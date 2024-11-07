using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HuntTarget : IState
{
    private Agent agent;

    public HuntTarget(Agent agent){
        this.agent = agent;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(true);
        agent.InCombat = true;
    }

    public void Tick(){
        if(agent.Target != null) {
            agent.CheckAndSetDestination(agent.Target.position);
            agent.LookAtDirection(agent.Target.position);
        }
    }

    public void OnExit(){
        agent.InCombat = false;
    }
}
