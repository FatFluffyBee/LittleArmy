using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle_RaiseShield : IState
{
    Agent agent;
    I_Shield Shield;

    public Idle_RaiseShield(Agent agent, I_Shield Shield) {
        this.agent = agent;
        this.Shield = Shield;
    }

    public void OnEnter() {
        agent.EnableAgentMovement(false);
    }

    public void Tick() {
        if(Shield.GetShield() != null)
            if(Shield.TargetShield != null) {
                Shield.GetShield().RaiseShield();
                agent.LookAtDirection(Shield.TargetShield.position);
            } else {
                Shield.GetShield().RaiseShield();
                agent.LookAtDirection(agent.transform.position + Vector3.forward);
            }
        Debug.Log(agent.name + " " + Shield.GetShield());
    }
    
    public void OnExit(){
        if(Shield.GetShield() != null)
            Shield.GetShield().LowerShield();
    }
}
