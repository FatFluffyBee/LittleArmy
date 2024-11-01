using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordman_Shield : Swordman
{
    [Header("Shield")]
    [SerializeField] private float shieldRaiseRange = 30;
    [SerializeField] private Shield shield;
    private Transform closestRangeTarget;

    protected override void DoIdle() //only modif from swordman is the shield raising the shield
    {
        closestRangeTarget = GetClosestTargetInRange(shieldRaiseRange, AgentType.Ennemi, TargetType.Range, DistMode.View);

        if(closestRangeTarget != null) {
            shield.RaiseShield();
            LookAtDirection(closestRangeTarget.position);
        } else {
            shield.LowerShield();
            LookAtDirection(transform.position + Vector3.forward);
        }

        returnHome = false;
        EnableAgentMovement(false);

        if(target != null) {
            SwitchAgentState(AgentState.Following);
            shield.LowerShield();
        }
            
        if(!IsAgentAtHomePoint()) {
            SwitchAgentState(AgentState.Travelling);
            SetDestination(homePoint);
            shield.LowerShield();
        }
    }
}
