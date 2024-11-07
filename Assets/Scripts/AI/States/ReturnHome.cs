using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHome : IState
{
    private Agent agent;

    public ReturnHome(Agent agent){
        this.agent = agent;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(true);
        agent.SetDestination(agent.HomePoint); 
    }

    public void Tick(){
        agent.LookAtDirection(agent.HomePoint);
    }

    public void OnExit(){

    }
}
