using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBuilding : IState
{
    private Agent_Ennemi agent;
    private float chargeDuration;
    private float chargeTimer;

    public AttackBuilding(Agent_Ennemi agent, float chargeDuration){
        this.agent = agent;
        this.chargeDuration = chargeDuration;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(false);
        chargeTimer = 0;
    }

    public void Tick(){ //hotwiring anim for charge easy here or separate in more state?
        if(agent.TargetBld != null) {
            chargeTimer += Time.deltaTime;
            if(chargeTimer >= chargeDuration) {
                agent.LaunchBuildingAttack();
            }
            agent.LookAtDirection(agent.TargetBld.transform.position);
        }
    }

    public void OnExit(){}
}
