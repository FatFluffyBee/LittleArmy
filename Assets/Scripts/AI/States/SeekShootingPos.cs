using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekShootingPos : IState
{
    private I_Range I_Range;
    private Agent agent;
    private float atkRange;
    private float confortRange;
    private float confortRangeGap;

    public SeekShootingPos(Agent agent, float atkRange, float confortRange, float confortRangeGap){
        this.agent = agent;
        I_Range = agent.GetComponent<I_Range>();
        this.atkRange = atkRange;
        this.confortRange = confortRange;
        this.confortRangeGap = confortRangeGap;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(true);
        agent.InCombat = true;
    }

    public void Tick(){
        if(agent.Target != null) { 
            if(agent.NavDistToTarget > atkRange || !I_Range.IsTargetHittable){
                agent.CheckAndSetDestination(agent.Target.position);
            } else if(agent.NavDistToTarget < confortRange - confortRangeGap || agent.NavDistToTarget > confortRange + confortRangeGap){
                Vector3 confortIdealPos = agent.Target.position + (agent.transform.position - agent.Target.position).normalized * confortRange;
                UnityEngine.AI.NavMesh.SamplePosition(confortIdealPos, out UnityEngine.AI.NavMeshHit navPos, atkRange, UnityEngine.AI.NavMesh.AllAreas);
                agent.CheckAndSetDestination(navPos.position);
            }
            agent.LookAtDirection(agent.Target.position);
        }
    }

    public void OnExit(){
        agent.InCombat = false;
    }
}
