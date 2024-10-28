using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Archer : Agent
{
    [Header("Projectile Launch")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float reloadTime;
    [SerializeField] private Vector2 timeToChargeRange;
    [SerializeField] private float atkRange;
    [SerializeField] private float deviation;
    [SerializeField] private Vector2 airTimeRange;
    [SerializeField] private float gravityMul;
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

    [Header("Debug Trajectory")]
    private LineRenderer lineRd;
    public float stepDuration;
    public int stepNumbers;
    
    void Start(){
        lineRd = GetComponent<LineRenderer>();
    }
    void Update(){ //todo recompile
        BaseUpdate();

        List<DataTarget> potentialTargets = GetDataTargetsInViewRange(atkRange, AgentType.Ennemi);
        potentialTargets = OrderDataTargetsByDist(potentialTargets);
        if(potentialTargets.Count > 0) {
            target = potentialTargets[0].col.transform;
        }

        List<DataTarget> potentialThreats = GetDataTargetsInViewRange(disruptRange, AgentType.Ennemi); 
        DataTarget disruptDataTarget = FindClosestTargetInNavRange(potentialThreats);
        
        if(disruptDataTarget.col != null)
            if(disruptDataTarget.dist < disruptRange && AgStatus != AgentStatus.Travelling) {
                SwitchAgentState(AgentStatus.Fleeing);
                disrupterTarget = disruptDataTarget.col.transform;
            }
            else {
                disrupterTarget = null;
            }
        else {
            disrupterTarget = null;
        }

        switch(AgStatus) {
            case AgentStatus.Idle : //dont move but attack if in range
                if(target != null && reloadTimeTimer < Time.time) {
                    SwitchAgentState(AgentStatus.Charging);
                    timeToCharge = Random.Range(timeToChargeRange.x, timeToChargeRange.y);
                    timeToChargeCount = timeToCharge + Time.time;
                }
                else if(target == null && !IsAgentAtHomePoint()) {
                    SwitchAgentState(AgentStatus.Travelling);
                    SetDestination(homePoint);
                }

                LookAtDirection(transform.position + Vector3.forward);
                EnableAgentMovement(false);
            break;

            case AgentStatus.Travelling : //travelling to a new spot)
                if(IsAgentAtDestination()) {
                    SwitchAgentState(AgentStatus.Idle);
                }

                if(navMeshAgent.path.corners.Length > 1) LookAtDirection(navMeshAgent.path.corners[1]);
                EnableAgentMovement(true);
                break;

            case AgentStatus.Fleeing :
                if(disrupterTarget == null)
                    SwitchAgentState(AgentStatus.Idle);
                    SetDestination(homePoint);
                EnableAgentMovement(true);
            break;

            case AgentStatus.Charging :
                if(potentialTargets.Count == 0)
                    SwitchAgentState(AgentStatus.Idle);

                if(timeToChargeCount < Time.time) {
                    SwitchAgentState(AgentStatus.Attacking);
                }
                EnableAgentMovement(false);
            break;

            case AgentStatus.Attacking : //attack the ennemy
                readyToFire = true;
                SwitchAgentState(AgentStatus.Idle);
                EnableAgentMovement(false);
            break;
        }  
    }

    void FixedUpdate(){ 
        if(readyToFire){
            FireProjectile();
            reloadTimeTimer = Time.time + reloadTime;
            readyToFire = false;
        }
    }

    void FireProjectile(){
        float range01 = Vector3.Distance(target.transform.position, launchPoint.position) / atkRange;
        float airTimeFromDist = airTimeRange.x + (airTimeRange.y - airTimeRange.x) * range01; //retourne le temps que met la flèche pour attendre sa cible en fonction de la distance à celle-ci

        Vector3 ennemiFuturePos = target.GetComponent<Agent>().GetPredictedPos(airTimeFromDist);

        Vector3 initialVelocity = TrajMaths.InitialVelocityForBellTrajectory(launchPoint.position, ennemiFuturePos, airTimeFromDist, deviation, gravityMul);

        if(debug){
            Vector3 pos = launchPoint.position;
            Vector3 velocity = initialVelocity;
            lineRd.positionCount = stepNumbers+1;
            lineRd.SetPosition(0, pos);
            for(int i = 0; i < stepNumbers; i++)        {
                pos += velocity * stepDuration;
                velocity += Physics.gravity * stepDuration * gravityMul;
                
                lineRd.SetPosition(i+1, pos);
            }
        }

        GameObject instance = Instantiate(arrowPrefab, launchPoint.transform.position, Quaternion.identity);
        instance.GetComponent<Rigidbody>().velocity = initialVelocity;
        instance.GetComponent<GravityAmplifier>().Initialize(gravityMul);
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

