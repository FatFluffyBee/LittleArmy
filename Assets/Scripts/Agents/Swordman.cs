using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordman : Agent
{
    [SerializeField] private string currentStateName;
    private FiniteStateMachine stateMachine;
    [SerializeField] private float aggroRange = 20f;
    [SerializeField] private float maxRangeFromHomePoint;
   
    [Header("Attack")]
    [SerializeField] private float atkNumTargets = 1;
    [SerializeField] private float atkRange;
    [SerializeField] private float atkRadius;
    [SerializeField] private float atkDamage;
    [SerializeField] private float atkKnockback;
    [SerializeField] float reloadDuration;
    private float reloadTimer;
    [SerializeField] private ParticleSystem slashPartSystem;

    void Awake() 
    {
        Initialize();
        stateMachine = new FiniteStateMachine();

        var idle = new Idle(this);
        var huntTarget = new HuntTarget(this, atkRange);
        var prioMovement = new PrioMovement(this);
        var returnHome = new ReturnHome(this);
        var chargeAttack = new ChargeAttack(this, LaunchSlashAttack);
        
        At(idle, huntTarget, () => Target != null);
        At(returnHome, huntTarget, AsTargetAndCloseToHome());
        At(huntTarget, returnHome, AsNoTargetOrTooFarFromHome());
        At(returnHome, idle, AsReachedHomePoint());
        At(idle, returnHome, AsNotReachedHomePoint());
        At(huntTarget, chargeAttack, TargetInAtkRangeAndReloadReady());
        At(chargeAttack, huntTarget, () => reloadTimer > Time.time);
        //At(prioMovement, idle, AsReachedHomePoint());

        //stateMachine.AddAnyTransition(prioMovement, MoveOrderGivenAndOutOfCombat());

        stateMachine.SetState(idle); 

        void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
        Func<bool> AsReachedHomePoint() => () => IsAgentAtHomePoint();
        Func<bool> AsNotReachedHomePoint() => () => !IsAgentAtHomePoint();
        //Func<bool> MoveOrderGivenAndOutOfCombat() => () => AsBeenMoveOrdered && !InCombat;
        Func<bool> TargetInAtkRangeAndReloadReady() => () => Target != null && NavDistToTarget < atkRange && reloadTimer < Time.time;
        Func<bool> AsNoTargetOrTooFarFromHome() => () => NavDistToHome > maxRangeFromHomePoint || Target == null;
        Func<bool> AsTargetAndCloseToHome() => () => NavDistToHome < maxRangeFromHomePoint && Target != null;
    }

    void Update(){ 
        BaseUpdate();
        Target = GetClosestTargetInRange(aggroRange, AgentType.Ennemi, TargetType.All, DistMode.Nav, out float navDist);
        NavDistToTarget = navDist;
        if(Target != null) NavDistToHome = NavMaths.DistBtwPoints(Target.position, HomePoint);

        stateMachine.Tick();

        currentStateName = stateMachine.GetStateName();
        Debug.DrawLine(transform.position + Vector3.up * 5, HomePoint + Vector3.up * 5, Color.yellow);
    }

    private void LaunchSlashAttack() {
        slashPartSystem.Stop();
        slashPartSystem.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position + (Target.position - transform.position).normalized * atkRange / 2, atkRadius);
        if(hits.Length > 0) {
            List<DataTarget> dataTargets = new List<DataTarget>();
            foreach(Collider hit in hits) {
                if(hit.transform.GetComponent<IsTargeteable>())
                    if(hit.transform.GetComponent<IsTargeteable>().agentType == AgentType.Ennemi) {
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

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.green; 
            Gizmos.DrawWireSphere(transform.position, aggroRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, maxRangeFromHomePoint); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}

