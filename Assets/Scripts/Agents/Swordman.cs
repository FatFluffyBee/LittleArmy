using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Swordman : Agent
{
   [SerializeField] private float travellingViewRange;
   [SerializeField] private float idleViewRange;
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
    public TargettingType targettingType = TargettingType.First;
    public float timeBtwTargetCheck = 0.2f;

    private float navDistToTarget;
    
    void Update(){ //todo recompile
        BaseUpdate();

        List<Transform> potentialTargets = GetTargetsInViewRange(idleViewRange, AgentType.Ennemi);
        target = FindClosestTargetInNavRange(potentialTargets);
        if(target != null)
            navDistToTarget = NavMaths.DistBtwPoints(transform.position, target.position);
        else 
            navDistToTarget = 0;

        float aggroRange = 0;
        switch(AgStatus) {
            case AgentStatus.Travelling :
                aggroRange = travellingViewRange;
            break;

            case AgentStatus.Idle :
                aggroRange = idleViewRange;
            break;
        }

        if(target != null)
            if(navDistToTarget < aggroRange) {
                if(timeBtwAtkTimer < Time.time) {
                    if(navDistToTarget < atkRange) {
                        LaunchSlashAttack();
                    }
                    else {
                        SetDestination(target.position);
                    }    
                }
                else {
                    Vector3 circlingIdealPos = target.position + (transform.position - target.position).normalized * circlingRange;
                    NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                    SetDestination(navPos.position);
                }
            }
            else {
                GiveMoveOrder(homePoint);
            }
        UpdateRotation(circlingRange, navDistToTarget, target);
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
            Gizmos.DrawWireSphere(transform.position, idleViewRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, travellingViewRange); //Draw range
            Gizmos.color = Color.blue; 
            Gizmos.DrawWireSphere(transform.position, circlingRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}

