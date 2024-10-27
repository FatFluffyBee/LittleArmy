using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class Swordman : Agent
{
   [SerializeField] private float aggroRange = 20f;
   [SerializeField] private float circlingRange;
   
   [Header("Attack")]
   [SerializeField] private float atkRange;
   [SerializeField] private float atkRadius;
   [SerializeField] private float atkDamage;
   [SerializeField] private float atkKnockback;
   [SerializeField] private float timeBtwAttack;
   private float timeBtwAtkTimer = 0;
   [SerializeField] private ParticleSystem slashPartSystem;

    [Header("Targetting")]
    [SerializeField] private Transform target;
    public float timeBtwTargetCheck = 0.2f;
    private bool returnHome = false;

    private float navDistToTarget;
    private float navDistToHome;
    
    void Update(){ 
        BaseUpdate();

        navDistToHome = NavMaths.DistBtwPoints(transform.position, homePoint);
        List<Transform> potentialTargets = GetTargetsInViewRange(aggroRange, AgentType.Ennemi);
        target = FindClosestTargetInNavRange(potentialTargets);
        if(target != null) {
            navDistToTarget = NavMaths.DistBtwPoints(transform.position, target.position);
            if(navDistToTarget > aggroRange) target = null;
        }
        else {
            navDistToTarget = Mathf.Infinity;
        }

        switch(AgStatus) {
            case AgentStatus.Idle : //dont move but attack if in range
                if(target != null) SwitchAgentState(AgentStatus.Following);
                if(!IsAgentAtHomePoint()) {
                    SwitchAgentState(AgentStatus.Travelling);
                    SetDestination(homePoint);
                }
                returnHome = false;
                LookAtDirection(transform.position + Vector3.forward);
                feedbackMovement = false;
            break;

            case AgentStatus.Travelling : //travelling to a new spot
                if(target != null) SwitchAgentState(AgentStatus.Following);
                else if(IsAgentAtDestination()) {
                    SwitchAgentState(AgentStatus.Idle);
                    asBeenMoveOrdered = false;
                }

                if(navMeshAgent.path.corners.Length > 1) LookAtDirection(navMeshAgent.path.corners[1]);
                feedbackMovement = true;
                break;

            case AgentStatus.Following : //follow an ennemy trail to get in attack range      
                if(navDistToTarget > aggroRange || target == null || returnHome) {  //ennemi out of aggro zone or no ennemy
                    SwitchAgentState(AgentStatus.Travelling);
                    SetDestination(homePoint);
                }
                else if(navDistToTarget < atkRange && timeBtwAtkTimer < Time.time) { //ennemi in range  and ready to atk
                    SwitchAgentState(AgentStatus.Attacking);
                } 
                else if (navDistToTarget < circlingRange && timeBtwAtkTimer < Time.time) { //ennemi not in circling range so just avance 
                    SetDestination(target.position);
                } 
                else { //ennemi in circling range
                    Vector3 circlingIdealPos = target.position + (transform.position - target.position).normalized * circlingRange;
                    NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                    SetDestination(navPos.position);
                }

                LookAtDirection(target.position);
                feedbackMovement = true;
            break;

            case AgentStatus.Attacking : //attack the ennemy
                LaunchSlashAttack();
                SwitchAgentState(AgentStatus.Following);
                feedbackMovement = false;
                if(asBeenMoveOrdered && navDistToHome > aggroRange * 2) returnHome = true;
            break;
        }

        //UpdateRotation(circlingRange, navDistToTarget, target);
    }

    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (target.position - transform.position).normalized * atkRange / 2, atkRadius);
        foreach(Collider hit in hits) {
            if(hit.transform.GetComponent<IsTargeteable>())
                if(hit.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ennemi) {
                    Vector3 knockBarDir = hit.transform.position - transform.position; //
                    knockBarDir.y = 0;
                    Vector3 knockbackVector = knockBarDir.normalized * atkKnockback;
                    hit.GetComponent<HealthSystem>().TakeDamage(atkDamage, knockbackVector);
                }
        }
        timeBtwAtkTimer = Time.time + timeBtwAttack;
    }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.green; 
            Gizmos.DrawWireSphere(transform.position, aggroRange); //Draw range
            Gizmos.color = Color.blue; 
            Gizmos.DrawWireSphere(transform.position, circlingRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}

