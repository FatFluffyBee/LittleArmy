using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ennemi_Basic : Agent
{
    [Header("Behavior")]
    private Transform agentTarget;
    private float navDistToTarget;
    private Castle buildingTarget;
    [SerializeField] private float viewRange;
    [SerializeField] private float circlingRange;
    [SerializeField] private float atkRange;
    
    [Header("Agent Attack")]
    [SerializeField] private ParticleSystem slashPartSystem;
    [SerializeField] private float timeBtwAtk;
    float timeBtwAtkTimer = 0;
    [SerializeField] private float atkRadius;
    [SerializeField] private float atkKnockback;
    [SerializeField] private float atkDamage;

    [Header("Building Attack")]
    [SerializeField] private GameObject bldAtkProj;
    [SerializeField] private Transform bldAtkLaunchPoint;
    [SerializeField] private float bldAtkRange;
    [SerializeField] private float bldAtkCd;
    [SerializeField] private float bldAtkDamage;
    [SerializeField] private float bldAtkImmobilizeTime;
    [SerializeField] private float bldProjGravityModif;
    private float bldAtkCdTimer = 0;
    private float bldAtkImmobilizeTimer = 0;
    
    void Start()
    {
        Initialize(null, Color.black);
        AgStatus = AgentStatus.SeekBuilding;
    }

    void Update()
    { 
        BaseUpdate();

        //first check if need to change status
        
        List<Transform> potentialTargets = GetTargetsInViewRange(viewRange, AgentType.Ally);
        agentTarget = FindClosestTargetInNavRange(potentialTargets);

        if(agentTarget != null) { 
            navDistToTarget = NavMaths.DistBtwPoints(transform.position, agentTarget.position);
            if(navDistToTarget > viewRange) agentTarget = null;
        }

        //then apply behavior depending on current state
        switch(AgStatus) {
            case AgentStatus.SeekAgent :
                if(agentTarget == null)
                    SwitchAgentState(AgentStatus.SeekBuilding);

                if(navDistToTarget < atkRange && timeBtwAtkTimer < Time.time) { //ally in range  and ready to atk
                    SwitchAgentState(AgentStatus.Attacking);
                } 
                else if (navDistToTarget < circlingRange && timeBtwAtkTimer < Time.time) { //ally not in circling range so just avance 
                    SetDestination(agentTarget.position);
                } 
                else { //ally in circling range
                    Vector3 circlingIdealPos = agentTarget.position + (transform.position - agentTarget.position).normalized * circlingRange;
                    NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                    SetDestination(navPos.position);
                }
                
                LookAtDirection(agentTarget.position);
                feedbackMovement = true;
            break;

            case AgentStatus.Attacking :
                LaunchSlashAttack();
                SwitchAgentState(AgentStatus.SeekAgent);

                LookAtDirection(agentTarget.position);
                feedbackMovement = false;
            break;

            case AgentStatus.SeekBuilding : //Todo hmmmmmmmm weird
                if(agentTarget != null) {
                    AgStatus = AgentStatus.SeekAgent;
                }
                
                if(buildingTarget == null)
                    buildingTarget = EnnemiObjective.instance.GetClosestObjective(transform.position);

                if(NavMesh.SamplePosition(buildingTarget.transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas)) {
                    if(bldAtkCdTimer < Time.time && NavMaths.DistBtwPoints(transform.position, hit.position) < bldAtkRange) {
                        SwitchAgentState(AgentStatus.AttackBuilding);
                    }
                    else {
                        SetDestination(hit.position);
                    } 
                } 

                if(navMeshAgent.path.corners.Length > 1)
                    LookAtDirection(navMeshAgent.path.corners[1]);
                feedbackMovement = true;
            break;

            case AgentStatus.AttackBuilding :
                if(bldAtkCdTimer < Time.time) {
                    LaunchBuildingAttack();
                }
                if(bldAtkImmobilizeTimer < Time.time) {
                    SwitchAgentState(AgentStatus.SeekBuilding);
                }
                    
                LookAtDirection(buildingTarget.transform.position);
                feedbackMovement = false;
            break;
        }
    }

    private void LaunchBuildingAttack() {
        GameObject instance = Instantiate(bldAtkProj, bldAtkLaunchPoint.position, Quaternion.identity);
        instance.GetComponent<Torch>().Initialize(buildingTarget.GetComponent<Castle>());
        
        bldAtkCdTimer = bldAtkCd + bldAtkImmobilizeTime + Time.time;
        bldAtkImmobilizeTimer = bldAtkImmobilizeTime + Time.time;
    }

    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (agentTarget.position - transform.position).normalized * atkRange / 2, atkRadius);
        foreach(Collider hit in hits) {
            if(hit.transform.GetComponent<IsTargeteable>())
                if(hit.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ally) {
                    Vector3 knockBarDir = hit.transform.position - transform.position; //
                    knockBarDir.y = 0;
                    Vector3 knockbackVector = knockBarDir.normalized * atkKnockback;
                    hit.GetComponent<HealthSystem>().TakeDamage(atkDamage, knockbackVector);
                }
        }
        timeBtwAtkTimer = Time.time + timeBtwAtk;
    }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.green; 
            Gizmos.DrawWireSphere(transform.position, viewRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, circlingRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}
