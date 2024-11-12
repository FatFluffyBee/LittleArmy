using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ennemi_Basic : Agent_Ennemi
{
    [Header("Behavior")]
    private FiniteStateMachine stateMachine;
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

    [Header("Shield")]
    [SerializeField] private bool asShield;
    [SerializeField] private Shield shield; 
    [SerializeField] private float shieldViewRange;
    public Transform TargetShield {get;set;}
    
    void Start()
    {
        Initialize();

        if(!asShield) {
            shield?.gameObject.SetActive(false);
        }

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

        if(asShield && shield != null) {
            UpdateShield();
        }
        
        stateMachine.Tick();
        currentStateName = stateMachine.GetStateName();
    }

    private void UpdateShield() {
        TargetShield = GetClosestTargetInRange(shieldViewRange, AgentType.Ally, TargetType.Range, DistMode.View);
        if(TargetShield != null)
            shield.RaiseShield();
        else
            shield.LowerShield();
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

    private void OnDrawGizmos() {
        if(debug) {
            Gizmos.color = Color.green; 
            Gizmos.DrawWireSphere(transform.position, aggroRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, bldAtkRange); //Draw range
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, atkRange); //Draw range
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(transform.position, shieldViewRange); //Draw range
            if(Application.isPlaying)
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        }    
    }
}
