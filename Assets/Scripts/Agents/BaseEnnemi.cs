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
    [SerializeField] private float aggroRange;
    [SerializeField] private float circlingRange;
    [SerializeField] private float atkRange;
    
    [Header("Agent Attack")]
    [SerializeField] private ParticleSystem slashPartSystem;
    [SerializeField] private float timeBtwAtk;
    float timeBtwAtkTimer = 0;
    [SerializeField] private float atkRadius;
    [SerializeField] private float atkKnockback;
    [SerializeField] private float atkDamage;
    [SerializeField] private float atkNumTargets;

    [Header("Building Attack")]
    [SerializeField] private GameObject bldAtkProj;
    [SerializeField] private Transform bldAtkLaunchPoint;
    [SerializeField] private float bldAtkRange;
    [SerializeField] private float bldAtkCd;
    [SerializeField] private float bldAtkDamage;
    [SerializeField] private float bldAtkChargeTime;
    [SerializeField] private float bldProjGravityModif;
    private float bldAtkChargeTimer = 0;
    private float bldAtkCdTimer = 0;
    
    void Start()
    {
        Initialize(null, Color.black);
        currentState = AgentState.SeekBuilding;
    }

    void Update()
    { 
        BaseUpdate();
        
        agentTarget = GetClosestTargetInRange(aggroRange, AgentType.Ally, TargetType.All, DistMode.Nav, out navDistToTarget);

        switch(currentState) {
            case AgentState.SeekAgent :
                if(agentTarget == null) {
                    SwitchAgentState(AgentState.SeekBuilding);
                }
                else {
                    if(navDistToTarget < atkRange && timeBtwAtkTimer < Time.time) { //ally in range  and ready to atk
                        SwitchAgentState(AgentState.Attacking);
                    } 
                    else if (timeBtwAtkTimer < Time.time) { //ally not in circling range so just avance 
                        SetDestination(agentTarget.position);
                    } 
                    else { //ally in circling range
                        Vector3 circlingIdealPos = agentTarget.position + (transform.position - agentTarget.position).normalized * circlingRange;
                        NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                        SetDestination(navPos.position);
                     }
                }
                    
                if(agentTarget != null)
                    LookAtDirection(agentTarget.position);
                EnableAgentMovement(true);
            break;

            case AgentState.Attacking :
                LaunchSlashAttack();
                SwitchAgentState(AgentState.SeekAgent);

                LookAtDirection(agentTarget.position);
                EnableAgentMovement(false);
            break;

            case AgentState.SeekBuilding :
                if(agentTarget != null) {
                    currentState = AgentState.SeekAgent;
                }
                
                if(buildingTarget == null)
                    buildingTarget = EnnemiObjective.instance.GetClosestObjective(transform.position);

                if(NavMesh.SamplePosition(buildingTarget.transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas)) {
                    if(bldAtkCdTimer < Time.time && NavMaths.DistBtwPoints(transform.position, hit.position) < bldAtkRange) {
                        SwitchAgentState(AgentState.AttackBuilding);
                        bldAtkChargeTimer = bldAtkChargeTime + Time.time;
                    }
                    else {
                        SetDestination(hit.position);
                    } 
                } 

                if(navMeshAgent.path.corners.Length > 1)
                    LookAtDirection(navMeshAgent.path.corners[1]);
                EnableAgentMovement(true);
            break;

            case AgentState.AttackBuilding :
                if(buildingTarget == null) {
                        SwitchAgentState(AgentState.SeekBuilding);
                }
                else {
                    if(agentTarget != null) {
                        currentState = AgentState.SeekAgent;
                    }

                    if(bldAtkChargeTimer < Time.time) {
                        LaunchBuildingAttack();
                        SwitchAgentState(AgentState.SeekBuilding);
                    }

                    LookAtDirection(buildingTarget.transform.position); 
                    EnableAgentMovement(false);
                }
            break;
        }
    }

    private void LaunchBuildingAttack() {
        GameObject instance = Instantiate(bldAtkProj, bldAtkLaunchPoint.position, Quaternion.identity);
        instance.GetComponent<Torch>().Initialize(buildingTarget.GetComponent<Castle>());
        
        bldAtkCdTimer = bldAtkCd + Time.time;
    }

    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (agentTarget.position - transform.position).normalized * atkRange / 2, atkRadius);
        if(hits.Length > 0) {
            List<DataTarget> dataTargets = new List<DataTarget>();
            foreach(Collider hit in hits) {
                if(hit.transform.GetComponent<IsTargeteable>())
                    if(hit.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ally) {
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
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, circlingRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}
