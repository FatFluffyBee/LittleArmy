using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Archer : Agent
{
    [SerializeField] private string currentStateName;
    private FiniteStateMachine stateMachine;
    [Header("Projectile Launch")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float reloadDuration;
    private float reloadTimer;
    [SerializeField] private Vector2 timeToChargeRange;
    [SerializeField] private float atkRange;
    [SerializeField] private Vector2 deviationRange;
    [SerializeField] private Vector2 airTimeRange;
    [SerializeField] private float gravMul;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackForce;

    private bool readyToFire = false;

    [Header("Targetting")]
    private Transform disrupterTarget;
    [SerializeField] private float disruptRange = 5f;
    [SerializeField] private float softMaxFleeingRange = 10f;
    [SerializeField] private int maxTargetRandomPick = 5;

    [Header("Debug Trajectory")]
    [SerializeField] private float stepDuration;
    [SerializeField] private int stepNumbers;
    private LineRenderer lineRd;
    
    void Awake(){
        lineRd = GetComponent<LineRenderer>();

        Initialize();
        stateMachine = new FiniteStateMachine();

        var idle = new Idle(this);
        var returnHome = new ReturnHome(this);
        var chargeAttack = new ChargeAttack(this, timeToChargeRange.x, TMPTriggerFire);
        var prioMovement = new PrioMovement(this);
        
        At(idle, chargeAttack, AsTargetAndReloadReady());
        At(chargeAttack, idle, AsNoTargeOrReloadNotReady());
        At(idle, returnHome, AsNotReachedHomePoint());
        At(returnHome, idle, AsReachedHomePoint());
        At(returnHome, chargeAttack, AsTargetAndReloadReady());
        At(prioMovement, idle, AsReachedHomePoint());

        stateMachine.SetState(idle); 

        stateMachine.AddAnyTransition(prioMovement, () => AsBeenMoveOrdered);

        void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
        Func<bool> AsReachedHomePoint() => () => IsAgentAtHomePoint();
        Func<bool> AsNotReachedHomePoint() => () => !IsAgentAtHomePoint();
        Func<bool> AsTargetAndReloadReady() => () => Target != null && reloadTimer < Time.time;
        Func<bool> AsNoTargeOrReloadNotReady() => () => Target == null || reloadTimer > Time.time;
    }

    void Update(){ //todo recompile
        BaseUpdate();

        if(!IsTargetValid(Target, atkRange))
            Target = GetRandomTargetInRange(atkRange, AgentType.Ennemi, TargetType.All, DistMode.View, out float x, maxTargetRandomPick);

        stateMachine.Tick();

        currentStateName = stateMachine.GetStateName();
        Debug.Log(AsBeenMoveOrdered);
    }

    Action TMPTriggerFire => () => readyToFire = true;

    void FixedUpdate(){ 
        if(readyToFire){
            FireProjectile();
            Target = null;
            readyToFire = false;
        }
    }

    void FireProjectile(){
        Vector3 initialVelocity = TrajMaths.GetInitVelocityForBellCurveFromRangeValue(launchPoint.position, Target, atkRange, airTimeRange, gravMul, deviationRange, TrajMode.PredictionPos);

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

        SetReloadTimer();
    }

     private void SetReloadTimer() {
        reloadTimer = reloadDuration + Time.time;
    }

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, disruptRange); 
        }   
    }

    /*private void Flee() {
        disrupterTarget = GetClosestTargetInRange(disruptRange, AgentType.Ennemi, TargetType.All, DistMode.Nav, out float d);
        if(disrupterTarget != null && currentState != AgentState.Travelling){
            SwitchAgentState(AgentState.Fleeing);
        }
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
    }*/
}

