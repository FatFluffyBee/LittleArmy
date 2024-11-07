using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Attack : IState
{
    private Agent agent;
    private Action LaunchAttack;


    public Attack(Agent agent, float reloadDuration, Action LaunchAttack){
        this.agent = agent;
        this.LaunchAttack = LaunchAttack;
    }

    public void OnEnter(){
        agent.EnableAgentMovement(false);
    }

    public void Tick(){
        LaunchAttack();
        if(agent.AsBeenMoveOrdered) agent.InCombat = false; //behavior to do a one hit and go back as melee ally
        agent.LookAtDirection(agent.Target.position);
    }

    public void OnExit(){}
}
