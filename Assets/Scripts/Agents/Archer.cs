using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Archer : Agent
{
    [Header("Projectile Launch")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float reloadTime;
    [SerializeField] private Vector2 timeToChargeRange;
    [SerializeField] private float atkRange;
    [SerializeField] private Vector2 deviationRange;
    [SerializeField] private Vector2 airTimeRange;
    [SerializeField] private float gravMul;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackForce;
    private float reloadTimeTimer;
    private float timeToCharge;
    private float timeToChargeCount;
    private bool readyToFire = false;

    [Header("Targetting")]
    private Transform target;
    private Transform disrupterTarget;
    [SerializeField] private float disruptRange = 5f;
    [SerializeField] private float softMaxFleeingRange = 10f;
    [SerializeField] private int maxTargetRandomPick = 5;

    [Header("Debug Trajectory")]
    [SerializeField] private float stepDuration;
    [SerializeField] private int stepNumbers;
    private LineRenderer lineRd;
    
    void Start(){
        lineRd = GetComponent<LineRenderer>();
    }

    void Update(){ //todo recompile
        BaseUpdate();

        if(!IsTargetValid(target, atkRange))
            target = GetRandomTargetInRange(atkRange, AgentType.Ennemi, TargetType.All, DistMode.View, out float x, maxTargetRandomPick);

        /*disrupterTarget = GetClosestTargetInRange(disruptRange, AgentType.Ennemi, TargetType.All, DistMode.Nav, out float d);
        if(disrupterTarget != null && currentState != AgentState.Travelling){
            SwitchAgentState(AgentState.Fleeing);
        }*/

        switch(currentState) {
            case AgentState.Idle : //dont move but attack if in range
                if(target != null && reloadTimeTimer < Time.time) {
                    SwitchAgentState(AgentState.Attacking);
                    timeToCharge = Random.Range(timeToChargeRange.x, timeToChargeRange.y);
                    timeToChargeCount = timeToCharge + Time.time;
                }
                else if(target == null && !IsAgentAtHomePoint()) {
                    SwitchAgentState(AgentState.Travelling);
                    SetDestination(homePoint);
                }

                if(target != null)
                    LookAtDirection(target.position);
                else
                    LookAtDirection(transform.position + Vector3.forward);
                EnableAgentMovement(false);
            break;

            case AgentState.Travelling : //travelling to a new spot)
                if(IsAgentAtDestination()) {
                    SwitchAgentState(AgentState.Idle);
                }

                if(navMeshAgent.path.corners.Length > 1) 
                    LookAtDirection(navMeshAgent.path.corners[1]);
                EnableAgentMovement(true);
                break;

            /*case AgentState.Fleeing :
                if(disrupterTarget == null) {
                    SwitchAgentState(AgentState.Idle);
                    SetDestination(homePoint); 

                    LookAtDirection(homePoint);
                } else {
                    //agent run away from ennemy but not too fire away from homepoint so its not a big mess (it still is)
                    float ratioDistanceHomePointMax = Mathf.Clamp01(1 - Vector3.Distance(transform.position, homePoint) / softMaxFleeingRange);
                    Vector3 fleeingIdealPos = transform.position - (disrupterTarget.position - transform.position).normalized * (disruptRange + 1f) * ratioDistanceHomePointMax;
                    NavMesh.SamplePosition(fleeingIdealPos, out NavMeshHit navPos, 10f, NavMesh.AllAreas);
                    SetDestination(navPos.position);

                    LookAtDirection(disrupterTarget.position);
                }
                EnableAgentMovement(true);
            break;*/

            case AgentState.Attacking :
                if(target == null) {
                    SwitchAgentState(AgentState.Idle);
                } 
                else {
                    if(timeToChargeCount < Time.time) {
                        readyToFire = true;
                    }

                    LookAtDirection(target.position);
                }  
                EnableAgentMovement(false);
            break;
        }  
    }

    void FixedUpdate(){ 
        if(readyToFire){
            LaunchAttack();
        }
    }

    void LaunchAttack() {
        if(target == null) {
            readyToFire = false; //cause we fire on fixedupdate there can be dissonance where the target is outrange the next frame before and so it create infinite error loop
            return;
        } 

        FireProjectile();
        reloadTimeTimer = Time.time + reloadTime;
        readyToFire = false;
        SwitchAgentState(AgentState.Idle);
        target = null; //reset target so it can focus an another one closer if needed
    }

    void FireProjectile(){
        Vector3 initialVelocity = TrajMaths.GetInitVelocityForBellCurveFromRangeValue(launchPoint.position, target, atkRange, airTimeRange, gravMul, deviationRange, TrajMode.PredictionPos);

        if(debug){
            Vector3 pos = launchPoint.position;
            Vector3 velocity = initialVelocity;
            lineRd.positionCount = stepNumbers+1;
            lineRd.SetPosition(0, pos);
            for(int i = 0; i < stepNumbers; i++){
                pos += velocity * stepDuration;
                velocity += Physics.gravity * stepDuration * gravMul;
                
                lineRd.SetPosition(i+1, pos);
            }
        }

        GameObject instance = Instantiate(arrowPrefab, launchPoint.transform.position, Quaternion.identity);
        instance.GetComponent<Rigidbody>().velocity = initialVelocity;
        instance.GetComponent<GravityAmplifier>().Initialize(gravMul);
        instance.GetComponent<Projectile>().Initialize(damage, knockbackForce, transform.position, AgentType.Ennemi);
    }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, disruptRange); 
        }
            
    }
}

