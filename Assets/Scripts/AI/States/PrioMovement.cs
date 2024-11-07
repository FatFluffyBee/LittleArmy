using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrioMovement : IState
{
    private Agent agent;

    public PrioMovement(Agent agent){
        this.agent = agent;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(true);
        agent.CheckAndSetDestination(agent.HomePoint); 
        agent.AsBeenMoveOrdered = false;
    }

    public void Tick(){
        agent.CheckAndSetDestination(agent.HomePoint); //todo need it here in case of reassignation during an ordered movement (not easy fix)
        agent.AsBeenMoveOrdered = false;

        agent.LookAtDirection(agent.HomePoint);
    }

    public void OnExit(){
    }
}
