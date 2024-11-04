using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
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

    public static Vector3 GetInitVelocityForBellCurveFromRangeValue(Vector3 startPos, Transform target, float maxRange, Vector2 airTimeRange, float gravMul, 
         Vector2 deviationRange, TrajMode trajMode) {
        return GetInitVelocityForBellCurveFromRangeValue(startPos, target, maxRange, airTimeRange, gravMul, deviationRange, trajMode, out float a);
    }

    public static Vector3 GetInitVelocityForBellCurveFromRangeValue(Vector3 startPos, Transform target, float maxRange, Vector2 airTimeRange,
     float gravMul, Vector2 deviationRange, TrajMode trajMode, out float airTimeFromDist) {
        float range01 = Vector3.Distance(target.transform.position, startPos) / maxRange;
        airTimeFromDist = airTimeRange.x + (airTimeRange.y - airTimeRange.x) * range01; //retourne le temps que met la flèche pour attendre sa cible en fonction de la distance à celle-ci
        float deviationFromDist = deviationRange.x + (deviationRange.y - deviationRange.x) * range01; 

        float vertDiff = Mathf.Abs(target.transform.position.y - startPos.y);
        float verticalRatio = Mathf.Max(1, 1 + vertDiff / TerrainGrid.GRID_SCALE * 1 / range01 * 0.33f);
        airTimeFromDist *= verticalRatio; //without this arrow between high and low vertical value will hit walls

        Vector3 targetPos;
        if(trajMode == TrajMode.PredictionPos)
            targetPos = target.GetComponent<IsTargeteable>().GetPredictedPos(airTimeFromDist);
        else 
            targetPos = target.transform.position;

        return InitialVelocityForBellTrajectory(startPos, targetPos, airTimeFromDist, deviationFromDist, gravMul);
    }
}

 public enum TrajMode {PredictionPos, ActualPos};
