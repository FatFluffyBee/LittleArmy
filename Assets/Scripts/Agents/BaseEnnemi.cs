using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Ennemi_Basic : Agent_Ennemi
{
    [Header("Behavior")]
    FiniteStateMachine stateMachine;
    [SerializeField] private string currentStateName;
    [SerializeField] private float aggroRange;
    [SerializeField] private float atkRange;
    
    [Header("Agent Attack")]
    [SerializeField] private ParticleSystem slashPartSystem;
    [SerializeField] private float reloadDuration;
    float reloadTimer = 0;
    [SerializeField] private float atkRadius;
    [SerializeField] private float atkKnockback;
    [SerializeField] private float atkDamage;
    [SerializeField] private float atkNumTargets;
    
    void Start()
    {
        Initialize();
        stateMachine = new FiniteStateMachine();

        var huntTarget = new HuntTarget(this, atkRange);
        var chargeAttack = new ChargeAttack(this, LaunchSlashAttack);
        var huntBuilding = new HuntBuilding(this, bldAtkRange);
        var attackBuilding = new AttackBuilding(this, bldAtkChargeDuration);
        
        At(huntBuilding, huntTarget, () => Target != null);
        At(huntBuilding, attackBuilding, BldInRangeAndBldReloadReady());
        At(attackBuilding, huntTarget, () => Target != null);
        At(attackBuilding, huntBuilding, () => bldAtkReloadTimer > Time.time);
        At(huntTarget, huntBuilding, () => Target == null);
        At(huntTarget, chargeAttack, TargetInAtkRangeAndReloadReady());
        At(chargeAttack, huntTarget, () => reloadTimer > Time.time);

        stateMachine.SetState(huntBuilding); 

        void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
        
        Func<bool> BldInRangeAndBldReloadReady() => () => TargetBld != null && NavDistBld < bldAtkRange 
            && bldAtkReloadTimer < Time.time;
        Func<bool> TargetInAtkRangeAndReloadReady() => () => Target != null && NavDistToTarget < atkRange && reloadTimer < Time.time;
    }

    void Update()
    { 
        BaseUpdate();
        
        Target = GetClosestTargetInRange(aggroRange, AgentType.Ally, TargetType.All, DistMode.Nav, out float navDist);
        NavDistToTarget = navDist;
        stateMachine.Tick();

        currentStateName = stateMachine.GetStateName();
    }

    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (Target.position - transform.position).normalized * atkRange / 2, atkRadius);
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

                for(int i = 0; i < ((atkNumTargets > dataTargets.Count)? dataTargets.Count : atkNumTargets); i++) { //in case there is less target to hit than the max number of agent the atk can hit
                    Vector3 knockBarDir = dataTargets[i].col.transform.position - transform.position; 
                    knockBarDir.y = 0;
                    Vector3 knockbackVector = knockBarDir.normalized * atkKnockback;
                    dataTargets[i].col.GetComponent<HealthSystem>().TakeDamage(atkDamage, knockbackVector);
                } 
            }
        }
        SetReloadTimer();
    }

    private void SetReloadTimer() {
        reloadTimer = reloadDuration + Time.time;
    }
       
       
       /*switch(currentState) {
            case AgentState.SeekAgent :
                if(agentTarget == null) {
                    SwitchAgentState(AgentState.SeekBuilding);
                }
                else {
                    if(navDistToTarget < atkRange && timeBtwAtkTimer < Time.time) { //ally in range  and ready to atk
                        SwitchAgentState(AgentState.Attacking);
                    } 
                    else if (timeBtwAtkTimer < Time.time) { //ally not in circling range so just avance 
                        CheckAndSetDestination(agentTarget.position);
                    } 
                    else { //ally in circling range
                        Vector3 circlingIdealPos = agentTarget.position + (transform.position - agentTarget.position).normalized * circlingRange;
                        NavMesh.SamplePosition(circlingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                        CheckAndSetDestination(navPos.position);
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
                        CheckAndSetDestination(hit.position);
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
        }*/
   // }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.green; 
            Gizmos.DrawWireSphere(transform.position, aggroRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, bldAtkRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}
