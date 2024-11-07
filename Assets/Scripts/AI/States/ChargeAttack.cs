using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChargeAttack : IState
{
    private Agent agent;
    private float chargeDuration;
    private float chargeTimer;
    private Action LaunchAttack;

    public ChargeAttack(Agent agent, float chargeDuration, Action LaunchAttack){
        this.agent = agent;
        this.chargeDuration = chargeDuration;
        this.LaunchAttack = LaunchAttack;
    }

    public ChargeAttack(Agent agent, Action LaunchAttack){
        this.agent = agent;
        chargeDuration = 0;
        this.LaunchAttack = LaunchAttack;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(false);
        chargeTimer = 0;
    }

    public void Tick(){ //hotwiring anim for charge easy here or separate in more state?
        chargeTimer += Time.deltaTime;
        if(chargeTimer >= chargeDuration) {
            LaunchAttack();
            if(agent.AsBeenMoveOrdered) agent.InCombat = false; //behavior to do a one hit and go back as melee ally
        }
        agent.LookAtDirection(agent.Target.position);
    }

    public void OnExit(){}
}

