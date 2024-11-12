using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent_Ennemi : Agent
{
    [Header("Building Attack")]
    [SerializeField] protected GameObject bldAtkProj;
    [SerializeField] protected Transform bldAtkLaunchPoint;
    [SerializeField] protected float bldAtkRange;
    [SerializeField] protected float bldAtkReloadDuration;
    [SerializeField] protected float bldAtkDamage;
    [SerializeField] protected float bldAtkChargeDuration;
    [SerializeField] protected float bldProjGravityModif;
    protected float bldAtkReloadTimer = 0;
    public float NavDistBld {get; set;}
    public Castle TargetBld {get; set;}
    protected Vector3 targetBldPos;

    public override void BaseUpdate() {
        if(TargetBld != null)
            NavDistBld = GetNavDistBld();
        else
            NavDistBld = float.MaxValue;

        base.BaseUpdate();
    }

    public void GetNewBuildingTarget() {
        TargetBld = EnnemiObjective.instance.GetClosestObjective(transform.position);
        targetBldPos = TargetBld.GetClosestPosition();
        NavDistBld = GetNavDistBld();
    }

    public void LaunchBuildingAttack() {
        GameObject instance = Instantiate(bldAtkProj, bldAtkLaunchPoint.position, Quaternion.identity);
        instance.GetComponent<Torch>().Initialize(TargetBld);

        SetBldReloadTimer();
    }

    public void SetBldReloadTimer() => bldAtkReloadTimer = bldAtkReloadDuration + Time.time;

    public float GetNavDistBld() {
        return Vector3.Distance(TargetBld.transform.position, transform.position);
    }
}
