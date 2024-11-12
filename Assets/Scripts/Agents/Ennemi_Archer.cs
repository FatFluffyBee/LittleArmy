using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Ennemi_Archer : Agent_Ennemi, I_Range
{
    private FiniteStateMachine stateMachine;
    [SerializeField] private string currentStateName;
    [Header("Projectile Launch")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float reloadDuration;
    [SerializeField] private Vector2 timeToChargeRange;
    [SerializeField] private float atkRange;
    [SerializeField] private Vector2 deviationRange;
    [SerializeField] private Vector2 airTimeRange;
    [SerializeField] private float gravMul;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackForce;
    private float reloadTimer;
    private bool readyToFire = false;

    [Header("Targetting")]
    //private Transform disrupterTarget;
    [SerializeField] private float disruptRange = 5f;
    [SerializeField] private float confortRange;
    [SerializeField] private float confortRangeGap;
    [SerializeField] private int maxTargetRandomPick = 5;
    [SerializeField] private int stepVerifNumber = 10;
    public bool IsTargetHittable {get; set;}

    [Header("Debug Trajectory")]
    [SerializeField] private float stepDuration;
    [SerializeField] private int stepNumbers;
    private LineRenderer lineRd;
    
    void Start()
    {
        Initialize();
        stateMachine = new FiniteStateMachine();

        var seekShootingPos = new SeekShootingPos(this, atkRange, confortRange, confortRangeGap);
        var chargeAttack = new ChargeAttack(this, timeToChargeRange.x, TriggerAttack);
        var huntBuilding = new HuntBuilding(this, bldAtkRange);
        var attackBuilding = new AttackBuilding(this, bldAtkChargeDuration);
        
        At(huntBuilding, seekShootingPos, () => Target != null);
        At(huntBuilding, attackBuilding, BldInRangeAndBldReloadReady());
        At(attackBuilding, seekShootingPos, () => Target != null);
        At(attackBuilding, huntBuilding, BldReloadnotReadyOrNoBldTarget());
        At(seekShootingPos, huntBuilding, () => Target == null);
        At(seekShootingPos, chargeAttack, TargetInRangeShotValidAndReloadReady());
        At(chargeAttack, seekShootingPos, () => reloadTimer > Time.time);

        stateMachine.SetState(huntBuilding); 

        void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
        
        Func<bool> BldReloadnotReadyOrNoBldTarget() => () => TargetBld == null || bldAtkReloadTimer > Time.time;
        Func<bool> BldInRangeAndBldReloadReady() => () => TargetBld != null && NavDistBld < bldAtkRange 
            && bldAtkReloadTimer < Time.time;
        Func<bool> TargetInRangeShotValidAndReloadReady() => () => Target != null && NavDistToTarget < atkRange && reloadTimer < Time.time && IsTargetHittable;
    }

    void Update()
    { 
        BaseUpdate();
        
        if(!IsTargetValid(Target, atkRange)) {
            Target = GetRandomTargetInRange(atkRange, AgentType.Ally, TargetType.All, DistMode.View, out float viewDist, maxTargetRandomPick);
            NavDistToTarget = viewDist;
        } else {
            NavDistToTarget = Vector3.Distance(Target.position, transform.position);
        }

        if(Target != null) 
            IsTargetHittable = IsTargetHittable(transform.position, Target, atkRange, airTimeRange, gravMul, stepVerifNumber);

        //disrupterTarget = GetClosestTargetInRange(disruptRange, AgentType.Ally, TargetType.All, DistMode.Nav);
            
        stateMachine.Tick();

        currentStateName = stateMachine.GetStateName();
    }

    void FixedUpdate(){ 
        if(readyToFire){
            FireProjectile();
            Target = null;
            readyToFire = false;
        }
    }

    Action TriggerAttack => () => readyToFire = true;

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
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, bldAtkRange); 
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, confortRange); 
        }
            
    }
}
