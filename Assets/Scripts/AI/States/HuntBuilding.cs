using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class HuntBuilding : IState
{
    private Agent_Ennemi agent;
    private float bldAtkRange;

    public HuntBuilding(Agent_Ennemi agent, float bldAtkRange){
        this.agent = agent;
        this.bldAtkRange = bldAtkRange;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(true);
        agent.GetNewBuildingTarget();
    }

    public void Tick(){
        if(agent.TargetBld == null) {
            agent.GetNewBuildingTarget();
            if(agent.TargetBld == null) return;
        }
       
        if(agent.TargetBld != null && bldAtkRange < agent.NavDistBld) {
            agent.CheckAndSetDestination(agent.TargetBld.GetClosestPosition());
            agent.LookAtDirection(agent.TargetBld.GetClosestPosition());
        }
    }

    public void OnExit(){}

}
