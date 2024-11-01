using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ennemi_Archer : Agent
{
    [Header("Projectile Launch")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float reloadTime;
    [SerializeField] private Vector2 timeToChargeRange;
    [SerializeField] private float atkRange;
    [SerializeField] private Vector2 deviationRange;
    [SerializeField] private Vector2 airTimeRange;
    [SerializeField] private Vector2 gravityMulRange;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackForce;
    private float reloadTimeTimer;
    private float timeToCharge;
    private float timeToChargeCount;
    private bool readyToFire = false;

    [Header("Targetting")]
    private Transform agentTarget;
    private float distToTarget;
    private Transform disrupterTarget;
    [SerializeField] private float disruptRange = 5f;
    [SerializeField] private float confortRange;
    [SerializeField] private float confortRangeGap;
    [SerializeField] private int maxTargetRandomPick = 5;

    [Header("Building Attack")]
    [SerializeField] private GameObject bldAtkProj;
    [SerializeField] private Transform bldAtkLaunchPoint;
    [SerializeField] private float bldAtkRange;
    [SerializeField] private float bldAtkCd;
    [SerializeField] private float bldAtkDamage;
    [SerializeField] private float bldAtkChargeTime;
    [SerializeField] private float bldProjGravityModif;
    private Castle buildingTarget;
    private float bldAtkChargeTimer = 0;
    private float bldAtkCdTimer = 0;

    [Header("Debug Trajectory")]
    [SerializeField] private float stepDuration;
    [SerializeField] private int stepNumbers;
    private LineRenderer lineRd;
    
    void Start()
    {
        Initialize(null, Color.black);
        currentState = AgentState.SeekBuilding;
    }

    // Update is called once per frame
    void Update()
    {
        BaseUpdate();

        if(!IsTargetValid(agentTarget, atkRange))
            agentTarget = GetRandomTargetInRange(atkRange, AgentType.Ally, TargetType.All, DistMode.View, out distToTarget, maxTargetRandomPick);
        else {
            distToTarget = Vector3.Distance(agentTarget.position, transform.position);
        }

        disrupterTarget = GetClosestTargetInRange(disruptRange, AgentType.Ally, TargetType.All, DistMode.Nav);
        if(disrupterTarget != null){
            SwitchAgentState(AgentState.Fleeing);
        }

        switch(currentState) {
            case AgentState.SeekBuilding:
                DoSeekBuilding();
            break;

            case AgentState.AttackBuilding:
                DoAttackBuilding();
            break;

            case AgentState.SeekAgent:
                DoSeekAgent();
            break;

            case AgentState.AttackAgent:
                DoAttacking();
            break;

            case AgentState.Fleeing:
                DoFleeing();
            break;
        }
    }

    void FixedUpdate(){ 
        if(readyToFire){
            LaunchAttack();
        }
    }

    void DoSeekBuilding() {
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
    }

    void DoAttackBuilding() {
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
}

    void DoSeekAgent() {
         if(agentTarget == null) {
            currentState = AgentState.SeekBuilding;
            return;
        } 
        
        if(distToTarget > atkRange) {
            SetDestination(agentTarget.position);
        }
        else if(reloadTimeTimer < Time.time && distToTarget < atkRange){
            currentState = AgentState.AttackAgent;
            timeToCharge = Random.Range(timeToChargeRange.x, timeToChargeRange.y);
            timeToChargeCount = timeToCharge + Time.time;
        } else if(distToTarget < confortRange - confortRangeGap || distToTarget > confortRange + confortRangeGap){
            Vector3 confortIdealPos = transform.position - (agentTarget.position - transform.position).normalized * confortRange;
            NavMesh.SamplePosition(confortIdealPos, out NavMeshHit navPos, atkRange, NavMesh.AllAreas);
            SetDestination(navPos.position);
        }

        LookAtDirection(agentTarget.position);
        EnableAgentMovement(true);
    }

    void DoAttacking() {
        if(agentTarget == null) {
            SwitchAgentState(AgentState.SeekBuilding);
        } 
        else {
            if(timeToChargeCount < Time.time) {
                readyToFire = true;
            }

            LookAtDirection(agentTarget.position);
        }  
        EnableAgentMovement(false);
    }

    void DoFleeing() {
        if(disrupterTarget == null) {
            SwitchAgentState(AgentState.SeekAgent);
        } else {       
            Vector3 fleeingIdealPos = transform.position - (disrupterTarget.position - transform.position).normalized * (disruptRange + 1f);
            NavMesh.SamplePosition(fleeingIdealPos, out NavMeshHit navPos, atkRange, NavMesh.AllAreas);
            SetDestination(navPos.position);

            LookAtDirection(disrupterTarget.position);
        }
        EnableAgentMovement(true);
    }

    void LaunchAttack() {
        if(agentTarget == null) {
            readyToFire = false; //cause we fire on fixedupdate there can be dissonance where the target is outrange the next frame before and so it create infinite error loop
            return;
        } 

        FireProjectile();
        reloadTimeTimer = Time.time + reloadTime;
        readyToFire = false;
        SwitchAgentState(AgentState.SeekAgent);
        agentTarget = null; //reset target so it can focus an another one closer if needed
    }

    void FireProjectile(){
        float range01 = Vector3.Distance(agentTarget.transform.position, launchPoint.position) / atkRange;
        float airTimeFromDist = airTimeRange.x + (airTimeRange.y - airTimeRange.x) * range01; //retourne le temps que met la flèche pour attendre sa cible en fonction de la distance à celle-ci
        float gravityMulFromDist = gravityMulRange.x + (gravityMulRange.y - gravityMulRange.x) * range01; 
        float deviationFromDist = deviationRange.x + (deviationRange.y - deviationRange.x) * range01; 

        Vector3 ennemiFuturePos = agentTarget.GetComponent<Agent>().GetPredictedPos(airTimeFromDist);

        Vector3 initialVelocity = TrajMaths.InitialVelocityForBellTrajectory(launchPoint.position, ennemiFuturePos, airTimeFromDist, deviationFromDist, gravityMulFromDist);

        if(debug){
            Vector3 pos = launchPoint.position;
            Vector3 velocity = initialVelocity;
            lineRd.positionCount = stepNumbers+1;
            lineRd.SetPosition(0, pos);
            for(int i = 0; i < stepNumbers; i++)        {
                pos += velocity * stepDuration;
                velocity += Physics.gravity * stepDuration * gravityMulFromDist;
                
                lineRd.SetPosition(i+1, pos);
            }
        }

        GameObject instance = Instantiate(arrowPrefab, launchPoint.transform.position, Quaternion.identity);
        instance.GetComponent<Rigidbody>().velocity = initialVelocity;
        instance.GetComponent<GravityAmplifier>().Initialize(gravityMulFromDist);
        instance.GetComponent<Projectile>().Initialize(damage, knockbackForce, transform.position, AgentType.Ennemi);
    }

    private void LaunchBuildingAttack() {
        GameObject instance = Instantiate(bldAtkProj, bldAtkLaunchPoint.position, Quaternion.identity);
        instance.GetComponent<Torch>().Initialize(buildingTarget.GetComponent<Castle>());
        
        bldAtkCdTimer = bldAtkCd + Time.time;
    }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, disruptRange); 
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, bldAtkRange); 
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, confortRange); 
        }
            
    }
}
