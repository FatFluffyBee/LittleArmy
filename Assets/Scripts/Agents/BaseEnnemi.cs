using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ennemi_Basic : Agent
{
    [Header("Behavior")]
    private Transform agentTarget;
    private float navDistToTarget;
    private Transform buildingTarget;
    [SerializeField] private float viewRange;
    [SerializeField] private float circlingRange;
    [SerializeField] private float attackLaunchRange;
    
    [Header("Attack")]
    [SerializeField] private ParticleSystem slashPartSystem;
    [SerializeField] private float timeBtwAttack;
    float timeBtwAttackTimer = 0;
    [SerializeField] private float atkRadius;
    [SerializeField] private float atkKnockback;
    [SerializeField] private float atkDamage;
    
    void Start()
    {
        Initialize(null, Color.black);
    }

    void Update()
    { 
        BaseUpdate();

        //first check if need to change status
        
        List<Transform> potentialTargets = GetTargetsInViewRange(viewRange, AgentType.Ally);
        agentTarget = FindClosestTargetInNavRange(potentialTargets);

        if(agentTarget != null) { 
            AgStatus = AgentStatus.SeekAgent;
            navDistToTarget = NavMaths.DistBtwPoints(transform.position, agentTarget.position);
            feedbackMovement = true;
        }
        else {
            AgStatus = AgentStatus.SeekBuilding;
            feedbackMovement = true;
        }

        //then apply behavior depending on current state
        switch(AgStatus) {
            case AgentStatus.SeekAgent :
               if(navDistToTarget < circlingRange) {
                    if(timeBtwAttackTimer < Time.time) {
                        if(navDistToTarget < attackLaunchRange) {
                            LaunchSlashAttack();
                        }
                        SetDestination(agentTarget.position);
                    }
                    else {
                        Vector3 circlingIdealPos = agentTarget.position + (transform.position - agentTarget.position).normalized * circlingRange;
                        NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                        SetDestination(navPos.position);
                    }
                } else 
                SetDestination(agentTarget.position);
                LookAtDirection(agentTarget.position);
            break;

            /*case AgentStatus.CircleAgent :

            break;

            case AgentStatus.AttackAgent :
                
            break;*/

            case AgentStatus.SeekBuilding :
                if(buildingTarget == null)
                    buildingTarget = EnnemiObjective.instance.GetClosestObjective(transform.position).transform;
                if(NavMesh.SamplePosition(buildingTarget.position, out NavMeshHit hit, 10f, NavMesh.AllAreas)) {
                    SetDestination(hit.position);
            } 
            LookAtDirection(buildingTarget.position);
            break;

            /*case AgentStatus.AttackBuilding :
            break;*/
        }

        Transform posToLookAt = null;
        if(AgStatus == AgentStatus.SeekAgent)
            posToLookAt = agentTarget;
        else if (AgStatus == AgentStatus.AttackBuilding)
            posToLookAt = buildingTarget;
    }
    
    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (agentTarget.position - transform.position).normalized * attackLaunchRange / 2, atkRadius);
        foreach(Collider hit in hits) {
            if(hit.transform.GetComponent<IsTargeteable>())
                if(hit.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ally) {
                    Vector3 knockBarDir = hit.transform.position - transform.position; //
                    knockBarDir.y = 0;
                    Vector3 knockbackVector = knockBarDir.normalized * atkKnockback;
                    hit.GetComponent<HealthSystem>().TakeDamage(atkDamage, knockbackVector);
                }
        }

        timeBtwAttackTimer = Time.time + timeBtwAttack;
    }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.green; 
            Gizmos.DrawWireSphere(transform.position, viewRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, circlingRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, attackLaunchRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}
