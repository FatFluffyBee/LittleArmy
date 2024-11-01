using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Swordman : Agent
{
   [SerializeField] private float aggroRange = 20f;
   [SerializeField] private float circlingRange;
   
   [Header("Attack")]
   [SerializeField] private float atkNumTargets = 1;
   [SerializeField] private float atkRange;
   [SerializeField] private float atkRadius;
   [SerializeField] private float atkDamage;
   [SerializeField] private float atkKnockback;
   [SerializeField] private float timeBtwAtk;
   private float timeBtwAtkTimer = 0;
   [SerializeField] private ParticleSystem slashPartSystem;

    [Header("Targetting")]
    protected Transform target;
    protected bool returnHome = false;

    private float navDistToTarget;
    private float navDistToHome;
    
    void Update(){ 
        BaseUpdate();

        navDistToHome = NavMaths.DistBtwPoints(transform.position, homePoint);
        target = GetClosestTargetInRange(aggroRange, AgentType.Ennemi, TargetType.All, DistMode.Nav, out navDistToTarget);

        switch(currentState) {
            case AgentState.Idle : //dont move but attack if in range
                DoIdle();
            break;

            case AgentState.Travelling : //travelling to a new spot)
                DoTravelling();
                break;

            case AgentState.Following : //follow an ennemy trail to get in attack range      
                DoFollowing();
            break;

            case AgentState.Attacking : //attack the ennemy
                DoAttacking();
            break;
        }

        //UpdateRotation(circlingRange, navDistToTarget, target);
    }

    protected virtual void DoIdle() {
        if(target != null) SwitchAgentState(AgentState.Following);
        if(!IsAgentAtHomePoint()) {
            SwitchAgentState(AgentState.Travelling);
            SetDestination(homePoint);
        }
        returnHome = false;
        LookAtDirection(transform.position + Vector3.forward);
        EnableAgentMovement(false);
    }

    private void DoTravelling() {
        if(target != null) SwitchAgentState(AgentState.Following);
        else if(IsAgentAtDestination()) {
            SwitchAgentState(AgentState.Idle);
            asBeenMoveOrdered = false;
        }

        if(navMeshAgent.path.corners.Length > 1) LookAtDirection(navMeshAgent.path.corners[1]);
        EnableAgentMovement(true);
    }

    private void DoFollowing() {
        if(navDistToTarget > aggroRange || target == null || returnHome) {  //ennemi out of aggro zone or no ennemy
            SwitchAgentState(AgentState.Travelling);
            SetDestination(homePoint);
        }
        else if(navDistToTarget < atkRange && timeBtwAtkTimer < Time.time) { //ennemi in range  and ready to atk
            SwitchAgentState(AgentState.Attacking);
        } 
        else if (navDistToTarget < circlingRange && timeBtwAtkTimer < Time.time) { //ennemi not in circling range so just avance 
            SetDestination(target.position);
        } 
        else { //ennemi in circling range
            Vector3 circlingIdealPos = target.position + (transform.position - target.position).normalized * circlingRange;
            NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
            SetDestination(navPos.position);
        }
        
        if(target != null)
            LookAtDirection(target.position);
        EnableAgentMovement(true);
    }

    private void DoAttacking() {
        LaunchSlashAttack();
        SwitchAgentState(AgentState.Following);
        EnableAgentMovement(false);
        if(asBeenMoveOrdered && navDistToHome > aggroRange * 2) returnHome = true;
    }

    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (target.position - transform.position).normalized * atkRange / 2, atkRadius);
        if(hits.Length > 0) {
            List<DataTarget> dataTargets = new List<DataTarget>();
            foreach(Collider hit in hits) {
                if(hit.transform.GetComponent<IsTargeteable>())
                    if(hit.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ennemi) {
                        dataTargets.Add(new DataTarget(hit, Vector3.Distance(transform.position, hit.transform.position)));
                    }
            }

            if(dataTargets.Count > 0) {
                dataTargets = OrderDataTargetsByDist(dataTargets);
            }

            for(int i = 0; i < ((atkNumTargets > dataTargets.Count)? dataTargets.Count : atkNumTargets); i++) { //in case there is less target to hit than the max number of agent the atk can hit
                Vector3 knockBarDir = dataTargets[i].col.transform.position - transform.position; 
                knockBarDir.y = 0;
                Vector3 knockbackVector = knockBarDir.normalized * atkKnockback;
                dataTargets[i].col.GetComponent<HealthSystem>().TakeDamage(atkDamage, knockbackVector);
            } 
        }
        
        timeBtwAtkTimer = Time.time + timeBtwAtk;
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

