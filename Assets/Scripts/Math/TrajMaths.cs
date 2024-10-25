using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TrajMaths 
{
    public static Vector3 InitialVelocityForBellTrajectory(Vector3 startPos, Vector3 targetPos, float travelTime, float deviation, float gravityMult)
    {
        //Randomize a little the velocity vectors 
        Vector3 targetDeviation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        targetDeviation = targetDeviation.magnitude > 1? targetDeviation.normalized : targetDeviation;
        targetDeviation *= deviation;
        targetPos += targetDeviation;

        Vector3 newGravity = Physics.gravity * gravityMult; 
        Vector3 movToTarget = targetPos - startPos;
        movToTarget.y -= gravityMult / 9.81f; //! I dont know why but I need this otherwise the gravity becomes really weird
        movToTarget /= travelTime;

        float gravityCancel =  - 0.5f * newGravity.y * travelTime;
        movToTarget.y += gravityCancel;

        Vector3 finalVelocity = movToTarget;
        return finalVelocity;   
    }


}
